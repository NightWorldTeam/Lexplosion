using Lexplosion.Logic.Objects.CommonClientData;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Lexplosion.Logic.FileSystem
{
    interface IArchivedInstanceInstaller<TManifest>
    {
        public InstanceContent GetInstanceContent();

        public void SaveInstanceContent(InstanceContent content);

        /// <summary>
        /// Проверяет все ли файлы клиента присутсвуют
        /// </summary>
        public bool InvalidStruct(InstanceContent localFiles);

        /// <summary>
        /// Скачивает архив с модпаком.
        /// </summary>
        /// <returns>
        /// Возвращает манифест, полученный из архива.
        /// </returns>
        public TManifest DownloadInstance(string downloadUrl, string fileName, ref InstanceContent localFiles, CancellationToken cancelToken);

        /// <summary>
        /// Скачивает все аддоны модпака из спика
        /// </summary>
        /// <returns>
        /// Возвращает список ошибок.
        /// </returns>
        public List<string> InstallInstance(TManifest data, InstanceContent localFiles, CancellationToken cancelToken);

        public event Action<int> MainFileDownloadEvent;
        public event ProcentUpdate AddonsDownloadEvent;
    }
}
