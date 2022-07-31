using Lexplosion.Gui.Extension;
using Lexplosion.Gui.Models;
using Lexplosion.Gui.Models.InstanceForm;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Management.Instances;
using System;
using System.Linq;

namespace Lexplosion.Gui.ViewModels
{
    public class InstanceFormViewModel : VMBase
    {
        private InstanceClient _instanceClient; // Данные о Instance.

        public InstanceClient Client 
        {
            get => _instanceClient;
        }

        private readonly MainViewModel _mainViewModel; // Ссылка на MainViewModel

        #region props

        /// <summary>
        /// Свойство модели InstanceForm
        /// </summary>
        public InstanceFormModel Model { get; }

        /// <summary>
        /// Свойтсво отвечает за видимость DropdownMenu.
        /// Создано для возможности вручную скрывать DropdownMenu.
        /// </summary>
        private bool _isDropdownMenuOpen;
        public bool IsDropdownMenuOpen
        {
            get => _isDropdownMenuOpen; set
            {
                _isDropdownMenuOpen = value;
                OnPropertyChanged();
            }
        }

        #endregion props

        #region commands

        /// <summary>
        /// Команда отвечает за функционал верхней кнопки в форме.
        /// </summary>
        private RelayCommand _upperBtnCommand;
        public RelayCommand UpperBtnCommand
        {
            get => _upperBtnCommand ?? (_upperBtnCommand = new RelayCommand(obj =>
            {
                switch ((UpperButtonFunc)obj)
                {
                    case UpperButtonFunc.Download:
                        if (!_mainViewModel.Model.IsLibraryContainsInstance(_instanceClient))
                            _mainViewModel.Model.LibraryInstances.Add(this);
                        Model.DownloadModel.DonwloadPrepare();
                        break;
                    case UpperButtonFunc.ProgressBar:
                        // TODO: может сделать, что-то типо меню скачивания??
                        break;
                    case UpperButtonFunc.Play:
                        Model.LaunchModel.LaunchInstance();
                        break;
                    case UpperButtonFunc.Close:
                        LaunchGame.GameStop();
                        Model.UpperButton.ChangeFuncPlay();
                        break;
                }
            }));
        }

        /// <summary>
        /// Команда отвечает за функционал кнопок в DropdownMenu
        /// </summary>
        private RelayCommand _lowerBtnCommand;
        public RelayCommand LowerBtnCommand
        {
            get => _lowerBtnCommand ?? (_lowerBtnCommand = new RelayCommand(obj =>
            {
                Console.WriteLine(((LowerButtonFunc)obj).ToString());
                switch ((LowerButtonFunc)obj)
                {
                    case LowerButtonFunc.AddToLibrary:
                        IsDropdownMenuOpen = false;
                        break;
                    case LowerButtonFunc.DeleteFromLibrary:
                        IsDropdownMenuOpen = false;
                        Model.InstanceClient.Delete();
                        _mainViewModel.Model.RemoveInstanceFromLibrary(Model.InstanceClient);
                        break;
                    case LowerButtonFunc.OpenFolder:
                        IsDropdownMenuOpen = false;
                        Model.OpenInstanceFolder();
                        break;
                    case LowerButtonFunc.CancelDownload:
                        IsDropdownMenuOpen = false;
                        break;
                    case LowerButtonFunc.Update:
                        IsDropdownMenuOpen = false;
                        Model.DownloadModel.DonwloadPrepare();
                        break;
                    case LowerButtonFunc.OpenWebsite:
                        IsDropdownMenuOpen = false;
                        try
                        {
                            System.Diagnostics.Process.Start(_instanceClient.WebsiteUrl);
                        }
                        catch
                        {
                            // message box here.
                        }
                        break;
                    case LowerButtonFunc.RemoveInstance:
                        IsDropdownMenuOpen = false;
                        _mainViewModel.Model.RemoveInstanceFromLibrary(Model.InstanceClient);
                        Model.InstanceClient.Delete();
                        break;
                    case LowerButtonFunc.Export:
                        IsDropdownMenuOpen = false;
                        _mainViewModel.IsExporting = true;
                        _mainViewModel.InstanceExport.InstanceName = _instanceClient.Name;
                        _mainViewModel.InstanceExport.IsFullExport = true;
                        _mainViewModel.InstanceExport.InstanceClient = _instanceClient;
                        _mainViewModel.InstanceExport.UnitsList = _instanceClient.GetPathContent();

                        foreach (var s in _mainViewModel.InstanceExport.UnitsList.Keys) 
                        {
                            Console.WriteLine(s);
                        }
                        break;
                }
            }));
        }

        #endregion commands

        public InstanceFormViewModel(MainViewModel mainViewModel, InstanceClient instanceClient)
        {
            _mainViewModel = mainViewModel;
            Model = new InstanceFormModel(instanceClient);
            _instanceClient = instanceClient;
        }
    }
}
