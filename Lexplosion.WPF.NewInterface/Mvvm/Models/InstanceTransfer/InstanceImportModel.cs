using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceTransfer
{
    public sealed class InstanceImportModel : ViewModelBase
    {
        private readonly Action<InstanceClient> _addToLibrary;
        private readonly Action<InstanceClient> _removeFromLibrary;


        public ObservableCollection<ImportProcess> ImportProcesses { get; } = new ();
        public Action<IEnumerable<string>> ImportAction { get; }


        #region Constructors


        public InstanceImportModel(Action<InstanceClient> addToLibrary, Action<InstanceClient> removeFromLibrary)
        {
            _addToLibrary = addToLibrary;
            _removeFromLibrary = removeFromLibrary;


            ImportAction = (filePaths) =>
            {
                foreach (var path in filePaths)
                    Import(path);
            };
        }


        #endregion Constructors


        #region Public Methods 


        /// <summary>
        /// Открывает модальное окно, для выбора файлов с PC.
        /// </summary>
        public void BrowseFiles()
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = Constants.ImportFileDialogFilters;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (dialog.FileName.EndsWith(Constants.ImportFileExtensionZip) || dialog.FileName.EndsWith(Constants.ImportFileExtensionNWPack) || dialog.FileName.EndsWith(Constants.ImportFileExtensionMRPack))
                    {
                        Import(dialog.FileName);
                    }
                }
            }
        }


        public void Import(string path)
        {
            var importFile = new ImportProcess(path);

            ImportProcesses.Add(importFile);

            InstanceClient instanceClient = null;
            // Запускаем импорт
            instanceClient = InstanceClient.Import(path, (ir) =>
            {
                ImportResultHandler(ir, importFile, instanceClient);
            });

            // Добавляем в библиотеку.
            // TODO: IMPORTANT синхронизировать import и instanceform.
            _addToLibrary(instanceClient);
        }

        public void CancelImport(ImportProcess importProcess) 
        {
            if (!importProcess.IsImporing)
                return;

            var index = ImportProcesses.IndexOf(importProcess);

            if (index == -1)
                return;

            ImportProcesses.RemoveAt(index);
        }


        #endregion Public Methods


        #region Private Methods


        private void ImportResultHandler(ImportResult importResult, ImportProcess importFile, InstanceClient instanceClient)
        {
            importFile.IsImporing = false;

            // TODO: Send Notification
            OnPropertyChanged(nameof(instanceClient.Name));

            if (importResult != ImportResult.Successful)
            {
                importFile.IsSuccessful = false;
                _removeFromLibrary(instanceClient);
            }
        }


        #endregion Private Methods
    }
}
