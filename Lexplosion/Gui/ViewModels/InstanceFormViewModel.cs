using Lexplosion.Gui.Extension;
using Lexplosion.Gui.Models;
using Lexplosion.Gui.Models.InstanceForm;
using Lexplosion.Gui.ViewModels.ModalVMs;
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
                        {
                            if (!_mainViewModel.Model.IsLibraryContainsInstance(_instanceClient))
                                _mainViewModel.Model.LibraryInstances.Add(this);
                            Model.DownloadModel.DonwloadPrepare();
                            break;
                        }
                    case UpperButtonFunc.ProgressBar:
                        {
                            // TODO: может сделать, что-то типо меню скачивания??
                            break;
                        }

                    case UpperButtonFunc.Play:
                        {
                            if (Model.IsCanRun)
                            {
                                Model.LaunchModel.LaunchInstance();

                                foreach (var instance in _mainViewModel.Model.LibraryInstances)
                                {
                                    if (instance != this)
                                        instance.Model.IsCanRun = false;
                                }
                            }
                            break;
                        }

                    case UpperButtonFunc.Close:
                        {
                            LaunchGame.GameStop();
                            Model.UpperButton.ChangeFuncPlay();
                            foreach (var instance in _mainViewModel.Model.LibraryInstances)
                            {
                                instance.Model.IsCanRun = true;
                            }
                            break;
                        }
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
                        {
                            IsDropdownMenuOpen = false;
                            break;
                        }

                    case LowerButtonFunc.DeleteFromLibrary:
                        {
                            IsDropdownMenuOpen = false;
                            Model.InstanceClient.Delete();
                            _mainViewModel.Model.RemoveInstanceFromLibrary(Model.InstanceClient);
                            break;
                        }

                    case LowerButtonFunc.OpenFolder:
                        {
                            IsDropdownMenuOpen = false;
                            Model.OpenInstanceFolder();
                            break;
                        }

                    case LowerButtonFunc.CancelDownload:

                        {
                            IsDropdownMenuOpen = false;
                            break;
                        }

                    case LowerButtonFunc.Update:
                        {
                            IsDropdownMenuOpen = false;
                            Model.DownloadModel.DonwloadPrepare();
                            break;
                        }

                    case LowerButtonFunc.OpenWebsite:
                        {
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
                        }

                    case LowerButtonFunc.RemoveInstance:
                        {
                            IsDropdownMenuOpen = false;
                            _mainViewModel.Model.RemoveInstanceFromLibrary(Model.InstanceClient);
                            Model.InstanceClient.Delete();
                            break;
                        }

                    case LowerButtonFunc.Export:
                        {
                            IsDropdownMenuOpen = false;
                            _mainViewModel.ModalWindowVM.IsModalOpen = true;

                            // возможно не надо вообще эксемпляр класса сохранять.
                            _mainViewModel.ExportViewModel = new ExportViewModel(_mainViewModel)
                            {
                                InstanceName = _instanceClient.Name,
                                IsFullExport = true,
                                InstanceClient = _instanceClient,
                                UnitsList = _instanceClient.GetPathContent()
                            };

                            _mainViewModel.ModalWindowVM.ChangeCurrentModalContent(_mainViewModel.ExportViewModel);

                            foreach (var s in _mainViewModel.ExportViewModel.UnitsList.Keys)
                            {
                                Console.WriteLine(s);
                            }
                            break;
                        }
                }
            }));
        }

        #endregion commands

        public InstanceFormViewModel(MainViewModel mainViewModel, InstanceClient instanceClient)
        {
            _mainViewModel = mainViewModel;
            Model = new InstanceFormModel(mainViewModel, instanceClient);
            Model.IsCanRun = true;
            _instanceClient = instanceClient;
        }
    }
}
