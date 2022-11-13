using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Lexplosion.Global;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Logic.Objects;
using static Lexplosion.Logic.FileSystem.WithDirectory;

namespace Lexplosion.Logic.FileSystem
{
    static class DataFilesManager
    {
        public static void SaveAccount(string login, string accessData, AccountType accountType)
        {
            accessData = Convert.ToBase64String(AesСryp.Encode(accessData, Encoding.UTF8.GetBytes(LaunсherSettings.passwordKey), Encoding.UTF8.GetBytes(LaunсherSettings.passwordKey.Substring(0, 16))));

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

        /// <summary>
        /// Получает сохраненный аккаунт.
        /// </summary>
        /// <param name="login">Сюда будет помещен логин.</param>
        /// <param name="accessData">Данные для получения доступа. Может быть пароль, а может какой-нибудь токен.</param>
        /// <param name="accountType">Сюда передавать тип аккаунта, который нужно получить. Передавать null, если нужно получить использованный в последний раз аккаунт.</param>
        /// <returns>
        /// Возвращает тип полученного аккаунта, он может отличаться от параметра accountType, 
        /// ведь если accountType будет null, то будет возвращен тип аккаунта, использованного в последний раз.
        /// </returns>
        public static AccountType GetAccount(out string login, out string accessData, AccountType? accountType)
        {
            var data = GetFile<AcccountsFormat>(LaunсherSettings.LauncherDataPath + "/account.json");
            if (data != null && data.Profiles != null && data.Profiles.Count > 0)
            {
                AccountType selectedAccount = accountType ?? data.SelectedProfile;

                if (data.Profiles.ContainsKey(selectedAccount))
                {
                    AcccountsFormat.Profile profile = data.Profiles[selectedAccount];

                    if (!string.IsNullOrEmpty(profile.Login) && !string.IsNullOrEmpty(profile.AccessData))
                    {
                        login = profile.Login;
                        accessData = AesСryp.Decode(Convert.FromBase64String(profile.AccessData), Encoding.UTF8.GetBytes(LaunсherSettings.passwordKey), Encoding.UTF8.GetBytes(LaunсherSettings.passwordKey.Substring(0, 16)));
                    }
                    else
                    {
                        login = null;
                        accessData = null;
                    }

                    return selectedAccount;
                }
            }

            login = null;
            accessData = null;

            return accountType ?? AccountType.NightWorld;
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
                return JsonConvert.DeserializeObject<T>(fileContent);
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

        public static void SaveManifest(string instanceId, VersionManifest data)
        {
            SaveFile(DirectoryPath + "/instances/" + instanceId + "/" + "manifest.json", JsonConvert.SerializeObject(data));
            if (data.libraries != null)
            {
                if (data.version.additionalInstaller != null)
                {
                    var baseLibs = new Dictionary<string, LibInfo>();
                    var additionalLibs = new Dictionary<string, LibInfo>();

                    // в этом цикле разъединяем либрариесы и дополнительный отправляем в отадельные файлы
                    foreach (var key in data.libraries.Keys)
                    {
                        LibInfo value = data.libraries[key];
                        if (value.additionalInstallerType == null)
                        {
                            baseLibs[key] = value;
                        }
                        else
                        {
                            additionalLibs[key] = value;
                        }
                    }

                    SaveFile(DirectoryPath + "/versions/libraries/" + data.version.GetLibName + ".json", JsonConvert.SerializeObject(baseLibs));

                    if (additionalLibs != null && additionalLibs.Count > 0)
                    {
                        SaveFile(DirectoryPath + "/versions/additionalLibraries/" + data.version.additionalInstaller.GetLibName + ".json", JsonConvert.SerializeObject(additionalLibs));
                    }
                }
                else
                {
                    SaveFile(DirectoryPath + "/versions/libraries/" + data.version.GetLibName + ".json", JsonConvert.SerializeObject(data.libraries));
                }
            }
        }

        public static VersionManifest GetManifest(string instanceId, bool includingLibraries)
        {
            VersionManifest data = GetFile<VersionManifest>(DirectoryPath + "/instances/" + instanceId + "/" + "manifest.json");
            if (data == null)
            {
                return null;
            }

            if (includingLibraries)
            {
                var librariesData = GetFile<Dictionary<string, LibInfo>>(DirectoryPath + "/versions/libraries/" + data.version.GetLibName + ".json") ?? new Dictionary<string, LibInfo>();

                var installer = data.version?.additionalInstaller;
                if (installer != null)
                {
                    var additionallibrarieData = GetFile<Dictionary<string, LibInfo>>(DirectoryPath + "/versions/additionalLibraries/" + installer?.GetLibName + ".json");

                    if (additionallibrarieData != null)
                    {
                        foreach (var lib in additionallibrarieData.Keys)
                        {
                            librariesData[lib] = additionallibrarieData[lib];
                            librariesData[lib].additionalInstallerType = installer.type;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }

                data.libraries = librariesData;
            }

            return data;
        }

        public static InstalledAddonsFormat GetInstalledAddons(string instanceId)
        {
            string path = WithDirectory.DirectoryPath + "/instances/" + instanceId + "/installedAddons.json";

            var data = DataFilesManager.GetFile<InstalledAddonsFormat>(path);
            if (data == null)
            {
                return new InstalledAddonsFormat();
            }

            return data;
        }

        public static void SaveInstalledAddons(string instanceId, InstalledAddonsFormat data)
        {
            string path = WithDirectory.DirectoryPath + "/instances/" + instanceId + "/installedAddons.json";
            DataFilesManager.SaveFile(path, JsonConvert.SerializeObject(data));
        }
    }
}
