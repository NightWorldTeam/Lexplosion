using Lexplosion.Logic.Management;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Tools;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Modal;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal.InstanceTransfer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using static Lexplosion.Logic.Management.ImportInterruption;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceTransfer
{
    public sealed class InstanceImportModel : ViewModelBase
    {
        private readonly Action<InstanceClient> _addToLibrary;
        private readonly Action<InstanceClient> _removeFromLibrary;
        private readonly AppCore _appCore;
        private IModalViewModel _currentModalViewModelBase;


        public ObservableCollection<ImportProcess> ImportProcesses { get; } = new();
        public Action<IEnumerable<string>> ImportAction { get; }

        public Queue<InstanceImportFillDataViewModel> FillDataViewModels { get; } = [];


        #region Constructors


        public InstanceImportModel(AppCore appCore, Action<InstanceClient> addToLibrary, Action<InstanceClient> removeFromLibrary)
        {
            _appCore = appCore;
            _addToLibrary = addToLibrary;
            _removeFromLibrary = removeFromLibrary;
            _appCore.ModalNavigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;

            ImportAction = (filePaths) =>
            {
                foreach (var path in filePaths)
                    Import(path);
            };
        }

        private void OnCurrentViewModelChanged()
        {
            _currentModalViewModelBase = _currentModalViewModelBase ?? _appCore.ModalNavigationStore.CurrentViewModel;
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
            var dynamicStateHandler = new DynamicStateData<ImportInterruption, InterruptionType>();

            dynamicStateHandler.StateChanged += OnImportDynamicStateHandlerStateChanged;

            instanceClient = InstanceClient.Import(path, (ir) =>
            {
                ImportResultHandler(ir, importFile, instanceClient);
            }, dynamicStateHandler.GetHandler);

            // Добавляем в библиотеку.
            // TODO: IMPORTANT синхронизировать import и instanceform.
            _addToLibrary(instanceClient);
        }

        private void OnImportDynamicStateHandlerStateChanged(ImportInterruption importInterruption, InterruptionType arg2)
        {
            // Если в очереди пусто, окрываем модальное окно.
            // Если в очереди есть элементы, добавляем новый в очередью.
            // Если элемены в очереди закончились, и на момент открытия
            // было открыто модальное окно импорта возвращаем пользователя туда
            // Иначе, закрываем все модальные окна.

            var vm = new InstanceImportFillDataViewModel(_appCore, (baseInstanceData) =>
            {
                importInterruption.BaseData = baseInstanceData;

                if (FillDataViewModels.Count > 0)
                {
                    var vm = FillDataViewModels.Dequeue();
                    _appCore.ModalNavigationStore.Open(vm);
                    return;
                }
                _appCore.ModalNavigationStore.Open(_currentModalViewModelBase);
            });

            if (FillDataViewModels.Count == 0)
            {
                _appCore.ModalNavigationStore.Open(vm);
            }
            else
            {
                FillDataViewModels.Enqueue(vm);
            }
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
