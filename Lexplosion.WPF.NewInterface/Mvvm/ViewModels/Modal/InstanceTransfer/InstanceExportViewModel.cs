using Lexplosion.Global;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Modal;
using Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceTransfer;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal.InstanceTransfer
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

    public sealed class InstanceExportViewModel : ActionModalViewModelBase
    {
        private readonly InstanceClient _instanceClient;


        #region Properties


        public InstanceExportModel Model { get; }


        #endregion Properties


        #region Commands


        private RelayCommand _treeViewItemExpandCommand;
        public ICommand TreeViewItemExpandCommand 
        {
            get => _treeViewItemExpandCommand ?? (_treeViewItemExpandCommand = new RelayCommand(obj => 
            {
                Model.InstanceFileTree.SubTreeExpand(obj as PathLevel);
            })); 
        }


        #endregion Commands


        #region Constructors


        public InstanceExportViewModel(InstanceClient instanceClient)
        {
            _instanceClient = instanceClient;
            Model = new InstanceExportModel(instanceClient);
            ActionCommandExecutedEvent += OnActionCommandExecuted;
        }


        #endregion Constructors


        #region Private Methods


        /// <summary>
        /// Создаёт директорию по умолчанию для экспорта - "LauncherFile"/export.
        /// После того, как пользователь выбрал файл, запускает метод Export, после чего обрабатывает результат.
        /// </summary>
        /// <param name="obj">Параметр команды ActionCommand</param>
        private void OnActionCommandExecuted(object obj)
        {
            using (var saveFileDialog = new System.Windows.Forms.SaveFileDialog()) 
            {
                Runtime.DebugWrite(GlobalData.GeneralSettings.GamePath);

                var exportDirPath = GlobalData.GeneralSettings.GamePath.Replace('/', '\\') + @"\export";
                
                // Проверяем существует ли директория по умолчанию.
                if (!Directory.Exists(exportDirPath))
                    // Создаём директорию по умолчанию.
                    Directory.CreateDirectory(exportDirPath);

                saveFileDialog.InitialDirectory = exportDirPath;
                saveFileDialog.Filter = "zip files (*.zip)|*.zip";
                saveFileDialog.FilterIndex = 2;
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.FileName = _instanceClient.LocalId + ".zip";


                if (saveFileDialog.ShowDialog() == DialogResult.OK) 
                {
                    // ExportedInstance.Add(this);

                    var result = Model.Export(saveFileDialog.FileName);
                    Runtime.DebugWrite(result);
                    // result hanlder
                }
            }
        }


        #endregion Private Methods
    }
}
