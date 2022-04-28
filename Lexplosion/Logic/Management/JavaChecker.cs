using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using Newtonsoft.Json;

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

        private struct GameVersion
        {
            private int[] _numbers;
            public bool IsValid { get; private set; }

            public GameVersion(string gameVersion)
            {
                if (gameVersion == "max")
                {
                    _numbers = new int[3]
                    {
                        Int32.MaxValue,
                        Int32.MaxValue,
                        Int32.MaxValue
                    };

                    IsValid = true;
                    return;
                }

                string[] numbers = gameVersion.Split('.');

                if (numbers.Length == 3)
                {
                    int num1 = 0, num2 = 0, num3 = 0;
                    IsValid = Int32.TryParse(numbers[0], out num1) && Int32.TryParse(numbers[1], out num2) && Int32.TryParse(numbers[2], out num3);

                    _numbers = new int[3] {
                        num1,
                        num2,
                        num3
                    };
                }
                else
                {
                    IsValid = false;
                    _numbers = new int[3];
                }
            }

            public static bool operator >=(GameVersion elem1, GameVersion elem2)
            {
                if (elem1._numbers[0] > elem2._numbers[0])
                    return true;
                else if (elem1._numbers[0] < elem2._numbers[0])
                    return false;

                if (elem1._numbers[1] > elem2._numbers[1])
                    return true;
                else if (elem1._numbers[1] < elem2._numbers[1])
                    return false;

                if (elem1._numbers[2] > elem2._numbers[2])
                    return true;
                else if (elem1._numbers[2] < elem2._numbers[2])
                    return false;

                return true;
            }

            public static bool operator <=(GameVersion elem1, GameVersion elem2)
            {
                if (elem1._numbers[0] < elem2._numbers[0])
                    return true;
                else if (elem1._numbers[0] > elem2._numbers[0])
                    return false;

                if (elem1._numbers[1] < elem2._numbers[1])
                    return true;
                else if (elem1._numbers[1] > elem2._numbers[1])
                    return false;

                if (elem1._numbers[2] < elem2._numbers[2])
                    return true;
                else if (elem1._numbers[2] > elem2._numbers[2])
                    return false;

                return true;
            }
        }

        private static readonly KeySemaphore _downloadSemaphore = new KeySemaphore();

        private bool _semIsReleased = false;
        private GameVersion _gameVersion;
        private JavaVersion _thisJava = null;
        private JavaVersionsFile _versionsFile;

        public bool IsValid
        {
            get
            {
                return _gameVersion.IsValid;
            }
        }

        public JavaChecker(string gameVersion)
        {
            _gameVersion = new GameVersion(gameVersion);
        }

        public bool Check(out CheckResult result, out JavaVersion java)
        {
            List<JavaVersion> versions = ToServer.GetJavaVersions();

            foreach (JavaVersion javaVer in versions)
            {
                GameVersion version = new GameVersion(javaVer.LastGameVersion);
                if (version.IsValid && version >= _gameVersion)
                {
                    _thisJava = javaVer;
                    break;
                }
            }

            _downloadSemaphore.WaitOne(_thisJava.JavaName);

            if (_thisJava != null && _thisJava.JavaName != null)
            {
                _versionsFile = DataFilesManager.GetFile<JavaVersionsFile>(WithDirectory.DirectoryPath + "/java/javaVersions.json");
                if (_versionsFile != null && _versionsFile.ContainsKey(_thisJava.JavaName) && _versionsFile[_thisJava.JavaName] != null)
                {
                    if (_versionsFile[_thisJava.JavaName].LastUpdate < _thisJava.LastUpdate)
                    {
                        java = _thisJava;
                        result = CheckResult.Successful;
                        return true;
                    }
                    else
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
            else
            {
                java = null;
                result = CheckResult.DefinitionError;
                return false;
            }
        }

        public bool Update()
        {
            if (_versionsFile == null)
            {
                _versionsFile = new JavaVersionsFile();
            }

            if (WithDirectory.DonwloadJava(_thisJava.JavaName))
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
