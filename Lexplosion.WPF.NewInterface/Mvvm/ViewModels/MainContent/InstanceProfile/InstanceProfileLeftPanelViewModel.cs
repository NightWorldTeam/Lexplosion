using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core.Tools;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using Lexplosion.WPF.NewInterface.Stores;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading;
using System;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Management.Addons;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.Logic.Objects;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.InstanceProfile
{
    public class InstanceFieldInfo<T> : LeftPanelFieldInfo
    {
        private readonly LeftPanelFieldInfo _info;

        public InstanceFieldInfo(string name, T value, Func<T, string>? converter = null)
            : base(name, converter == null ? value.ToString() : converter(value))
        { }

        public InstanceFieldInfo(string name, Func<object> loadValue) : base(name, loadValue)
        { }
    }

    public class InstanceProfileLeftPanelViewModel : LeftPanelViewModel
    {
        private readonly AppCore _appCore;

        private InstanceModelBase _instanceModel;
        private INavigationStore _navigationStore;


        #region Properties


        public ImageBrush InstanceImage
        {
            get => new ImageBrush(ImageTools.ToImage(_instanceModel.Logo));
        }

        public string InstanceName { get => _instanceModel.Name; }
        public string InstanceVersion { get => _instanceModel.BaseData.GameVersion?.Id; }
        public string InstanceModloader { get => _instanceModel.BaseData.Modloader.ToString(); }
        public string PlayerPlayedTime { get => _instanceModel.IsInstalled ? "10ч" : DownloadCount; }
        public string DownloadCount { get => _instanceModel.TotalDonwloads; }
        public bool IsInstalled { get => _instanceModel.IsInstalled; }


        private ObservableCollection<LeftPanelFieldInfo> _additionalInfo = [];
        public IEnumerable<LeftPanelFieldInfo> AdditionalInfo { get => _additionalInfo; }


        public DownloadingData DownloadingData { get => _instanceModel.DownloadingData; }
        public InstanceModelBase InstanceModel { get => _instanceModel; }


        private bool _isJustLaunching;
        public bool IsJustLaunching
        {
            get => _isJustLaunching; set
            {
                _isJustLaunching = value;
                OnPropertyChanged();
            }
        }


        #endregion Properties


        #region Commands


        private RelayCommand _playCommand;
        /// <summary>
        /// Запускает клиент, если клиент требуется докачать, докачивает.
        /// </summary>
        public ICommand PlayCommand
        {
            get => RelayCommand.GetCommand(ref _playCommand, () =>
            {
                _instanceModel.Run();
            });
        }

        private RelayCommand _closeCommand;
        /// <summary>
        /// Запускает клиент.
        /// </summary>
        public ICommand CloseCommand
        {
            get => RelayCommand.GetCommand(ref _closeCommand, () =>
            {
                _instanceModel.Close();
            });
        }

        private RelayCommand _installCommand;
        /// <summary>
        /// Устанавливает клиент, если клиент не добавлен в библиотеку добавляет.
        /// </summary>
        public ICommand InstallCommand
        {
            get => RelayCommand.GetCommand(ref _installCommand, () => _instanceModel.Download());
        }

        public ICommand BackCommand { get; }



        #endregion Commands


        #region Contructors


        public InstanceProfileLeftPanelViewModel(AppCore appCore, INavigationStore navigationStore, ICommand toMainMenuLayoutCommand, InstanceModelBase instanceModelBase)
        {
            _appCore = appCore;
            BackCommand = new RelayCommand((obj) =>
            {
                if (instanceModelBase.IsInstalled || instanceModelBase.IsDownloading)
                {
                    // Останавливаем обновление директорий сборки.
                    AddonsManager.GetManager(instanceModelBase.BaseData).StopWatchingDirectory();
                }
                toMainMenuLayoutCommand.Execute(obj);
            });

            _navigationStore = navigationStore;

            _instanceModel = instanceModelBase;
            _instanceModel.NameChanged += OnNameChanged;
            _instanceModel.LogoChanged += OnLogoChanged;
            _instanceModel.GameVersionChanged += OnVersionChanged;
            _instanceModel.ModloaderChanged += OnModloaderChanged;
            _instanceModel.StateChanged += OnStateChanged;
            _instanceModel.DownloadProgressChanged += OnDownloadProgressChanged;

            _instanceModel.DownloadComplited += OnDownloadComplited;

            //_instanceModel.DataChanged += OnInstanceModelDataChanged;


            GenerateAdditionalInfo();
        }

        private void OnDownloadComplited(InstanceInit instanceInit, IEnumerable<string> arg2, bool isLaunching)
        {
            if (instanceInit == InstanceInit.Successful)
            {
                _appCore.MessageService.Success("Instance_HasBeenInstalledSuccessful", true, _instanceModel.Name);
            }
            else
            {
                _appCore.MessageService.Error("FailedToInstallColon_", true, _instanceModel.Name);
            }
        }

        private void OnDownloadProgressChanged(StageType type, ProgressHandlerArguments progressArgs)
        {
            if (IsInstalled)
                OnPropertyChanged(nameof(IsInstalled));

            if (IsJustLaunching)
                IsJustLaunching = false;

            OnPropertyChanged(nameof(DownloadingData));
        }


        #endregion Constructors


        #region Private Methods


        private void GenerateAdditionalInfo()
        {
            InstanceData additionalInfo = null;

            if (!_instanceModel.IsInstalled)
            {
            }

            _appCore.UIThread.Invoke(() =>
            {
                _additionalInfo.Clear();
                _additionalInfo.Add(new InstanceFieldInfo<MinecraftVersion>("Version:", _instanceModel.GameVersion));
                _additionalInfo.Add(new LeftPanelFieldInfo("GameType:", _instanceModel.BaseData.Modloader.ToString()));

                if (_instanceModel.IsInstalled)
                {
                    _additionalInfo.Add(new InstanceFieldInfo<long>("PlayedTime:", 100000, SecondsToPlayTime));
                }
                else
                {
                    _additionalInfo.Add(new InstanceFieldInfo<long>("DownloadCount:", () =>
                    {
                        additionalInfo = _instanceModel.AdditionalData;
                        return DownloadsCountToString(additionalInfo.TotalDownloads);
                    }));
                }
            });
        }


        string SecondsToPlayTime(long seconds)
        {
            if (seconds >= 3600)
            {
                return $"{seconds / 3600}ч";
            }
            return $"{seconds / 60}мин";
        }

        string DownloadsCountToString(long number)
        {
            if (number == 0)
                return "0";

            if (number < 10000)
                return number.ToString();

            var size = (long)Math.Log10(number);

            switch (size)
            {
                //k
                case 4:
                    {
                        return (number / 1000).ToString("##.###k");
                    }
                case 5:
                    {
                        return (number / 1000).ToString("###.###k");
                    }
                // M
                case 7:
                    {
                        return (number / 1000000).ToString("##.##M");
                    }
                case 8:
                    {
                        return (number / 100000).ToString("###.##M");
                    }
                default:
                    return (number / Math.Pow(10, size)).ToString("#.##M");
            }
        }


        private void OnNameChanged()
        {
            OnPropertyChanged(nameof(InstanceName));
        }

        private void OnLogoChanged()
        {
            OnPropertyChanged(nameof(InstanceImage));
        }

        private void OnVersionChanged()
        {
            OnPropertyChanged(nameof(InstanceVersion));
        }

        private void OnModloaderChanged()
        {
            OnPropertyChanged(nameof(InstanceModloader));
        }

        private void OnStateChanged()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                GenerateAdditionalInfo();
            });
        }

        public void DeleteInstance()
        {
            // возвращаем пользователя обратно на страницу библиотеки, потом проиграываем задержку, после чего вызываем удаление.
            // P.S задержка нужна, чтобы анимация проигрывалась без косяков.
            BackCommand?.Execute(null);
            // запускаем задержку и удаляем сборку.
            Runtime.TaskRun(() =>
            {
                // TODO: поработать на задержкой.
                Thread.Sleep(10);
                App.Current.Dispatcher?.Invoke(() =>
                {
                    _instanceModel.Delete();
                });
            });
        }


        private void OnInstanceModelDataChanged()
        {
            OnPropertyChanged(nameof(InstanceName));
            OnPropertyChanged(nameof(InstanceVersion));
            OnPropertyChanged(nameof(InstanceModloader));
            OnPropertyChanged(nameof(IsInstalled));

            GenerateAdditionalInfo();
        }


        #endregion Private Methods
    }
}
