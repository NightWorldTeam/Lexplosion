using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Threading;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using Lexplosion.Tools;

namespace Lexplosion.Logic.Management
{
    using JavaVersionsFile = List<JavaVersion>;

    class JavaChecker : IDisposable
    {
        public enum CheckResult
        {
            Successful,
            DefinitionError
        }

        private static readonly KeySemaphore<string> _downloadSemaphore = new();
        private static readonly object _versionsManifestLock = new object();

        private bool _semIsReleased = false;
        private string _javaName;
        private JavaVersion _thisJava = null;
        private JavaVersionsFile _versionsFile;
        private CancellationToken _cancelToken;
        private Dictionary<string, JavaFiles.Unit> _updateList = new();

        private const string MANIFEST_FILE = "/java/javaVersionsManifest.json";
        private const string FILES_DIR = "/java/versions/";
        private const string VERSIONS_MANIFEST_DIR = "/java/versionsManifests/";

        public JavaChecker(string javaName, CancellationToken cancelToken, bool isReleased = false)
        {
            _javaName = !string.IsNullOrWhiteSpace(javaName) ? javaName : "jre-legacy";
            _cancelToken = cancelToken;
            _semIsReleased = isReleased;
        }

        public JavaVersion GetJavaInfo()
        {
            try
            {
                // получаем файл с версиями джавы
                lock (_versionsManifestLock)
                {
                    _versionsFile = DataFilesManager.GetFile<JavaVersionsFile>(WithDirectory.DirectoryPath + MANIFEST_FILE);
                }

                if (_versionsFile == null)
                {
                    return null;
                }

                JavaVersion javaInfo = null;

                //ищем нужную версию джавы
                foreach (JavaVersion javaVer in _versionsFile)
                {
                    if (javaVer.JavaName == _javaName)
                    {
                        javaInfo = javaVer;
                        break;
                    }
                }

                return javaInfo;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Определяет путь до джавы для этой версии майкрафта (по имени джавы, которую эта версия использует)
        /// Метод произведет проверку версии и вернет либо path1 либо path2.
        /// </summary>
        /// <param name="path1">Путь до старой джавы</param>
        /// <param name="path2">Путь до новой джавы</param>
        /// <returns>Для старых версий будет возвращен path1, для новых - path2</returns>
        public static string DefinePath(string path1, string path2, string javaName)
        {
            return (javaName == "jre-legacy") ? path1 : path2;
        }

        public bool Check(out CheckResult result, out JavaVersion java)
        {
            // для начала в локальном списке ищем нужную нам джаву
            _thisJava = GetJavaInfo();

            Runtime.DebugWrite("javaName: " + _javaName + ", _thisJava is null " + (_thisJava == null));


            if (_thisJava?.JavaName != null) //нашли
            {
                result = CheckResult.Successful;
            }
            else //не нашли, получаем данные с сервера
            {
                JavaVersionManifest versions = ToServer.GetJavaVersions();

                if (versions == null)
                {
                    Runtime.DebugWrite("versions list is null");
                    java = null;
                    result = CheckResult.DefinitionError;
                    return false;
                }

                _versionsFile = new JavaVersionsFile();

                //ищем нужную дажву
                foreach (string javaVerName in versions.GetWindowsActual.Keys)
                {
                    if (javaVerName == _javaName)
                    {
                        _thisJava = new JavaVersion(javaVerName, versions.GetWindowsActual[javaVerName]);
                        _versionsFile.Add(_thisJava);
                    }
                    else
                    {
                        _versionsFile.Add(new JavaVersion(javaVerName, versions.GetWindowsActual[javaVerName]));
                    }
                }

                lock (_versionsManifestLock)
                {
                    DataFilesManager.SaveFile(WithDirectory.DirectoryPath + MANIFEST_FILE, JsonConvert.SerializeObject(_versionsFile));
                }
            }

            java = _thisJava;
            if (_thisJava?.JavaName == null)
            {
                Runtime.DebugWrite("_thisJava is null " + (_thisJava == null));
                result = CheckResult.DefinitionError;
                return false;
            }

            _downloadSemaphore.WaitOne(_thisJava.JavaName);

            var fileDir = WithDirectory.DirectoryPath + VERSIONS_MANIFEST_DIR + _thisJava.JavaName + ".json";
            var javaFiles = DataFilesManager.GetFile<JavaFiles>(fileDir);

            if (javaFiles?.Files == null || javaFiles.Files.Count < 1)
            {
                string manifestString = ToServer.HttpGet(_thisJava.ManifestUrl);
                try
                {
                    javaFiles = JsonConvert.DeserializeObject<JavaFiles>(manifestString);
                }
                catch { }

                if (javaFiles?.Files == null || javaFiles.Files.Count < 1)
                {
                    result = CheckResult.DefinitionError;
                    return false;
                }

                DataFilesManager.SaveFile(fileDir, manifestString);
            }

            string javaBaseDir = WithDirectory.DirectoryPath + FILES_DIR + _thisJava.JavaName + "/";
            foreach (var fileName in javaFiles.Files.Keys)
            {
                var unit = javaFiles.Files[fileName];
                if (!string.IsNullOrWhiteSpace(unit?.Downloads?.Raw?.DownloadUrl) && unit.Type == JavaFiles.UnitType.File)
                {
                    var filePath = javaBaseDir + fileName;
                    try
                    {
                        if (!File.Exists(filePath))
                        {
                            _updateList[fileName] = unit;
                        }
                    }
                    catch { }
                }
            }

            result = CheckResult.Successful;
            return _updateList.Count != 0;
        }

        public bool Update(Action<int, int, int, string> percentHandler)
        {
            int filesCount = _updateList.Count;
            int file = 0;

            string javaPath = FILES_DIR + _thisJava.JavaName + "/";

            foreach (var unitName in _updateList.Keys)
            {
                try
                {
                    string path = Path.GetDirectoryName(unitName);
                    string fileName = Path.GetFileName(unitName);

                    var taskArgs = new TaskArgs
                    {
                        PercentHandler = delegate (int value)
                        {
                            percentHandler((int)(((decimal)file / (decimal)filesCount) * 100), file, filesCount, fileName);
                        },
                        CancelToken = _cancelToken
                    };

                    if (!WithDirectory.InstallFile(_updateList[unitName].Downloads.Raw.DownloadUrl, fileName, javaPath + path, taskArgs))
                    {
                        _semIsReleased = true;
                        _downloadSemaphore.Release(_thisJava.JavaName);
                        return false;
                    }
                }
                catch
                {
                    _semIsReleased = true;
                    _downloadSemaphore.Release(_thisJava.JavaName);
                    return false;
                }

                file++;
            }

            _semIsReleased = true;
            _downloadSemaphore.Release(_thisJava.JavaName);
            return true;
        }

        public void Dispose()
        {
            string javaName = _thisJava?.JavaName;
            if (javaName != null && !_semIsReleased)
            {
                _downloadSemaphore.Release(javaName);
            }
        }
    }
}
