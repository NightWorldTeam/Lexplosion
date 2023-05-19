using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Logic.Objects.Curseforge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Lexplosion.Logic.FileSystem.InstanceInstaller;

namespace Lexplosion.Logic.FileSystem
{
    interface IArchivedInstanceInstaller<TManifest>
    {
        public InstanceContent GetInstanceContent();
        public void SaveInstanceContent(InstanceContent content);
        public bool InvalidStruct(InstanceContent localFiles);
        public TManifest DownloadInstance(string downloadUrl, string fileName, ref InstanceContent localFiles, CancellationToken cancelToken);
        public List<string> InstallInstance(TManifest data, InstanceContent localFiles, CancellationToken cancelToken);

        public event Action<int> MainFileDownloadEvent;
        public event ProcentUpdate AddonsDownloadEvent;
    }
}
