using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Core.Tools;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using Lexplosion.WPF.NewInterface.Stores;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading;
using Lexplosion.Logic.Management.Instances;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.InstanceProfile
{
    public class InstanceProfileLeftPanelViewModel : LeftPanelViewModel
    {
        private InstanceModelBase _instanceModel;
        private INavigationStore _navigationStore;


        #region Properties


        public ImageBrush InstanceImage
        {
            get => new ImageBrush(ImageTools.ToImage(_instanceModel.Logo));
        }

        public string InstanceName { get => _instanceModel.Name; }
        public string InstanceVersion { get => _instanceModel.InstanceData.GameVersion?.Id; }
        public string InstanceModloader { get => _instanceModel.InstanceData.Modloader.ToString(); }
        public string PlayerPlayedTime { get => _instanceModel.IsInstalled ? "10ч" : DownloadCount; }
        public string DownloadCount { get => _instanceModel.TotalDonwloads; }
        public bool IsInstalled { get => _instanceModel.IsInstalled; }


        private ObservableCollection<FrameworkElementModel> _instanceActions = new ObservableCollection<FrameworkElementModel>();
        public IEnumerable<FrameworkElementModel> InstanceActions { get => _instanceActions; }


        #endregion Properties


        #region Commands


        private RelayCommand _playCommand;
        /// <summary>
        /// Устанавливает клиент, если клиент требуется докачать, докачивает.
        /// </summary>
        public ICommand PlayCommand
        {
            get => RelayCommand.GetCommand(ref _playCommand, _instanceModel.Run);
        }

        private RelayCommand _installCommand;
        /// <summary>
        /// Устанавливает клиент, если клиент не добавлен в библиотеку добавляет.
        /// </summary>
        public ICommand InstallCommand 
        {
            get => RelayCommand.GetCommand(ref _installCommand, _instanceModel.Download);
        }

        public ICommand BackCommand { get; }
        


        #endregion Commands


        #region Contructors


        public InstanceProfileLeftPanelViewModel(INavigationStore navigationStore, ICommand toMainMenuLayoutCommand, InstanceModelBase instanceModelBase)
        {
            BackCommand = new RelayCommand((obj) =>
            {
                // Останавливаем обновление директорий сборки.
                InstanceAddon.StopWatchingDirectory();
                toMainMenuLayoutCommand.Execute(obj);
            });

            _navigationStore = navigationStore;

            _instanceModel = instanceModelBase;
            _instanceModel.NameChanged += OnNameChanged;
            _instanceModel.GameVersionChanged += OnVersionChanged;
            _instanceModel.ModloaderChanged += OnModloaderChanged;
            _instanceModel.StateChanged += OnStateChanged;

            _instanceModel.DataChanged += OnInstanceModelDataChanged;

            UpdateFrameworkElementModels();
        }


        #endregion Constructors


        #region Private Methods


        private void OnNameChanged()
        {
            OnPropertyChanged(nameof(InstanceName));
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
            UpdateFrameworkElementModels();
        }

        private void UpdateFrameworkElementModels()
        {
            _instanceActions.Clear();

            if (!_instanceModel.IsInstalled && !_instanceModel.InLibrary)
            {
                _instanceActions.Add(new FrameworkElementModel("AddToLibrary", _instanceModel.AddToLibrary, "AddToLibrary"));
            }

            if (_instanceModel.Source != InstanceSource.Local)
            {
                _instanceActions.Add(new FrameworkElementModel("Visit" + _instanceModel.Source.ToString(), _instanceModel.GoToWebsite, _instanceModel.Source.ToString(), 20, 20));
            }

            //if (!_instanceModel.IsInstalled && !_instanceModel.InLibrary)
            //    return;

            // 1. Website
            // 2. AddToLibrary
            // 2. OpenFolder
            // 3. Export
            // 4. RemoveFromLibrary / Delete (Перед удаление переводим пользователя в обратно библиотеку.)


            if (_instanceModel.InLibrary)
            {
                _instanceActions.Add(new FrameworkElementModel("OpenFolder", _instanceModel.OpenFolder, "Folder"));
                if (_instanceModel.IsInstalled)
                {
                    _instanceActions.Add(new FrameworkElementModel("Export", _instanceModel.Export, "Export"));
                }
            }

            if (!_instanceModel.IsInstalled && _instanceModel.InLibrary)
            {
                _instanceActions.Add(new FrameworkElementModel("RemoveFromLibrary", DeleteInstance, "Delete"));
            }
            else if (_instanceModel.IsInstalled)
            {
                _instanceActions.Add(new FrameworkElementModel("DeleteInstance", DeleteInstance, "Delete"));
            }
        }


        private void DeleteInstance() 
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
        }


        #endregion Private Methods
    }
}
