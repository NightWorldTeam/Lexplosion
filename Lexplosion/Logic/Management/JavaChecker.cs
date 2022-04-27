using Lexplosion.Logic.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lexplosion.Logic.FileSystem;
using System.IO;

namespace Lexplosion.Logic.Management
{
    using JavaVersionsFile = Dictionary<string, int>;

    class JavaChecker
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

        private GameVersion _gameVersion;

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

        public bool Check(out CheckResult result)
        {
            List<ToServer.JavaVersion> versions = ToServer.GetJavaVersions();
            ToServer.JavaVersion thisJava = null;

            foreach (ToServer.JavaVersion javaVer in versions)
            {
                if (javaVer.LastGameVersion == "max")
                {
                    thisJava = javaVer;
                    break;
                }
                else
                {
                    GameVersion version = new GameVersion(javaVer.LastGameVersion);
                    if (version.IsValid && version >= _gameVersion)
                    {
                        thisJava = javaVer;
                        break;
                    }
                }
            }

            if (thisJava != null && thisJava.JavaName != null)
            {
                JavaVersionsFile info = DataFilesManager.GetFile<JavaVersionsFile>(WithDirectory.DirectoryPath + "/java/javaVersions.json");
                if (info != null && info.ContainsKey(thisJava.JavaName))
                {
                    if (info[thisJava.JavaName] < thisJava.LastUpdate)
                    {
                        result = CheckResult.Successful;
                        return true;
                    }
                    else
                    {
                        if (!Directory.Exists(WithDirectory.DirectoryPath + "/java/" + thisJava.JavaName))
                        {
                            result = CheckResult.Successful;
                            return true;
                        }
                        else
                        {
                            result = CheckResult.Successful;
                            return false;
                        }
                    }
                }
                else
                {
                    result = CheckResult.Successful;
                    return true;
                }
            }
            else
            {
                result = CheckResult.DefinitionError;
                return false;
            }
        }


    }
}
