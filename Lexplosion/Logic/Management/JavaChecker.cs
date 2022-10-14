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
    using JavaVersionsFile = Dictionary<string, JavaVersion>;

    class JavaChecker : IDisposable
    {
        public enum CheckResult
        {
            Successful,
            DefinitionError
        }

        private static readonly KeySemaphore<string> _downloadSemaphore = new KeySemaphore<string>();

        private bool _semIsReleased = false;
        private long _releaseIndex;
        private JavaVersion _thisJava = null;
        private JavaVersionsFile _versionsFile;
        private CancellationToken _cancelToken;

        public JavaChecker(long releaseIndex, CancellationToken cancelToken, bool isReleased = false)
        {
            _releaseIndex = releaseIndex;
            _cancelToken = cancelToken;
            _semIsReleased = isReleased;
        }

        public JavaVersion GetJavaInfo()
        {
            // получаем файл с версиями джавы
            _versionsFile = DataFilesManager.GetFile<JavaVersionsFile>(WithDirectory.DirectoryPath + "/java/javaVersions.json");

            JavaVersion javaInfo = null;

            //ищем нужную версию джавы
            foreach (JavaVersion javaVer in _versionsFile.Values)
            {
                if (javaVer.LastReleaseIndex >= _releaseIndex)
                {
                    javaInfo = javaVer;
                    break;
                }
            }

            return javaInfo;
        }

        public bool Check(out CheckResult result, out JavaVersion java)
        {
            List<JavaVersion> versions = ToServer.GetJavaVersions(); // получамем данные с сервера

            if (versions == null) //данные получены не были. пытаемся выехать на том, что есть на диске
            {
                _thisJava = GetJavaInfo();

                if (_thisJava?.JavaName != null) //нашли
                {
                    java = _thisJava;
                    result = CheckResult.Successful;
                }
                else //не нашли
                {
                    java = null;
                    result = CheckResult.DefinitionError;
                }

                return false;
            }

            //данне с сервера нормально получили. ищем нужную дажву
            foreach (JavaVersion javaVer in versions)
            {
                if (javaVer.LastReleaseIndex >= _releaseIndex)
                {
                    _thisJava = javaVer;
                    break;
                }
            }

            if (_thisJava?.JavaName == null)
            {
                java = null;
                result = CheckResult.DefinitionError;
                return false;
            }

            _downloadSemaphore.WaitOne(_thisJava.JavaName);

            _versionsFile = DataFilesManager.GetFile<JavaVersionsFile>(WithDirectory.DirectoryPath + "/java/javaVersions.json");
            if (_versionsFile != null && _versionsFile.ContainsKey(_thisJava.JavaName) && _versionsFile[_thisJava.JavaName] != null)
            {
                //проверяем версию
                if (_versionsFile[_thisJava.JavaName].LastUpdate < _thisJava.LastUpdate)
                {   //версия старая, нужно обновить
                    java = _thisJava;
                    result = CheckResult.Successful;
                    return true;
                }
                else // с версией все нормально. проверяем наличие на диске
                {
                    if (!File.Exists(WithDirectory.DirectoryPath + "/java/" + _thisJava.JavaName + "/" + _thisJava.ExecutableFile))
                    {
                        java = _thisJava;
                        result = CheckResult.Successful;
                        return true;
                    }
                    else
                    {
                        java = _versionsFile[_thisJava.JavaName];
                        result = CheckResult.Successful;
                        return false;
                    }
                }
            }
            else
            {
                java = _thisJava;
                result = CheckResult.Successful;
                return true;
            }
        }

        public bool Update(Action<int, string> percentHandler)
        {
            if (_versionsFile == null)
            {
                _versionsFile = new JavaVersionsFile();
            }

            string filename = _thisJava.JavaName + ".zip";

            var taskArgs = new TaskArgs
            {
                PercentHandler = delegate (int value)
                {
                    percentHandler(value, filename);
                },
                CancelToken = _cancelToken
            };

            string bitDepth = Environment.Is64BitOperatingSystem ? "x64" : "x32";
            if (WithDirectory.DonwloadJava(_thisJava.JavaName, bitDepth, taskArgs))
            {
                _versionsFile[_thisJava.JavaName] = _thisJava;
                DataFilesManager.SaveFile(WithDirectory.DirectoryPath + "/java/javaVersions.json", JsonConvert.SerializeObject(_versionsFile));

                _semIsReleased = true;
                _downloadSemaphore.Release(_thisJava.JavaName);
                return true;
            }
            else
            {
                _semIsReleased = true;
                _downloadSemaphore.Release(_thisJava.JavaName);
                return false;
            }
        }

        public void Dispose()
        {
            if (!_semIsReleased)
            {
                _downloadSemaphore.Release(_thisJava.JavaName);
            }
        }
    }
}
