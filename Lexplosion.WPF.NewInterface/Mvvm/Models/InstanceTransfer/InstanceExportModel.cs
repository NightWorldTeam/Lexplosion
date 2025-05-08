using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Notifications;
using System;
using System.Threading.Tasks;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceTransfer
{
    public sealed class InstanceExportModel : ViewModelBase
    {
        // true - exporting | false - nothing
        public event Action<bool> ExportStatusChanged;


        private readonly AppCore _appCore;
        private readonly InstanceClient _instanceClient;


        #region Properties


        public InstanceFileTree InstanceFileTree { get; }
        public string InstanceName { get; set; }


        private bool _isFullExport;
        /// <summary>
        /// Выбран экспорт всех файлов?
        /// </summary>
        public bool IsFullExport
        {
            get => _isFullExport; set
            {
                _isFullExport = value;
                InstanceFileTree.ReselectedAllUnits(value);
            }
        }

        private bool _isExportStarted;
        /// <summary>
        /// Процесс экспорта был запущен.
        /// </summary>
        public bool IsExportActive
        {
            get => _isExportStarted; private set
            {
                _isExportStarted = value;
                OnPropertyChanged();
                ExportStatusChanged?.Invoke(value);
            }
        }


        #endregion Properties


        #region Constructors


        public InstanceExportModel(AppCore appCore, InstanceClient instanceClient)
        {
            _appCore = appCore;
            _instanceClient = instanceClient;
            InstanceName = _instanceClient.Name;
            InstanceFileTree = new InstanceFileTree(instanceClient);
        }


        #endregion Constructors


        #region Public Methods


        /// <summary>
        /// Запускает экспорт асинхронно. Возвращает результат экспорта.
        /// </summary>
        /// <param name="fileName">Название файла</param>
        /// <returns></returns>
        public async Task<ExportResult> Export(string fileName)
        {
            IsExportActive = true;

            return await Task.Run(() =>
            {
                var exportResult = _instanceClient.Export(InstanceFileTree.UnitsList, fileName, InstanceName);
                ShareResultHandler(exportResult);
                IsExportActive = false;
                return exportResult;
            });
        }


        #endregion Public Methods


        #region Private Methods


        private void ShareResultHandler(ExportResult exportResult)
        {
            switch (exportResult)
            {
                case ExportResult.Successful:
                    _appCore.MessageService.Success("Export_Sucessfully", true, InstanceName);
                    break;
                default:
                    _appCore.NotificationService.Notify(
                        new SimpleNotification(
                            string.Format(_appCore.Resources("InstanceShareColon_") as string, InstanceName),
                            _appCore.Resources($"Export{exportResult}") as string)
                        );
                    break;
            }
        }


        #endregion Private Methods
    }

}
