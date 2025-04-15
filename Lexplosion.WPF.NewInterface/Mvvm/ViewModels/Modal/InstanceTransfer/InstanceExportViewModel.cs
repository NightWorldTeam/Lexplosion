using Lexplosion.Global;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core.Modal;
using Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceTransfer;
using System.IO;
using System.Windows.Forms;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal.InstanceTransfer
{
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
                saveFileDialog.Filter = "nwpk files (*.nwpk)|*.nwpk";
                saveFileDialog.FilterIndex = 2;
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.FileName = Model.InstanceName + ".nwpk";


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
