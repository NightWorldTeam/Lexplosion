using Lexplosion.Gui.Models.InstanceForm;
using Lexplosion.Gui.ViewModels.ModalVMs;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Management.Instances;
using System;
using System.Collections.Generic;

namespace Lexplosion.Gui.ViewModels
{
    public sealed class InstanceFormViewModel : VMBase
    {
        #region Properties


        public MainViewModel MainVM { get; } // Ссылка на MainViewModel
        public InstanceClient Client { get; } // Ссылка на InstanceClient
        public InstanceFormModel Model { get; } // Ссылка на InstanceFormModel

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


        #endregion Properties


        #region Commands


        private RelayCommand _upperBtnCommand;
        /// <summary>
        /// Команда отвечает за функционал верхней кнопки в форме.
        /// </summary>
        public RelayCommand UpperBtnCommand
        {
            get => _upperBtnCommand ?? (_upperBtnCommand = new RelayCommand(obj =>
            {
                try 
                {
                    ExecuteUpperButtonFunc((UpperButtonFunc)obj);
                }
                catch { }
            }));
        }

        private RelayCommand _lowerBtnCommand;
        /// <summary>
        /// Команда отвечает за функционал кнопок в DropdownMenu
        /// </summary>
        public RelayCommand LowerBtnCommand
        {
            get => _lowerBtnCommand ?? (_lowerBtnCommand = new RelayCommand(obj =>
            {   
                try 
                {
                    ExecuteLowerButtonFunc((LowerButtonFunc)obj);
                }
                catch { }
            }));
        }


        #endregion Commands


        #region Constructors


        public InstanceFormViewModel(MainViewModel mainViewModel, InstanceClient instanceClient)
        {
            MainVM = mainViewModel;
            Model = new InstanceFormModel(mainViewModel, instanceClient);
            Client = instanceClient;
        }


        #endregion Contrusctors


        #region Public & Protected Methods


        /// <summary>
        /// Запускает сборку по данным формы.
        /// </summary>
        public void LaunchInstance(ComplitedLaunchCallback complitedLaunchCallback = null, GameExitedCallback gameExitedCallback = null) 
        {
            if (!MainVM.IsInstanceRunning)
            {
                if (complitedLaunchCallback != null && gameExitedCallback != null) 
                { 
                    Client.ComplitedLaunch += complitedLaunchCallback;
                    Client.GameExited += gameExitedCallback;
                }
                MainVM.IsInstanceRunning = true;

                Model.InstanceClient.DownloadStartedEvent += () => 
                {
                    MainVM.DownloadManager.AddProcess(this);

                    Model.UpperButton.ChangeFuncProgressBar();
                    Model.DownloadModel.IsDownloadInProgress = true;
                    Model.UpdateLowerButton();
                };

                Model.LaunchModel.LaunchInstance();
            }
        }

        /// <summary>
        /// Закрывает сборку по данным формы.
        /// </summary>
        public void CloseInstance() 
        {
            LaunchGame.GameStop();
            Model.UpperButton.ChangeFuncPlay();
            MainVM.IsInstanceRunning = false;
        }

        public void UpdateInstance() 
        {
            Model.InstanceClient.UpdateInstance();
        }

        internal void DownloadInstance(Action<DownloadStageTypes, ProgressHandlerArguments> progressHandler = null, Action<InstanceInit, List<string>, bool> complitedDownload = null, string version = null) 
        {
            if (progressHandler != null)
                Model.DownloadModel.DownloadActions.Add(progressHandler);

            if (complitedDownload != null)
                Model.DownloadModel.ComplitedDownloadActions.Add(complitedDownload);

            if (!Model.DownloadModel.IsDownloadInProgress)
            {
                if (!MainVM.Model.IsLibraryContainsInstance(Client))
                    MainVM.Model.LibraryInstances.Add(this);
                MainVM.DownloadManager.AddProcess(this);
                Model.DownloadModel.DownloadPrepare(version);
            }
        }

