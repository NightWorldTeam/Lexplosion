using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lexplosion.Logic.Objects.Curseforge;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.FileSystem;
using static Lexplosion.Logic.Network.CurseforgeApi;
using System.IO;
using Newtonsoft.Json;
using Lexplosion.Logic.Objects;

namespace Lexplosion.Logic.Management.Instances
{
    class InstanceAddon
    {
        //public static void GetAddonsLis()
        public string Name { get; private set; }
        public string Author { get; private set; }

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
            var addons = new List<InstanceAddon>();

            List<CurseforgeAddonInfo> addonsList = CurseforgeApi.GetAddonsList(pageSize, index, type, searchFilter);
            if (addonsList == null)
                return addons;

            int i = 0;
            foreach (CurseforgeAddonInfo addon in addonsList)
            {
                addons.Add(new InstanceAddon(addon, modpackInfo));
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
    }
}
