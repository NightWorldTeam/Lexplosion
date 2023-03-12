using Lexplosion.Gui.Models.InstanceForm;
using Lexplosion.Gui.ViewModels.ModalVMs;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Management.Instances;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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
            Model = new InstanceFormModel(mainViewModel, instanceClient, this);
            Client = instanceClient;
        }


        #endregion Contrusctors


        #region Public & Protected Methods


        /// <summary>
        /// Запускает сборку по данным формы.
        /// </summary>
        public void LaunchInstance(LaunchComplitedCallback complitedLaunchCallback = null, GameExitedCallback gameExitedCallback = null)
        {
            if (!MainVM.IsInstanceRunning)
            {
                if (complitedLaunchCallback != null && gameExitedCallback != null)
                {
                    Client.LaunchComplited += complitedLaunchCallback;
                    Client.GameExited += gameExitedCallback;
                }


                Model.InstanceClient.DownloadStarted += () =>
                {
                    MainVM.DownloadManager.AddProcess(this);

                    Model.UpperButton.ChangeFuncProgressBar();
                    Model.DownloadModel.HasProcents = true;
                    Model.DownloadModel.IsDownloadInProgress = true;
                };

                Model.InstanceClient.DownloadComplited += (InstanceInit result, List<string> downloadErrors, bool launchGame) =>
                {
                    Model.DownloadModel.HasProcents = false;
                    Model.DownloadModel.IsDownloadInProgress = false;
                    Model.IsLaunch = true;
                    Model.UpdateButtons();
                };

                Model.LaunchModel.LaunchInstance();
                Model.UpdateButtons();
            }
        }

        /// <summary>
        /// Закрывает сборку по данным формы.
        /// </summary>
        public void CloseInstance()
        {
            Model.InstanceClient.StopGame();
            Model.UpperButton.ChangeFuncPlay();
            MainVM.IsInstanceRunning = false;
            Model.DownloadModel.HasProcents = false;
            Model.OverviewField = Model.InstanceClient.Summary;
        }

        public void UpdateInstance()
        {
            Model.InstanceClient.Update();
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
            String message;

            if (IsFromLibrary)
            {
                // TODO: LOCALIZATE
                message = "Вы действительно желаете удалить  \"" + Model.InstanceClient.Name + "\"  из библиотеки?";
            }
            else
            {
                message = "Вы действительно желаете удалить  \"" + Model.InstanceClient.Name + "\"";
            }

            dialog.ShowDialog("Удаление сборки", message, RemoveInstance);
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
                IsFullExport = false,
                InstanceClient = Client,
                UnitsList = Client.GetPathContent()
            };

            MainVM.ModalWindowVM.ChangeCurrentModalContent(new CustomTabsMenuViewModel(
                new List<CustomTab>()
                {
                    new CustomTab(
                        "Export",
                        "M11 40q-1.2 0-2.1-.9Q8 38.2 8 37v-7.15h3V37h26v-7.15h3V37q0 1.2-.9 2.1-.9.9-2.1.9Zm13-7.65-9.65-9.65 2.15-2.15 6 6V8h3v18.55l6-6 2.15 2.15Z",
                        MainVM.ExportViewModel
                        ),
                    new CustomTab(
                        "Share",
                        "M6.54 55.08a1.91 1.91 0 0 1-.62-.1 2 2 0 0 1-1.38-2c0-.3 2.06-29.34 31.18-31.62V10.92a2 2 0 0 1 3.43-1.4l19.74 20.16a2 2 0 0 1 0 2.8L39.15 52.64a2 2 0 0 1-3.43-1.4V41c-19.44.74-27.41 13-27.49 13.15a2 2 0 0 1-1.69.93Zm33.18-39.26v7.41a2 2 0 0 1-1.93 2c-18.84.69-25.58 13.24-28 21.31 5-4.32 13.91-9.6 27.81-9.6h.09a2 2 0 0 1 2 2v7.41l15-15.26Z",
                        new ShareInstanceViewModel(), false
                        )
                }
                ));
        }

        #endregion Public & Protected Methods


        #region Private Methods


        private void ExecuteUpperButtonFunc(UpperButtonFunc buttonFunc)
        {
            Runtime.DebugWrite(buttonFunc.ToString());
            switch (buttonFunc)
            {
                case UpperButtonFunc.Download:
                    {
                        DownloadInstance();
                        break;
                    }
                case UpperButtonFunc.ProgressBar:
                    {
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
                        Model.DownloadModel.CancelInstanceDownload();
                        break;
                    }

                case LowerButtonFunc.Update:
                    {
                        if (!Model.DownloadModel.IsDownloadInProgress)
                        {
                            MainVM.DownloadManager.AddProcess(this);
                            Model.DownloadModel.DownloadPrepare();
                        }
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