        /// <summary>
        /// Удаляет сборку из библиотеки, но перед этим спрашивает пользователя действительно ли он хочет удалить.
        /// </summary>
        /// <param name="IsFromLibrary">Если сборка не установленная но в библиотеки. Влияет на выводимое сообщение</param>
        internal void RemoveInstance(bool IsFromLibrary) 
        {
            var dialog = new DialogViewModel(MainVM);
            string message;

            if (IsFromLibrary) 
            {
                // TODO: LOCALIZATE
                message = "Вы действительно желаете удалить " + Model.InstanceClient.Name + " из библиотеки?";
            }
            else 
            {
                message = "Вы действительно желаете удалить " + Model.InstanceClient.Name;
            }

            dialog.ShowDialog(message, RemoveInstance);
        }

        /// <summary>
        /// Удаляет сборку из библиотеки.
        /// </summary>
        internal void RemoveInstance()
        {
            Model.InstanceClient.Delete();
            MainVM.Model.RemoveInstanceFromLibrary(Model.InstanceClient);
            Model.UpdateButtons();
        }


        internal void OpenWebsite() 
        {
            try
            {
                System.Diagnostics.Process.Start(Client.WebsiteUrl);
            }
            catch
            {
                // message box here.
            }
        }

        internal void StartExportAndOpenModal() 
        {
            MainVM.ModalWindowVM.IsOpen = true;

            // возможно не надо вообще эксемпляр класса сохранять.
            MainVM.ExportViewModel = new ExportViewModel(MainVM)
            {
                InstanceName = Client.Name,
                IsFullExport = true,
                InstanceClient = Client,
                UnitsList = Client.GetPathContent()
            };

            MainVM.ModalWindowVM.ChangeCurrentModalContent(MainVM.ExportViewModel);
        }

        #endregion Public & Protected Methods


        #region Private Methods


        private void ExecuteUpperButtonFunc(UpperButtonFunc buttonFunc) 
        {
            switch (buttonFunc)
            {
                case UpperButtonFunc.Download:
                    {
                        DownloadInstance();
                        break;
                    }
                case UpperButtonFunc.ProgressBar:
                    {
                        // TODO: может сделать, что-то типо меню скачивания??
                        // ну да просто добавим открытие downloadmanager
                        MainVM.ModalWindowVM.OpenWindow(MainVM.DownloadManager);
                        break;
                    }

                case UpperButtonFunc.Play:
                    {
                        LaunchInstance();
                        break;
                    }

                case UpperButtonFunc.Close:
                    {
                        CloseInstance();
                        break;
                    }
            }
        }

        private void ExecuteLowerButtonFunc(LowerButtonFunc buttonFunc) 
        {
            IsDropdownMenuOpen = false;
            switch (buttonFunc)
            {
                case LowerButtonFunc.AddToLibrary:
                    {
                        Client.AddToLibrary();
                        break;
                    }

                case LowerButtonFunc.DeleteFromLibrary:
                    {
                        RemoveInstance(true);
                        break;
                    }

                case LowerButtonFunc.OpenFolder:
                    {
                        Model.OpenInstanceFolder();
                        break;
                    }

                case LowerButtonFunc.CancelDownload:
                    {
                        //Client.CancelDownload();
                        break;
                    }

                case LowerButtonFunc.Update:
                    {
                        Model.DownloadModel.DownloadPrepare();
                        break;
                    }

                case LowerButtonFunc.OpenWebsite:
                    {
                        OpenWebsite();
                        break;
                    }

                case LowerButtonFunc.RemoveInstance:
                    {
                        RemoveInstance(false);
                        break;
                    }

                case LowerButtonFunc.OpenDLCPage:
                    {
                        MainVM.MainMenuVM.OpenModpackPage(this, true);
                        break;
                    }

                case LowerButtonFunc.Export:
                    {
                        StartExportAndOpenModal();
                        break;
                    }
            }
        }


        #endregion Private Methods
    }
}
