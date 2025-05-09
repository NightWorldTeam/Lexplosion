using Lexplosion.Logic.Management.Import;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Modal;
using Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceTransfer;
using System;
using System.Windows.Forms;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal.InstanceTransfer
{
    public sealed class InstanceImportViewModel : ActionModalViewModelBase
    {
        public InstanceImportModel Model { get; }


        #region Commands


        private RelayCommand _browseFileFromLocalCommand;
        public ICommand BrowseFileFromLocalCommand 
        {
            get => RelayCommand.GetCommand(ref _browseFileFromLocalCommand, () => 
            {
                Model.BrowseFiles();
            });
        }



        private RelayCommand _cancelImportCommand;
        /// <summary>
        /// Отменяет импорт клиента.
        /// </summary>
        public ICommand CancelImportCommand
        {
            get => RelayCommand.GetCommand<ImportProcess>(ref _cancelImportCommand, Model.CancelImport);
        }


        private RelayCommand _importByUrlCommand;
        /// <summary>
        /// Запускает импорт по ссылке
        /// </summary>
        public ICommand ImportByUrlCommand 
        {
            get => RelayCommand.GetCommand(ref _importByUrlCommand, Model.ImportByUrl);
        }


        #endregion Commands


        #region Contructors


        public InstanceImportViewModel(AppCore appCore, Action<InstanceClient, ImportData?> addToLibrary, Action<InstanceClient> removeFromLibrary)
        {
            Model = new InstanceImportModel(appCore, addToLibrary, removeFromLibrary);
            ActionCommandExecutedEvent += Action;
        }


        #endregion Contructors


        #region Private Methods


        private void Action(object obj)
        {
            using (var dialog = new OpenFileDialog()) 
            {
                dialog.Filter = Constants.ImportFileDialogFilters;

                if (dialog.ShowDialog() == DialogResult.OK) 
                {
                    if (dialog.FileName.EndsWith(Constants.ImportFileExtensionZip) || dialog.FileName.EndsWith(Constants.ImportFileExtensionNWPack) || dialog.FileName.EndsWith(Constants.ImportFileExtensionMRPack))
                    {
                        Model.Import(dialog.FileName);
                    }
                }
            }
        }


        #endregion Private Methods
    }
}
