using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Tools;
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
        /// Выполняет извлечение основных данных сборки из ее файла
        /// </summary>
        /// <param name="instanceFileGetter">Делегат, выполняющий получение самого файла сборки</param>
        /// <param name="fileName">Имя файла сборки</param>
        /// <param name="localFiles">Локальные файлы, полученные из файла сборки</param>
        /// <param name="cancelToken">Токен отмены</param>
        /// <returns>
        /// Возвращает манифест, полученный из архива.
        /// </returns>
        public TManifest Extraction(InstanceFileGetter instanceFileGetter, ref InstanceContent localFiles, CancellationToken cancelToken);

        /// <summary>
        /// Скачивает все аддоны модпака из списка
        /// </summary>
        /// <returns>
        /// Возвращает список ошибок.
        /// </returns>
        public List<string> Install(TManifest data, InstanceContent localFiles, CancellationToken cancelToken);

        /// <summary>
        /// Устанавливает id для сборки и создает папку с ней
        /// </summary>
        public void SetInstanceId(string instanceId);

        public event Action<int> MainFileDownload;
        public event ProcentUpdate AddonsDownload;
    }

    /// <summary>
    /// Делегат, выполняющий получение файла модпака.
    /// </summary>
    /// <param name="tempDir">Путь до папки temp</param>
    /// <param name="taskArgsBuilder">Делегат, который создаст TaskArgs. Принимает имя файла модпака</param>
    /// <returns>Первое значение - удачен ли результат выполнения, второе - путь до файла модпака, третье - имя файла модпака</returns>
    public delegate (bool, string, string) InstanceFileGetter(string tempDir, Func<string, TaskArgs> taskArgsBuilder);
}
