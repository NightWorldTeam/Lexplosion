using Lexplosion.Gui.ModalWindow;
using Lexplosion.Gui.Models.InstanceFactory;
using Lexplosion.Gui.ViewModels.ModalVMs;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Network;
using System.Collections.ObjectModel;

namespace Lexplosion.Gui.ViewModels.FactoryMenu
{
    public sealed class FactoryGeneralViewModel : ModalVMBase
    {
        private readonly MainViewModel _mainViewModel;


        #region Properties


        public InstanceFactoryModel Model { get; }


        private ImportViewModel _importViewModel;
        public ImportViewModel ImportVM 
        {
            get => _importViewModel; set 
            {
                _importViewModel = value;
                OnPropertyChanged();
            }
        }

        private bool _isModloaderSelected = false;
        public bool IsModloaderSelected
        {
            get => _isModloaderSelected; set
            {
                _isModloaderSelected = value;
                OnPropertyChanged();
            }
        }

        private string[] _gameVersions;
        public string[] GameVersions
        {
            get => _gameVersions; set
            {
                _gameVersions = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<string> _modloaderVersion;
        public ObservableCollection<string> ModloaderVersions
        {
            get => _modloaderVersion; set
            {
                _modloaderVersion = value;
                OnPropertyChanged();
            }
        }

        private string _selectedModloaderVersion;
        public string SelectedModloaderVersion
        {
            get => _selectedModloaderVersion; set
            {
                _selectedModloaderVersion = value;
                OnPropertyChanged();
            }
        }

        private string _selectedVersion;
        public string SelectedVersion
        {
            get => _selectedVersion; set
            {
                _selectedVersion = value ?? GameVersions[0];
                OnPropertyChanged();
                ReselectVersionLoadModloaderVersions(_selectedVersion);
            }
        }

        private string _selectedOnlyVersion;

        private bool _isShowSnapshots;
        public bool IsShowSnapshots 
        {
            get => _isShowSnapshots; set 
            {
                _isShowSnapshots = value; 
                OnPropertyChanged();
                UpdateVersions();
            }
        }

        private bool _hasSodiumInstall;
        public bool HasSodiumInstall 
        {
            get => _hasSodiumInstall; set 
            {
                _hasSodiumInstall = value;
                OnPropertyChanged();
            }
        }


        #endregion Properties


        #region Commands


        private RelayCommand _switchModloaderType;
        public RelayCommand SwitchModloaderType
        {
            get => _switchModloaderType ?? (_switchModloaderType = new RelayCommand(obj =>
            {
                ChangeModloaderType((ModloaderType)obj);
            }));
        }

        private RelayCommand _createInstance;
        /// <summary>
        /// Создание самого модпака начинается здесь.
        /// </summary>
        public override RelayCommand ActionCommand
        {
            get => _createInstance ?? (new RelayCommand(obj =>
            {
                CreateInstance();
            }));
        }

        private RelayCommand _closeModalWindowCommand;
        public override RelayCommand CloseModalWindowCommand
        {
            get => _closeModalWindowCommand ?? (_closeModalWindowCommand = new RelayCommand(obj =>
            {
                _mainViewModel.ModalWindowVM.IsOpen = false;
                _mainViewModel.ModalWindowVM.ChangeCurrentModalContent(null);
            }));
        }

        public RelayCommand LogoImportCommand
        {
            get => new RelayCommand(obj =>
            {
                OpenDialogWindowForImage();
            });
        }

        #endregion Commands



        public FactoryGeneralViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            Model = new InstanceFactoryModel();
            Model.ModloaderType = ModloaderType.Vanilla;
            UpdateVersions();  

            _importViewModel = new ImportViewModel(_mainViewModel, this);
        }

        #region Private Methods


        private void UpdateVersions() 
        {
            if (IsShowSnapshots)
            {
                GameVersions = MainViewModel.AllGameVersions.ToArray();
            }
            else GameVersions = MainViewModel.ReleaseGameVersions.ToArray();
            SelectedVersion = GameVersions[0];
        }

        private void CreateInstance() 
        {
            var instanceClient = InstanceClient.CreateClient(
                Model.Name ?? "New Client",
                InstanceSource.Local,
                _selectedOnlyVersion,
                Model.ModloaderType,
                Model.LogoPath,
                SelectedModloaderVersion
           );

            _mainViewModel.Model.LibraryInstances.Add(new InstanceFormViewModel(_mainViewModel, instanceClient));
            _mainViewModel.ModalWindowVM.IsOpen = false;
        }

        private void ChangeModloaderType(ModloaderType modloaderType) 
        {
            Model.ModloaderType = modloaderType;

            IsModloaderSelected = Model.ModloaderType != ModloaderType.Vanilla;

            Lexplosion.Run.TaskRun(() =>
            {
                var versions = ToServer.GetModloadersList(_selectedOnlyVersion, Model.ModloaderType);

                ModloaderVersions = new ObservableCollection<string>(versions);

                if (ModloaderVersions.Count > 0)
                    SelectedModloaderVersion = ModloaderVersions[0];
            });
        }

        private void ReselectVersionLoadModloaderVersions(string selectedVersion) 
        {
            if (IsShowSnapshots)
            {
                if (selectedVersion.Contains("snapshot"))
                    _selectedOnlyVersion = selectedVersion.Replace("snapshot ", "");
                else
                    _selectedOnlyVersion = selectedVersion.Replace("release ", "");
            }
            else _selectedOnlyVersion = selectedVersion;

            if (Model.ModloaderType != ModloaderType.Vanilla)
            {
                Lexplosion.Run.TaskRun(() =>
                {
                    ModloaderVersions = new ObservableCollection<string>(ToServer.GetModloadersList(_selectedOnlyVersion, Model.ModloaderType));

                    if (ModloaderVersions.Count > 0) SelectedModloaderVersion = ModloaderVersions[0];
                });
            }
        }

        private void OpenDialogWindowForImage() 
        {
            using (var dialog = new System.Windows.Forms.OpenFileDialog())
            {
                dialog.Filter = "Image files|*.bmp;*.jpg;*.gif;*.png;*.tif|All files|*.*";

                // Process open file dialog box results
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Model.LogoPath = dialog.FileName;
                }
            }
        }
        #endregion Private Methods
    }
}
