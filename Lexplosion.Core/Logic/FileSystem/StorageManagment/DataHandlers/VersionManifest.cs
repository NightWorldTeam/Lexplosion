using Lexplosion.Logic.Objects.CommonClientData;
using System.Collections.Generic;

namespace Lexplosion.Logic.FileSystem.StorageManagment.DataHandlers
{
    class VersionManifestHandler : JsonDataFile, IDataHandler<VersionManifest>
    {
        private bool _includingLibraries;
        private string _instanceId;

        public VersionManifestHandler(string instanceId, bool inlcludingLibraries)
        {
            _includingLibraries = inlcludingLibraries;
            _instanceId = instanceId;
        }

        public VersionManifest LoadFromStorage()
        {
            string dir = WithDirectory.DirectoryPath;

            VersionManifest data = LoadFromFile<VersionManifest>(dir + "/instances/" + _instanceId + "/" + "manifest.json");
            if (data == null)
            {
                return null;
            }

            if (_includingLibraries)
            {
                var librariesData = LoadFromFile<Dictionary<string, LibInfo>>(dir + "/versions/libraries/" + data.version.GetLibName + ".json") ?? new Dictionary<string, LibInfo>();

                var installer = data.version?.additionalInstaller;
                if (installer != null)
                {
                    var additionallibrarieData = LoadFromFile<Dictionary<string, LibInfo>>(dir + "/versions/additionalLibraries/" + installer?.GetLibName + ".json");

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

        public void SaveToStorage(VersionManifest data)
        {
            string dir = WithDirectory.DirectoryPath;

            SaveToFile<VersionManifest>(data, dir + "/instances/" + _instanceId + "/" + "manifest.json");

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

                    SaveToFile<Dictionary<string, LibInfo>>(baseLibs, dir + "/versions/libraries/" + data.version.GetLibName + ".json");

                    if (additionalLibs != null && additionalLibs.Count > 0)
                    {
                        SaveToFile<Dictionary<string, LibInfo>>(additionalLibs, dir + "/versions/additionalLibraries/" + data.version.additionalInstaller.GetLibName + ".json");
                    }
                }
                else
                {
                    SaveToFile<Dictionary<string, LibInfo>>(data.libraries, dir + "/versions/libraries/" + data.version.GetLibName + ".json");
                }
            }
        }
    }

    struct VersionManifestArgs : IDataHandlerArgs<VersionManifest>
    {
        private VersionManifestHandler _handler;

        public VersionManifestArgs(string instanceId, bool inlcludingLibraries = true)
        {
            _handler = new VersionManifestHandler(instanceId, inlcludingLibraries);
        }

        public IDataHandler<VersionManifest> Handler { get => _handler; }
    }
}
