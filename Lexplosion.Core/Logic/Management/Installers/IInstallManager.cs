using System;
using Lexplosion.Logic.Objects.CommonClientData;

namespace Lexplosion.Logic.Management.Installers
{
    public interface IInstallManager
    {
        /// <summary>
        /// Проверяет сборку на обновления.
        /// </summary>
        /// <param name="releaseIndex">Сюда возвращается редизный индекс игры. Это нужно чтобы определить версию джавы.</param>
        /// <param name="instanceVersion">
        /// Сюда пихать версию сборки, которая нужна. Если это не надо, то просто null. 
        /// При передачи сюда ненулевого параметра по возможности метод Update установит эту версию(ну если исключений не будет).
        /// Если параметр нулл, то он будет проигнорирован.
        /// </param>
        /// <returns>Результат проверки.</returns>
        InstanceInit Check(out long releaseIndex, string instanceVersion);

        InitData Update(string javaPath, ProgressHandlerCallback progressHandler); // TODO: сделать так, чтобы при неудачном скачивании некоторых файлов он сохранял результат работы, чтобы не перекачивать всё снова

        event Action<string, int, DownloadFileProgress> FileDownloadEvent;
        event Action DownloadStarted;
    }
}
