using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Core;
using System;
using System.Threading.Tasks;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceTransfer
{
    public sealed class InstanceExportModel : ViewModelBase
    {
        // true - exporting | false - nothing
        public event Action<bool> ExportStatusChanged;


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


        public InstanceExportModel(InstanceClient instanceClient)
        {
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
                IsExportActive = false;
                return exportResult;
            });
        }


        #endregion Public Methods
    }

}
