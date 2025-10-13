using Lexplosion.Logic.Management.Import;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.Modal;
using Lexplosion.UI.WPF.Mvvm.Models;
using Lexplosion.UI.WPF.Mvvm.Models.InstanceTransfer;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.Modal.InstanceTransfer
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
            get => RelayCommand.GetCommand<ImportProcess>(ref _cancelImportCommand, Model.OnImportCancelled);
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


        public InstanceImportViewModel(AppCore appCore, ImportStartFunc importStart, Func<IEnumerable<ImportProcess>> getActiveImports, Action<InstanceClient, ImportData?> addToLibrary, Action<InstanceClient> removeFromLibrary)
        {
            Model = new InstanceImportModel(appCore, importStart, getActiveImports, addToLibrary, removeFromLibrary);
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
