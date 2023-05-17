using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Lexplosion.Global;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.FileSystem.StorageManagment;
using static Lexplosion.Logic.FileSystem.WithDirectory;

namespace Lexplosion.Logic.FileSystem
{
    public static class DataFilesManager
    {
        public static void SaveAccount(string login, string accessData, AccountType accountType)
        {
            //костыль и мне похуй, лень проверку делать
            if (accountType == AccountType.NoAuth)
            {
                accessData = "zhopa";
            }

            accessData = Convert.ToBase64String(Cryptography.AesEncode(accessData, Encoding.UTF8.GetBytes(LaunсherSettings.passwordKey), Encoding.UTF8.GetBytes(LaunсherSettings.passwordKey.Substring(0, 16))));

            var data = GetFile<AcccountsFormat>(LaunсherSettings.LauncherDataPath + "/account.json");
            if (data != null && data.Profiles != null && data.Profiles.Count > 0)
            {
                data.SelectedProfile = accountType;
                data.Profiles[accountType] = new AcccountsFormat.Profile
                {
                    Login = login,
                    AccessData = accessData
                };
            }
            else
            {
                data = new AcccountsFormat
                {
                    SelectedProfile = accountType,
                    Profiles = new Dictionary<AccountType, AcccountsFormat.Profile>
                    {
                        [accountType] = new AcccountsFormat.Profile
                        {
                            Login = login,
                            AccessData = accessData
                        }
                    }
                };
            }

            SaveFile(LaunсherSettings.LauncherDataPath + "/account.json", JsonConvert.SerializeObject(data));
        }

        public static void SaveSettings(Settings data, string instanceId = "")
        {
            string file;

            if (instanceId == "")
            {
                string path = LaunсherSettings.LauncherDataPath;
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                file = path + "/settings.json";
            }
            else
            {
                data.GamePath = null;

                string path = DirectoryPath + "/instances/" + instanceId;
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                file = path + "/instanceSettings.json";
            }

            try
            {
                Settings settings = GetSettings(instanceId);
                if (settings != null)
                {
                    settings.Merge(data);
                }
                else
                {
                    settings = data;
                }

                using (FileStream fstream = new FileStream(file, FileMode.Create))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(settings));
                    fstream.Write(bytes, 0, bytes.Length);
                    fstream.Close();
                }

            }
            catch { }
        }

        public static Settings GetSettings(string instanceId = "")
        {
            string file;
            if (instanceId == "")
            {
                file = LaunсherSettings.LauncherDataPath + "/settings.json";
            }
            else
            {
                file = DirectoryPath + "/instances/" + instanceId + "/instanceSettings.json";

                if (!File.Exists(file))
                {
                    return new Settings();
                }
            }

            try
            {
                using (FileStream fstream = File.OpenRead(file))
                {
                    byte[] fileBytes = new byte[fstream.Length];
                    fstream.Read(fileBytes, 0, fileBytes.Length);
                    fstream.Close();

                    Settings settings = JsonConvert.DeserializeObject<Settings>(Encoding.UTF8.GetString(fileBytes));
                    if (instanceId != "") settings.GamePath = null;

                    return settings ?? new Settings();
                }
            }
            catch
            {
                return new Settings();
            }
        }

        public static bool DeleteLastUpdates(string instanceId) //Эта функция удаляет файл lastUpdates.json
        {
            try
            {
                if (File.Exists(DirectoryPath + "/instances/" + instanceId + "/lastUpdates.json"))
                {
                    File.Delete(DirectoryPath + "/instances/" + instanceId + "/lastUpdates.json");
                }

                return true;

            }
            catch { return false; }
        }

        public static int GetUpgradeToolVersion()
        {
            if (!File.Exists(DirectoryPath + "/up-version.txt"))
                return -1;

            try
            {
                using (FileStream fstream = File.OpenRead(DirectoryPath + "/up-version.txt"))
                {
                    byte[] fileBytes = new byte[fstream.Length];
                    fstream.Read(fileBytes, 0, fileBytes.Length);
                    fstream.Close();

                    return Int32.Parse(Encoding.UTF8.GetString(fileBytes));

                }

            }
            catch { return -1; }

        }

        public static void SetUpgradeToolVersion(int version)
        {
            try
            {
                if (!File.Exists(DirectoryPath + "/up-version.txt"))
                    File.Create(DirectoryPath + "/up-version.txt").Close();

                using (FileStream fstream = new FileStream(DirectoryPath + "/up-version.txt", FileMode.Create))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(version.ToString());
                    fstream.Write(bytes, 0, bytes.Length);
                    fstream.Close();
                }

            }
            catch { }

        }

        public static bool SaveFile(string name, string content)
        {
            try
            {
                string dirName = Path.GetDirectoryName(name);
                if (!Directory.Exists(dirName))
                {
                    Directory.CreateDirectory(dirName);
                }

                using (FileStream fstream = new FileStream(name, FileMode.Create, FileAccess.Write))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(content);
                    fstream.Write(bytes, 0, bytes.Length);
                    fstream.Close();
                }

                return true;

            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Получет и декодирует содержиоме JSON файла
        /// </summary>
        /// <typeparam name="T">Тип, к которому привести JSON</typeparam>
        /// <param name="file">Путь до файла</param>
        /// <returns>Декодированные данные</returns>
        public static T GetFile<T>(string file)
        {
            try
            {
                string fileContent = GetFile(file);
                return fileContent != null ? JsonConvert.DeserializeObject<T>(fileContent) : default;
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// Получет содержимое текстового файла
        /// </summary>
        /// <param name="file">Путь до файла</param>
        /// <returns>Текстовые данные</returns>
        public static string GetFile(string file)
        {
            try
            {
                if (File.Exists(file))
                {
                    using (FileStream fstream = File.OpenRead(file))
                    {
                        byte[] fileBytes = new byte[fstream.Length];
                        fstream.Read(fileBytes, 0, fileBytes.Length);
                        fstream.Close();

                        return Encoding.UTF8.GetString(fileBytes);
                    }
                }

                return null;
            }
            catch
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }

                return null;
            }
        }

        public static T GetData<T>(IDataHandlerArgs<T> args)
        {
            var handler = args.Handler;
            return handler.LoadFromStorage();
        }

        public static void SaveData<T>(IDataHandlerArgs<T> args, T data)
        {
            var handler = args.Handler;
            handler.SaveToStorage(data);
        }
    }
}
