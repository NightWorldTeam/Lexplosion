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
        private const string _deleteInstanceTitle = "Вы действительно желаете удалить {0}? \n\nможно потом сделать выбор того что можно сохранить? например карты)))";
        private const string _deleteInstanceFromLibraryTitle = "Вы действительно желаете удалить {0} из библиотеки? Ну тут вряд-ли что-то ещё надо, ибо сборка не установленна";

        private InstanceClient _instanceClient; // Данные о Instance.

        public InstanceClient Client
        {
            get => _instanceClient;
        }

        public MainViewModel MainVM { get; } // Ссылка на MainViewModel

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
                            if (!MainVM.Model.IsLibraryContainsInstance(_instanceClient))
                                MainVM.Model.LibraryInstances.Add(this);
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
                            if (!MainVM.IsInstanceRunning)
                            {
                                Model.LaunchModel.LaunchInstance();
                            }
                            break;
                        }

                    case UpperButtonFunc.Close:
                        {
                            LaunchGame.GameStop();
                            Model.UpperButton.ChangeFuncPlay();
                            MainVM.IsInstanceRunning = false;
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
                            var dialog = new DialogViewModel(MainVM);
                            dialog.ShowDialog(String.Format(_deleteInstanceTitle, Model.InstanceClient.Name),
                                new Action(delegate ()
                                {
                                    Model.InstanceClient.Delete();
                                    MainVM.Model.RemoveInstanceFromLibrary(Model.InstanceClient);
                                })
                            );
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
                            var dialog = new DialogViewModel(MainVM);
                            dialog.ShowDialog(String.Format(_deleteInstanceTitle, Model.InstanceClient.Name),
                                new Action(delegate ()
                                    {
                                        MainVM.Model.RemoveInstanceFromLibrary(Model.InstanceClient);
                                        Model.InstanceClient.Delete();
                                    })
                                );
                            break;
                        }

                    case LowerButtonFunc.Export:
                        {
                            IsDropdownMenuOpen = false;
                            MainVM.ModalWindowVM.IsModalOpen = true;

                            // возможно не надо вообще эксемпляр класса сохранять.
                            MainVM.ExportViewModel = new ExportViewModel(MainVM)
                            {
                                InstanceName = _instanceClient.Name,
                                IsFullExport = true,
                                InstanceClient = _instanceClient,
                                UnitsList = _instanceClient.GetPathContent()
                            };

                            MainVM.ModalWindowVM.ChangeCurrentModalContent(MainVM.ExportViewModel);

                            foreach (var s in MainVM.ExportViewModel.UnitsList.Keys)
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
            MainVM = mainViewModel;
            Model = new InstanceFormModel(mainViewModel, instanceClient);
            Model.IsCanRun = true;
            _instanceClient = instanceClient;
        }
    }
}
