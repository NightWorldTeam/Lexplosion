﻿using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;
using Lexplosion.Tools;
using Lexplosion.Logic.Network.Web;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.Curseforge;
using Lexplosion.Logic.Objects.CommonClientData;
using static Lexplosion.Logic.FileSystem.WithDirectory;
using static Lexplosion.Logic.FileSystem.DataFilesManager;
using System.Linq;

namespace Lexplosion.Logic.FileSystem
{
    class CurseforgeInstaller : StandartInstanceInstaller<InstanceManifest>
    {
        public CurseforgeInstaller(string instanceId) : base(instanceId) { }

        protected override InstanceManifest LoadManifest(string unzupArchivePath)
        {
            return GetFile<InstanceManifest>(unzupArchivePath + "manifest.json");
        }

        protected override void ArchiveHadnle(string unzupArchivePath, out List<string> files)
        {
            files = new List<string>();

            // тут переосим нужные файлы из этого архива

            string sourcePath = unzupArchivePath + "overrides/";
            string destinationPath = WithDirectory.GetInstancePath(instanceId);

            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            //если архив нового типа, то папки overrides не будет. Все папки сборки будут храниться рядом с манифестом 
            if (!Directory.Exists(sourcePath))
            {
                sourcePath = unzupArchivePath;
            }

            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                string dir = dirPath.Replace(sourcePath, destinationPath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }

            foreach (string path in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                if (Path.GetFileName(path) != "manifest.json")
                {
					string destPath = path.Replace(sourcePath, destinationPath);
					File.Copy(path, destPath, true);
                    files.Add(path.Replace(sourcePath, "/").Replace("\\", "/"));
                }
            }
        }

