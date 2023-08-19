using Lexplosion.Common.ModalWindow;
using Lexplosion.Controls;
using Lexplosion.Tools;

namespace Lexplosion.Common.ViewModels.ModalVMs.InstanceTransfer
{
    public abstract class ImportBase : ModalVMBase, INotifiable
    {
        private DoNotificationCallback _doNotification;
        public DoNotificationCallback DoNotification
        {
            get => _doNotification; protected set
            {
                _doNotification = value ?? ((header, message, time, type) => { });
            }
        }

        public ImportBase(DoNotificationCallback doNotification)
        {
            DoNotification = doNotification;
        }

        protected void DownloadResultHandler(ImportResult importResult)
        {
            var message = GetImportResultMessages(importResult);
            switch (importResult)
            {
                // todo: translate
                case ImportResult.Successful:
                    {
                        DoNotification("Все прекрасно)", message, 5, 0);
                    }
                    break;
                default:
                    {
                        DoNotification("Мы не знаем, как так получилось, но", message, 5, 1);
                    }
                    break;
            }
        }

        public static string GetImportResultMessages(ImportResult importResult)
        {
            switch (importResult)
            {
                case ImportResult.Successful: return ResourceGetter.GetString("downloadSuccessfullyCompleted");

                case ImportResult.ZipFileError: return ResourceGetter.GetString("importResultZipFileError");
                case ImportResult.GameVersionError: return ResourceGetter.GetString("importResultGameVersionError");
                case ImportResult.JavaDownloadError: return ResourceGetter.GetString("importResultJavaDownloadError");
                case ImportResult.IsOfflineMode: return ResourceGetter.GetString("importResultIsOfflineMode");

                case ImportResult.MovingFilesError: return ResourceGetter.GetString("importResultMovingFilesError");
                case ImportResult.DownloadError: return ResourceGetter.GetString("importResultDownloadError");
                case ImportResult.DirectoryCreateError: return ResourceGetter.GetString("importResultDirectoryCreateError");
                case ImportResult.Canceled: return ResourceGetter.GetString("importResultCanceled");

                default: return "Неизвестная ошибка!! лол";
            }
        }
    }
}
