using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Lexplosion.Global;
using Lexplosion.Tools;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.Nightworld;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Objects.CommonClientData;
using static Lexplosion.Logic.FileSystem.WithDirectory;
using static Lexplosion.Logic.FileSystem.DataFilesManager;

namespace Lexplosion.Logic.FileSystem
{
    class NightWorldInstaller : InstanceInstaller
    {
        public NightWorldInstaller(string instanceID) : base(instanceID) { }

        private Dictionary<string, List<string>> data = new Dictionary<string, List<string>>(); //сюда записываем файлы, которые нужно обновить
        private List<string> oldFiles = new List<string>(); // список старых файлов, которые нуждаются в обновлении

        private int updatesCount = 0;

        public event ProcentUpdate FilesDownloadEvent;

        /// <summary>
        /// Возвращает список всех дополнений сборки.
        /// </summary>
        /// <returns>Список дополнений. Ключ - путь относительно корня папки сборки, значение - айдишник.</returns>
        public Dictionary<string, string> GetInstanceContent()
        {
            var result = new Dictionary<string, string>();
            using (InstalledAddons installedAddons = InstalledAddons.Get(instanceId))
            {
                foreach (string key in installedAddons.Keys)
                {
                    InstalledAddonInfo addon = installedAddons[key];
                    if (addon != null)
                    {
                        result[addon.Path] = key;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Проверяет файлы сбокри (моды, конфиги и прочую хуйню).
        /// </summary>
        /// <returns>
        /// Возвращает количество файлов, которые нужно обновить. -1 в случае неудачи (возможно только если включена защита целосности клиента). 
        /// </returns>
        public int CheckInstance(NightWorldManifest filesInfo, ref LastUpdates updates, Dictionary<string, string> content)
        {
            string instancePath = InstancesPath + instanceId + "/";
            using (InstalledAddons installedAddons = InstalledAddons.Get(instanceId))
            {
                //Проходимся по списку папок(data) из класса instanceFiles
                foreach (string dir in filesInfo.data.Keys)
                {
                    string folder = instancePath + dir;

                    bool folderIsOld = false;
                    bool dirIsExists = Directory.Exists(folder);
                    try
                    {
                        //проверяем версию папки. если она старая - очищаем
                        if (!updates.ContainsKey(dir) || updates[dir] < filesInfo.data[dir].folderVersion)
                        {
                            if (dirIsExists)
                            {
                                folderIsOld = true;
                                // проходимся по всем файлам в папке и удаляем их
                                foreach (string file in filesInfo.data[dir].objects.Keys)
                                {
                                    // если файл есть в спсике установленных аддонов - удаляем его по-красивому через RemoveFromDir, если нету - удаляем так.
                                    // это всё нужно чтобы не удалился установленный пользователем контент. 
                                    string filePath = dir + "/" + file;
                                    if (content.ContainsKey(filePath))
                                    {
                                        InstalledAddonInfo addon = installedAddons[content[filePath]];
                                        if (addon != null)
                                        {
                                            addon.RemoveFromDir(instancePath);
                                            installedAddons.TryRemove(addon.FileID);
                                            updates.Remove(filePath);
                                        }
                                        else
                                        {
                                            updates.Remove(filePath);
                                            WithDirectory.DelFile(instancePath + filePath);
                                        }
                                    }
                                    else
                                    {
                                        updates.Remove(filePath);
                                        WithDirectory.DelFile(instancePath + filePath);
                                    }
                                }
                            }

                            updates[dir] = filesInfo.data[dir].folderVersion;
                        }

                        //отрываем файл с последними обновлениями и записываем туда updates, который уже содержит последнюю версию папки. Папка сейчас будет пустой, поэтому метод Update в любом случае скачает нужные файлы
                        SaveFile(InstancesPath + instanceId + "/lastUpdates.json", JsonConvert.SerializeObject(updates));
                    }
                    catch { }

                    //при включенной защите данной папки удалем левые файлы
                    #region security
                    if (filesInfo.data[dir].security && dirIsExists && !folderIsOld)
                    {
                        //Получаем список всех файлов в папке
                        string[] files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);

                        foreach (string file in files) //проходимся по папке
                        {
                            string fileName = file.Replace(folder, "").Remove(0, 1).Replace(@"\", "/");

                            if (filesInfo.data[dir].security)
                            {
                                try
                                {
                                    using (FileStream fstream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Read)) //открываем файл на чтение
                                    {
                                        byte[] bytes = new byte[fstream.Length];
                                        fstream.Read(bytes, 0, bytes.Length);
                                        fstream.Close();

                                        if (filesInfo.data[dir].objects.ContainsKey(fileName)) // проверяем есть ли этот файл в списке
                                        {
                                            using (SHA1 sha = new SHA1Managed())
                                            {
                                                if (Convert.ToBase64String(sha.ComputeHash(bytes)) != filesInfo.data[dir].objects[fileName].sha1 || bytes.Length != filesInfo.data[dir].objects[fileName].size)
                                                {
                                                    File.Delete(file); //удаляем файл, если не сходится хэш или размер

                                                    if (!data.ContainsKey(dir)) //если директория отсутствует в data, то добавляем её 
                                                    {
                                                        data.Add(dir, new List<string>());
                                                    }

                                                    data[dir].Add(fileName); //добавляем файл в список на обновление
                                                    updatesCount++;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            File.Delete(file);
                                        }
                                    }
                                }
                                catch
                                {
                                    //чтение одного из файлов не удалось, стопаем весь процесс
                                    return -1;
                                }
                            }
                        }
                    }
                    #endregion

                    //проходимся по всем файлам и проверяем их версии
                    foreach (string file in filesInfo.data[dir].objects.Keys)
                    {
                        // вычисляем имя файла на диске.
                        string actualPath; // его имя на диске. то есть с .disable, если он выключен
                        string basePath = dir + "/" + file; // базовое имя, то есть без .disable. Именно это имя имеет файл на сервере
                        if (content.ContainsKey(basePath))
                        {
                            InstalledAddonInfo addon = installedAddons[content[basePath]];
                            if (addon != null)
                            {
                                actualPath = addon.ActualPath;
                            }
                            else
                            {
                                actualPath = basePath;
                            }
                        }
                        else
                        {
                            actualPath = basePath;
                        }

                        // проверяем есть ли файл. если нету - кидаем на обновление
                        if (!dirIsExists || folderIsOld || !File.Exists(instancePath + actualPath))
                        {
                            if (!data.ContainsKey(dir))
                            {
                                data.Add(dir, new List<string>());
                                updatesCount++;
                            }

                            if (!data[dir].Contains(file))
                            {
                                data[dir].Add(file);
                                updatesCount++;
                            }
                        }
                        else // файл есть
                        {
                            //сверяем версию файла с его версией в списке, если версия старая, то отправляем файл на обновление
                            if (!updates.ContainsKey(basePath) || updates[basePath] != filesInfo.data[dir].objects[file].lastUpdate)
                            {
                                if (!data.ContainsKey(dir)) //если директория отсутствует в data, то добавляем её 
                                {
                                    data.Add(dir, new List<string>());
                                    updatesCount++;
                                }

                                if (!data[dir].Contains(file))
                                {
                                    data[dir].Add(file);
                                    updatesCount++;
                                }
                            }
                        }
                    }
                }

                //ищем старые файлы
                foreach (string folder in filesInfo.data.Keys)
                {
                    foreach (string file in filesInfo.data[folder].oldFiles)
                    {
                        try
                        {
                            string filePath = folder + "/" + file;
                            if (content.ContainsKey(filePath)) // сначала пытаемся его найти в спсике аддонов
                            {
                                InstalledAddonInfo addon = installedAddons[content[filePath]];
                                if (addon != null)
                                {
                                    // нашли. юзаем ActualPath чтобы получить имя
                                    if (File.Exists(instancePath + addon.ActualPath))
                                    {
                                        oldFiles.Add(filePath);
                                        updatesCount++;
                                    }
                                }
                                else
                                {
                                    // не нашли. херачим ручками
                                    if (File.Exists(instancePath + filePath))
                                    {
                                        oldFiles.Add(filePath);
                                        updatesCount++;
                                    }
                                }
                            }
                            else
                            {
                                // не нашли. херачим ручками
                                if (File.Exists(instancePath + filePath))
                                {
                                    oldFiles.Add(filePath);
                                    updatesCount++;
                                }
                            }
                        }
                        catch
                        {
                            // TODO: тут ошибку выкидывать
                        }
                    }
                }

                installedAddons.Save();
            }

            return updatesCount;
        }

        /// <summary>
        /// Обновляет файлы, которые метод CheckInstance добавил в список
        /// </summary>
        /// <returns>
        /// Возвращает список файлов, скачивание которых закончилось ошибкой
        /// </returns>
        public List<string> UpdateInstance(NightWorldManifest filesList, string externalId, ref LastUpdates updates, Dictionary<string, string> content, CancellationToken cancelToken)
        {
            int updated = 0;

            string tempDir = CreateTempDir();

            string addr;
            List<string> errors = new List<string>();

            string instancePath = InstancesPath + instanceId + "/";

            FilesDownloadEvent?.Invoke(updatesCount, 0);

            using (InstalledAddons installedAddons = InstalledAddons.Get(instanceId))
            {
                //скачивание файлов из списка data
                foreach (string dir in data.Keys)
                {
                    if (cancelToken.IsCancellationRequested) break;

                    foreach (string file in data[dir])
                    {
                        if (cancelToken.IsCancellationRequested) break;

                        if (filesList.data[dir].objects[file].url == null)
                        {
                            addr = LaunсherSettings.URL.Upload + "modpacks/" + externalId + "/" + dir + "/" + file;
                        }
                        else
                        {
                            addr = filesList.data[dir].objects[file].url;
                        }

                        string basePath = dir + "/" + file; // базовое имя, то есть без .disable. Именно это имя имеет файл на сервере
                        string actualPath; // его имя на диске. то есть с .disable, если он выключен
                        if (content.ContainsKey(basePath))
                        {
                            InstalledAddonInfo addon = installedAddons[content[basePath]];
                            if (addon != null)
                            {
                                actualPath = addon.ActualPath;
                            }
                            else
                            {
                                actualPath = basePath;
                            }
                        }
                        else
                        {
                            actualPath = basePath;
                        }

                        string filename = Path.GetFileName(basePath);
                        string path = Path.GetDirectoryName(instancePath + basePath) + "/";
                        string sha1 = filesList.data[dir].objects[file].sha1;
                        long size = filesList.data[dir].objects[file].size;

                        var taskArgs = new TaskArgs
                        {
                            PercentHandler = delegate (int a)
                            {
                                _fileDownloadHandler?.Invoke(filename, a, DownloadFileProgress.PercentagesChanged);
                            },
                            CancelToken = cancelToken
                        };

                        if (!SaveDownloadZip(addr, filename, path, tempDir, sha1, size, taskArgs))
                        {
                            _fileDownloadHandler?.Invoke(filename, 100, DownloadFileProgress.Error);
                            errors.Add(dir + "/" + file);
                        }
                        else
                        {
                            _fileDownloadHandler?.Invoke(filename, 100, DownloadFileProgress.Successful);
                            updates[dir + "/" + file] = filesList.data[dir].objects[file].lastUpdate; //добавляем файл в список последних обновлений
                            if (actualPath != basePath) //перименовываем файл, если его actualPath отличается от basePath
                            {
                                DelFile(instancePath + actualPath);
                                File.Move(instancePath + basePath, instancePath + actualPath);
                            }
                        }

                        updated++;
                        FilesDownloadEvent?.Invoke(updatesCount, updated);

                        //сохарняем updates
                        SaveFile(instancePath + "lastUpdates.json", JsonConvert.SerializeObject(updates));
                    }
                }

                //удаляем старые файлы
                foreach (string file in oldFiles)
                {
                    if (content.ContainsKey(file))
                    {
                        InstalledAddonInfo addon = installedAddons[content[file]];
                        if (addon != null)
                        {
                            addon.RemoveFromDir(instancePath);
                            installedAddons.TryRemove(addon.ProjectID);
                            updates.Remove(file);

                            updated++;
                            FilesDownloadEvent?.Invoke(updatesCount, updated);

                            installedAddons.Save();
                        }
                        else
                        {
                            if (File.Exists(instancePath + file))
                            {
                                File.Delete(instancePath + file);
                                if (updates.ContainsKey(file))
                                {
                                    updates.Remove(file);
                                    updated++;
                                    FilesDownloadEvent?.Invoke(updatesCount, updated);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (File.Exists(instancePath + file))
                        {
                            File.Delete(instancePath + file);
                            if (updates.ContainsKey(file))
                            {
                                updates.Remove(file);
                                updated++;
                                FilesDownloadEvent?.Invoke(updatesCount, updated);
                            }
                        }
                    }
                }
            }

            //сохарняем updates
            SaveFile(instancePath + "lastUpdates.json", JsonConvert.SerializeObject(updates));

            Directory.Delete(tempDir, true);

            return errors;
        }

        /// <summary>
        /// Проверяет все ли файлы клиента присутсвуют
        /// </summary>
        public bool InvalidStruct(LastUpdates updates, Dictionary<string, string> content)
        {
            string instancePath = InstancesPath + instanceId + "/";

            using (InstalledAddons installedAddons = InstalledAddons.Get(instanceId))
            {
                foreach (string file in updates.Keys)
                {
                    try
                    {
                        if (file != "libraries" && file != "version")
                        {
                            if (content.ContainsKey(file)) // опять же сначала пытаемся получить  файл из спсика аддонов
                            {
                                InstalledAddonInfo addon = installedAddons[content[file]];
                                if (addon != null)
                                {
                                    // если получили - берем ActualPath
                                    string actualPath = instancePath + addon.ActualPath;
                                    if (!File.Exists(actualPath) && !Directory.Exists(actualPath))
                                    {
                                        return true;
                                    }
                                }
                                else
                                {
                                    // не получили. формируем ручками
                                    string path = instancePath + file;
                                    if (!File.Exists(path) && !Directory.Exists(path))
                                    {
                                        return true;
                                    }
                                }
                            }
                            else
                            {
                                // не получили. формируем ручками
                                string path = instancePath + file;
                                if (!File.Exists(path) && !Directory.Exists(path))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    catch { }
                }
            }

            return false;
        }
    }
}
