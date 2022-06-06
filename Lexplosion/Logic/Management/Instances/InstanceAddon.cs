using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.Curseforge;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.FileSystem;
using static Lexplosion.Logic.Network.CurseforgeApi;

namespace Lexplosion.Logic.Management.Instances
{
    class InstanceAddon
    {
        public string Name { get; private set; }
        public string Author { get; private set; }
        public byte[] Logo { get; private set; } = null;
        public bool IsInstalled { get; private set; } = false;
        public bool UpdateAvailable { get; private set; }
        public string WebsiteUrl { get; private set; } = null;

        private readonly CurseforgeAddonInfo _modInfo;
        private readonly BaseInstanceData _modpackInfo;

        private InstanceAddon(CurseforgeAddonInfo modInfo, BaseInstanceData modpackInfo)
        {
            _modInfo = modInfo;
            _modpackInfo = modpackInfo;
            Name = modInfo.name;
        }

        public static List<InstanceAddon> GetAddons(BaseInstanceData modpackInfo, int pageSize, int index, AddonType type, string searchFilter = "")
        {
            string instanceId = modpackInfo.LocalId;
            var addons = new List<InstanceAddon>();

            // получаем спсиок всех аддонов с курсфорджа
            List<CurseforgeAddonInfo> addonsList = CurseforgeApi.GetAddonsList(pageSize, index, type, searchFilter);
            if (addonsList == null)
                return addons;

            // получаем список установленных аддонов
            var installedAddons = DataFilesManager.GetFile<InstalledAddons>(WithDirectory.DirectoryPath + "/instances/" + instanceId + "/installedAddons.json");

            // проходимся по аддонам с курсфорджа
            int i = 0;
            foreach (CurseforgeAddonInfo addon in addonsList)
            {
                bool isInstalled = 
                    (installedAddons.ContainsKey(addon.id) && 
                    File.Exists(WithDirectory.DirectoryPath + "/instances/" + instanceId + "/" + installedAddons[addon.id].ActualPath));

                int lastFileID = 0;
                if (isInstalled)
                {
                    // ищем последнюю версию аддона
                    foreach (var addonVersion in addon.gameVersionLatestFiles)
                    {
                        if (addonVersion.gameVersion == modpackInfo.GameVersion)
                        {
                            lastFileID = addonVersion.projectFileId;
                            break;
                        }
                    }
                }

                var instanceAddon = new InstanceAddon(addon, modpackInfo)
                {
                    IsInstalled = isInstalled,
                    Author = addon.GetAuthorName,
                    WebsiteUrl = addon.websiteUrl,
                    UpdateAvailable = (installedAddons[addon.id].FileID < lastFileID) // если установленная версия аддона меньше последней - значит доступно обновление
                };

                addons.Add(instanceAddon);
                i++;
            }

            return addons;
        }

        private void DeleteAddon(string instanceId, int projectID, InstalledAddonInfo installedAddon)
        {
            try
            {
                string path = WithDirectory.DirectoryPath + "/instances/" + instanceId + "/" + installedAddon.ActualPath;

                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch { }
        }

        private bool InstallAddon(int fileID)
        {
            int projectID = _modInfo.id;
            string instanceId = _modpackInfo.LocalId;
            string gameVersion = _modpackInfo.GameVersion;

            var installedAddons = DataFilesManager.GetFile<InstalledAddons>(WithDirectory.DirectoryPath + "/instances/" + instanceId + "/installedAddons.json");
            if (installedAddons == null)
            {
                installedAddons = new InstalledAddons();
            }

            bool addonAlreadyInstalled = installedAddons.ContainsKey(projectID);

            var addonsList = CurseforgeApi.DownloadAddon(projectID, fileID, "instances/" + instanceId + "/", !addonAlreadyInstalled, gameVersion);

            foreach (string file in addonsList.Keys)
            {
                if (addonsList[file].Item2 == DownloadAddonRes.Successful)
                {
                    if (addonsList[file].Item1.ProjectID == projectID && addonAlreadyInstalled)
                    {
                        DeleteAddon(instanceId, projectID, installedAddons[projectID]);
                    }

                    installedAddons[addonsList[file].Item1.ProjectID] = addonsList[file].Item1;
                }
            }

            DataFilesManager.SaveFile(WithDirectory.DirectoryPath + "/instances/" + instanceId + "/installedAddons.json", JsonConvert.SerializeObject(installedAddons));

            return true;
        }

        public void InstallLatestVersion()
        {
            int fileID = 0;
            foreach (var fileInfo in _modInfo.gameVersionLatestFiles)
            {
                if (fileInfo.gameVersion == _modpackInfo.GameVersion)
                {
                    fileID = fileInfo.projectFileId;
                    break;
                }
            }

            InstallAddon(fileID);
        }

        public void Disable()
        {
            int projectID = _modInfo.id;
            string instanceId = _modpackInfo.LocalId;

            var installedAddons = DataFilesManager.GetFile<InstalledAddons>(WithDirectory.DirectoryPath + "/instances/" + instanceId + "/installedAddons.json");
            if (installedAddons != null && installedAddons.ContainsKey(projectID))
            {
                try
                {
                    var installedAddon = installedAddons[projectID];
                    if (installedAddon.IsDisable)
                    {
                        string dir = WithDirectory.DirectoryPath + "/instances/" + instanceId + "/";
                        if (File.Exists(dir + installedAddon.ActualPath))
                        {
                            File.Move(dir + installedAddon.ActualPath, dir + installedAddon.Path);
                            installedAddon.IsDisable = false;

                            DataFilesManager.SaveFile(WithDirectory.DirectoryPath + "/instances/" + instanceId + "/installedAddons.json", JsonConvert.SerializeObject(installedAddons));
                        }
                    }
                    else
                    {
                        string dir = WithDirectory.DirectoryPath + "/instances/" + instanceId + "/";
                        if (File.Exists(dir + installedAddon.Path))
                        {
                            installedAddon.IsDisable = true;
                            File.Move(dir + installedAddon.Path, dir + installedAddon.ActualPath);

                            DataFilesManager.SaveFile(WithDirectory.DirectoryPath + "/instances/" + instanceId + "/installedAddons.json", JsonConvert.SerializeObject(installedAddons));
                        }
                    }
                }
                catch { }
            }
        }
    }
}
