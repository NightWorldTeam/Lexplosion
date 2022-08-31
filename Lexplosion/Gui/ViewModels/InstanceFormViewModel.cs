using Lexplosion.Gui.Models.InstanceForm;
using Lexplosion.Gui.ViewModels.ModalVMs;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Management.Instances;
using System;

namespace Lexplosion.Gui.ViewModels
{
    public sealed class InstanceFormViewModel : VMBase
    {
        private const string _deleteInstanceTitle = "Вы действительно желаете удалить ";
        private const string _deleteInstanceFromLibraryTitle = "Вы действительно желаете удалить {0} из библиотеки?";

        private InstanceClient _instanceClient; // Данные о Instance.


        #region props


        public InstanceClient Client
        {
            get => _instanceClient;
        }

        public MainViewModel MainVM { get; } // Ссылка на MainViewModel



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
                    UpdateUpperButtonFunc((UpperButtonFunc)obj);
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
                    UpdateLowerButtonFunc((LowerButtonFunc)obj);
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
            Model.IsCanRun = true;
            _instanceClient = instanceClient;
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

        public void DownloadInstance(ProgressHandlerCallback progressHandler = null, ComplitedDownloadCallback complitedDownload = null) 
        {
            if (!MainVM.Model.IsLibraryContainsInstance(_instanceClient))
                MainVM.Model.LibraryInstances.Add(this);

            if (progressHandler != null)
                Client.ProgressHandler += progressHandler;

            if (complitedDownload != null)
                Client.ComplitedDownload += complitedDownload;

            MainVM.DownloadManager.AddProcess(this);

            Model.DownloadModel.DonwloadPrepare();
        }

        /// <summary>
        /// Удаляет сборку из библиотеки, но перед этим спрашивает пользователя действительно ли он хочет удалить.
        /// </summary>
        /// <param name="IsFromLibrary">Если сборка не установленная но в библиотеки. Влияет на выводимое сообщение</param>
        public void RemoveInstance(bool IsFromLibrary) 
        {
            var dialog = new DialogViewModel(MainVM);
            string message;

            if (IsFromLibrary) 
            {
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
        public void RemoveInstance()
        {
            Model.InstanceClient.Delete();
            MainVM.Model.RemoveInstanceFromLibrary(Model.InstanceClient);
            Model.UpdateButtons();
        }

        #endregion Public & Protected Methods


        #region Private Methods


        private void UpdateUpperButtonFunc(UpperButtonFunc buttonFunc) 
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

        private void UpdateLowerButtonFunc(LowerButtonFunc buttonFunc) 
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
                        break;
                    }

                case LowerButtonFunc.Update:
                    {
                        Model.DownloadModel.DonwloadPrepare();
                        break;
                    }

                case LowerButtonFunc.OpenWebsite:
                    {
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
                        RemoveInstance(false);
                        break;
                    }

                case LowerButtonFunc.Export:
                    {
                        MainVM.ModalWindowVM.IsOpen = true;

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
        }


        #endregion Private Methods
    }
}
