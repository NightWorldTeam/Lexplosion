using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Lexplosion.Logic.Network.CurseforgeApi;


namespace Lexplosion.Logic.Management
{
    /// <summary>
    /// Структура файла, в котором хранятся установленные аддоны (installedAddons.json)
    /// </summary>
    public class InstalledAddons : Dictionary<int, InstalledAddonInfo> { }

    static class McAddonsManager
    {
        public static bool InstallAddon(int projectID, int fileID, string instanceId, string gameVersion)
        {
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

        public static void DisableAddon(string instanceId, int projectID)
        {
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

        public static void DeleteAddon(string instanceId, int projectID, InstalledAddonInfo installedAddon)
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
    }
}