        /// <summary>
        /// Скачивает все аддоны модпака из спика
        /// </summary>
        /// <returns>
        /// Возвращает список ошибок.
        /// </returns>
        public override List<string> Install(InstanceManifest data, InstanceContent localFiles, CancellationToken cancelToken)
        {
            InstalledAddonsFormat installedAddons = null;
            installedAddons = localFiles.InstalledAddons;

			var errors = new List<string>();

            try
            {
                InstanceContent compliteDownload = new InstanceContent
                {
                    InstalledAddons = new InstalledAddonsFormat(),
                    Files = localFiles.Files
                };

                List<InstanceManifest.FileData> downloadList = new List<InstanceManifest.FileData>();

                if (installedAddons != null)
                {
                    var existsAddons = new HashSet<string>(); // этот список содержит айдишники аддонов, что установлены действительно и есть в списке с курсфорджа
                    foreach (InstanceManifest.FileData file in data.files) // проходимся по списку адднов, полученному с курсфорджа
                    {
                        if (!installedAddons.ContainsKey(file.projectID)) // если этого аддона нету в списке уже установленных, то тогда кидаем на обновление
                        {
                            downloadList.Add(file);
                        }
                        else
                        {
                            if (installedAddons[file.projectID].FileID != file.fileID || !installedAddons[file.projectID].IsExists(InstancesPath + instanceId + "/"))
                            {
                                downloadList.Add(file);
                            }
                            else
                            {
                                existsAddons.Add(file.projectID); // Аддон есть в списке установленых. Добавляем его айдишник в список
                            }
                        }
                    }

					foreach (string addonId in installedAddons.Keys) // проходимя по списку установленных аддонов
                    {
                        if (!existsAddons.Contains(addonId)) // если аддона нету в этом списке, значит его нету в списке, полученном с курсфорджа (ну или нам не подходит его версия, или же файла нету). Поэтому удаляем
                        {
                            if (installedAddons[addonId].ActualPath != null)
                            {
                                Runtime.DebugWrite("Delete file: " + InstancesPath + instanceId + "/" + installedAddons[addonId].ActualPath);
                                DelFile(InstancesPath + instanceId + "/" + installedAddons[addonId].ActualPath);
                            }
                        }
                        else
                        {
                            compliteDownload.InstalledAddons[addonId] = installedAddons[addonId];
                        }
                    }
                }
                else
                {
                    downloadList = data.files;
                }

                int filesCount = downloadList.Count;
                AddonsDownloadEventInvoke(filesCount, 0);

                if (filesCount != 0)
                {
                    SaveInstanceContent(compliteDownload);

                    object fileBlock = new object(); // этот объект блокировщик нужен что бы синхронизировать работу с json файлами

                    // формируем список айдишников
                    var ids = new string[filesCount];
                    int j = 0;
                    foreach (InstanceManifest.FileData file in downloadList)
                    {
                        ids[j] = file.projectID;
                        j++;
                    }

                    // получем инфу о всех аддонах
                    List<CurseforgeAddonInfo> addnos_ = CurseforgeApi.GetAddonsInfo(ids);
                    if (addnos_ == null) // у этих долбаебов просто по  приколу может упасть соединение во время запроса. пробуем второй раз
                    {
                        Thread.Sleep(5000);
                        addnos_ = CurseforgeApi.GetAddonsInfo(ids);
                    }
                    //преобразовываем эту хуйню в нормальный спсиок
                    var addons = new Dictionary<string, CurseforgeAddonInfo>();
                    if (addnos_ != null)
                    {
                        foreach (CurseforgeAddonInfo addon in addnos_)
                        {
                            addons[addon.id] = addon;
                        }
                    }

                    TasksPerfomer perfomer = null;
                    if (filesCount > 0)
                        perfomer = new TasksPerfomer(3, filesCount);

                    var noDownloaded = new ConcurrentBag<InstanceManifest.FileData>();
                    int downloadedCount = 0;

                    Runtime.DebugWrite("СКАЧАТЬ БЛЯТЬ НАДО " + downloadList.Count + " ЗЛОЕБУЧИХ МОДОВ");
                    foreach (InstanceManifest.FileData file in downloadList)
                    {
                        perfomer.ExecuteTask(delegate ()
                        {
                            Runtime.DebugWrite("ADD MOD TO PERFOMER");
                            CurseforgeAddonInfo addonInfo;
                            if (addons.ContainsKey(file.projectID))
                            {
                                addonInfo = addons[file.projectID];
                            }
                            else
                            {
                                addonInfo = CurseforgeApi.GetAddonInfo(file.projectID);
                            }

                            var taskArgs = new TaskArgs
                            {
                                PercentHandler = delegate (int percent)
                                {
                                    _fileDownloadHandler?.Invoke(addonInfo.name, percent, DownloadFileProgress.PercentagesChanged);
                                },
                                CancelToken = cancelToken
                            };

                            var result = CurseforgeApi.DownloadAddon(addonInfo, file.fileID, "/instances/" + instanceId + "/", taskArgs);

                            _fileDownloadHandler?.Invoke(addonInfo.name, 100, DownloadFileProgress.Successful);

                            if (result.Value2 == DownloadAddonRes.Successful)
                            {
                                if (!file.required && !result.Value1.IsDisable)
                                {
                                    File.Move(WithDirectory.GetInstancePath(instanceId) + result.Value1.ActualPath, WithDirectory.GetInstancePath(instanceId) + result.Value1.ActualPath + ".disable");
                                    result.Value1.IsDisable = true;
                                }

                                downloadedCount++;
                                AddonsDownloadEventInvoke(filesCount, downloadedCount);

                                lock (fileBlock)
                                {
                                    compliteDownload.InstalledAddons[file.projectID] = result.Value1;
                                    SaveInstanceContent(compliteDownload);
                                }
                            }
                            else //скачивание мода не удалось.
                            {
                                Runtime.DebugWrite("ERROR " + result.Value2 + " " + result.Value1);
                                noDownloaded.Add(file);
                            }

                            Runtime.DebugWrite("EXIT PERFOMER");
                        });

                        if (cancelToken.IsCancellationRequested) break;
                    }

                    if (!cancelToken.IsCancellationRequested)
                    {
                        perfomer?.WaitEnd();

                        Runtime.DebugWrite("ДОКАЧИВАЕМ " + noDownloaded.Count);
                        foreach (InstanceManifest.FileData file in noDownloaded)
                        {
                            if (cancelToken.IsCancellationRequested) break;

                            CurseforgeAddonInfo addonInfo = CurseforgeApi.GetAddonInfo(file.projectID);
                            if (addonInfo.latestFiles == null && addons.ContainsKey(file.projectID))
                            {
                                addonInfo = addons[file.projectID];
                            }

                            var taskArgs = new TaskArgs
                            {
                                PercentHandler = delegate (int percent)
                                {
                                    _fileDownloadHandler?.Invoke(addonInfo.name, percent, DownloadFileProgress.PercentagesChanged);
                                },
                                CancelToken = cancelToken
                            };

                            int count = 0;
                            SetValues<InstalledAddonInfo, DownloadAddonRes> result = CurseforgeApi.DownloadAddon(addonInfo, file.fileID, "/instances/" + instanceId + "/", taskArgs);

                            while (count < 4 && result.Value2 != DownloadAddonRes.Successful && !cancelToken.IsCancellationRequested)
                            {
                                Thread.Sleep(1000);
                                Runtime.DebugWrite("REPEAT DOWNLOAD " + addonInfo.id);
                                addonInfo = CurseforgeApi.GetAddonInfo(file.projectID);
                                result = CurseforgeApi.DownloadAddon(addonInfo, file.fileID, "/instances/" + instanceId + "/", taskArgs);

                                count++;
                            }

                            if (result.Value2 != DownloadAddonRes.Successful)
                            {
                                Runtime.DebugWrite("ХУЙНЯ, НЕ СКАЧАЛОСЬ " + file.projectID + " " + result.Value2);
                                if (addonInfo != null)
                                {
                                    errors.Add(addonInfo.name + ", File id: " + file.fileID);
                                }
                                else
                                {
                                    errors.Add("Project Id: " + file.projectID + ", File id: " + file.fileID);
                                }

                                _fileDownloadHandler?.Invoke(addonInfo.name, 100, DownloadFileProgress.Error);
                            }
                            else
                            {
                                compliteDownload.InstalledAddons[file.projectID] = result.Value1;
                                SaveInstanceContent(compliteDownload);

                                _fileDownloadHandler?.Invoke(addonInfo.name, 100, DownloadFileProgress.Successful);
                            }

                            downloadedCount++;
                            AddonsDownloadEventInvoke(filesCount, downloadedCount);
                        }
                    }
                }

                if (errors.Count == 0 && !cancelToken.IsCancellationRequested)
                {
                    compliteDownload.FullClient = true;
                }

                SaveInstanceContent(compliteDownload);
                Runtime.DebugWrite("END INSTALL INSTANCE");

                return errors;
            }
            catch
            {
                errors.Add("unknownError");
                return null;
            }
        }
    }
}
