using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.Nightworld;
using Lexplosion.Global;
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
        /// Проверяет файлы сбокри (моды, конфиги и прочую хуйню).
        /// </summary>
        /// <returns>
        /// Возвращает количество файлов, которые нужно обновить. -1 в случае неудачи (возможно только если включена защита целосности клиента). 
        /// </returns>
        public int CheckInstance(NightWorldManifest filesInfo, ref LastUpdates updates)
        {
            //Проходимся по списку папок(data) из класса instanceFiles
            foreach (string dir in filesInfo.data.Keys)
            {
                string folder = DirectoryPath + "/instances/" + instanceId + "/" + dir;

                try
                {
                    if (!updates.ContainsKey("/" + dir) || updates["/" + dir] < filesInfo.data[dir].folderVersion) //проверяем версию папки. если она старая - очищаем
                    {
                        if (Directory.Exists(folder))
                        {
                            Directory.Delete(folder, true);
                            Directory.CreateDirectory(folder);
                        }

                        updates["/" + dir] = filesInfo.data[dir].folderVersion;
                    }

                    // TODO: тут из lastUpdates удалить все файлы из этой папки

                    //отрываем файл с последними обновлениями и записываем туда updates, который уже содержит последнюю версию папки. Папка сейчас будет пустой, поэтому метод Update в любом случае скачает нужные файлы
                    using (FileStream fstream = new FileStream(DirectoryPath + "/instances/" + instanceId + "/lastUpdates.json", FileMode.Create, FileAccess.Write))
                    {
                        byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(updates));
                        fstream.Write(bytes, 0, bytes.Length);
                        fstream.Close();
                    }
                }
                catch { }

                if (Directory.Exists(folder))
                {
                    //Получаем список всех файлов в папке
                    string[] files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);

                    foreach (string file in files) //проходимся по папке
                    {
                        string fileName = file.Replace(folder, "").Remove(0, 1).Replace(@"\", "/");

                        if (filesInfo.data[dir].security) //при включенной защите данной папки удалем левые файлы
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

                        //сверяем версию файла с его версией в списке, если версия старая, то отправляем файл на обновление
                        if (filesInfo.data[dir].objects.ContainsKey(fileName))
                        {
                            if (!updates.ContainsKey("/" + dir + "/" + fileName) || updates["/" + dir + "/" + fileName] != filesInfo.data[dir].objects[fileName].lastUpdate)
                            {
                                if (!data.ContainsKey(dir)) //если директория отсутствует в data, то добавляем её 
                                {
                                    data.Add(dir, new List<string>());
                                    updatesCount++;
                                }

                                if (!data[dir].Contains(fileName))
                                {
                                    data[dir].Add(fileName);
                                    updatesCount++;
                                }
                            }
                        }
                    }
                }

                //ищем отсутвующие файлы
                foreach (string file in filesInfo.data[dir].objects.Keys)
                {
                    if (!File.Exists(folder + "/" + file))
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
                }
            }

            //ищем старые файлы
            foreach (string folder in filesInfo.data.Keys)
            {
                foreach (string file in filesInfo.data[folder].oldFiles)
                {
                    try
                    {
                        if (File.Exists(DirectoryPath + "/instances/" + instanceId + "/" + folder + "/" + file))
                        {
                            oldFiles.Add(folder + "/" + file);
                            updatesCount++;
                        }

                    }
                    catch { }
                }
            }

            return updatesCount;
        }

        /// <summary>
        /// Обновляет файлы, которые метод CheckInstance добавил в список
        /// </summary>
        /// <returns>
        /// Возвращает список файлов, скачивание которых закончилось ошибкой
        /// </returns>
        public List<string> UpdateInstance(NightWorldManifest filesList, string externalId, ref LastUpdates updates)
        {
            int updated = 0;
            WebClient wc = new WebClient();
            string tempDir = CreateTempDir();

            string addr;
            List<string> errors = new List<string>();

            //скачивание файлов из списка data
            foreach (string dir in data.Keys)
            {
                foreach (string file in data[dir])
                {
                    string[] folders = file.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                    if (filesList.data[dir].objects[file].url == null)
                    {
                        addr = LaunсherSettings.URL.Upload + "modpacks/" + externalId + "/" + dir + "/" + file;
                    }
                    else
                    {
                        addr = filesList.data[dir].objects[file].url;
                    }

                    string filename = folders[folders.Length - 1];
                    string path = DirectoryPath + "/instances/" + instanceId + "/" + dir + "/" + file.Replace(filename, "");
                    string sha1 = filesList.data[dir].objects[file].sha1;
                    long size = filesList.data[dir].objects[file].size;

                    if (!SaveDownloadZip(addr, filename, path, tempDir, sha1, size, delegate (int a) { }))
                    {
                        errors.Add(dir + "/" + file);
                    }
                    else
                    {
                        updates["/" + dir + "/" + file] = filesList.data[dir].objects[file].lastUpdate; //добавляем файл в список последних обновлений
                    }

                    updated++;
                    FilesDownloadEvent?.Invoke(updatesCount, updated);

                    // TODO: где-то тут записывать что файл был обновлен, чтобы если загрузка была первана она началась с того же места
                }
            }

            wc.Dispose();

            //удаляем старые файлы
            foreach (string file in oldFiles)
            {
                if (File.Exists(DirectoryPath + "/instances/" + instanceId + "/" + file))
                {
                    File.Delete(DirectoryPath + "/instances/" + instanceId + "/" + file);
                    if (updates.ContainsKey("/" + file))
                    {
                        updates.Remove("/" + file);

                        updated++;
                        FilesDownloadEvent?.Invoke(updatesCount, updated);
                    }
                }
            }

            //сохарняем updates
            SaveFile(DirectoryPath + "/instances/" + instanceId + "/lastUpdates.json", JsonConvert.SerializeObject(updates));

            Directory.Delete(tempDir, true);

            return errors;
        }

        /// <summary>
        /// Проверяет все ли файлы клиента присутсвуют
        /// </summary>
        public bool InvalidStruct(LastUpdates updates)
        {
            foreach (string file in updates.Keys)
            {
                try
                {
                    string path = DirectoryPath + "/instances/" + instanceId + "/" + file;

                    if (!File.Exists(path) && !Directory.Exists(path) && (file != "libraries" && file != "version"))
                    {
                        return true;
                    }
                }
                catch { }
            }

            return false;
        }
    }
}
