using Lexplosion.Common.ModalWindow;
using Lexplosion.Controls;

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
                case ImportResult.Successful:
                    {
                        DoNotification("Все прекрасно)", message, 5, 0);
                    }
                    break;
                default:
                    {
                        DoNotification("Мы не знаем, как так получилось, но", message, 5, 0);
                    }
                    break;
            }
        }

        public static string GetImportResultMessages(ImportResult importResult) 
        {
            switch (importResult) 
            {
                case ImportResult.Successful:  return "Скачивание было успешно завершено";

                case ImportResult.ZipFileError:  return "Ошибка открытия zip файла";
                case ImportResult.GameVersionError:  return "Ошибка версии игры";
                case ImportResult.JavaDownloadError:  return "Проблема со скачиванием Java";
                case ImportResult.IsOfflineMode:  return "Оффлайн мод";

                case ImportResult.MovingFilesError:  return "Ошибка при перемещении файлов";
                case ImportResult.DownloadError:  return "Ошибка скачивания";
                case ImportResult.DirectoryCreateError:  return "Ошибка с создание директории";
                case ImportResult.Canceled:  return "Отменено";

                default: return "Неизвестная ошибка!! лол";
            }
        }
    }
}
