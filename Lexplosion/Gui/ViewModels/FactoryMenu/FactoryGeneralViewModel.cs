using Lexplosion.Global;
using Lexplosion.Gui.Commands;
using Lexplosion.Gui.ModalWindow;
using Lexplosion.Gui.Models;
using Lexplosion.Gui.Models.InstanceFactory;
using Lexplosion.Gui.Models.InstanceForm;
using Lexplosion.Gui.ViewModels.MainMenu;
using Lexplosion.Gui.ViewModels.ModalVMs;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Lexplosion.Gui.ViewModels.FactoryMenu
{
    //public class ImportedFiles 
    //{
    //    private InstanceClient _instanceClient;
    //}

    public sealed class FactoryGeneralViewModel : ModalVMBase
    {
        private RelayCommand _switchModloaderType;
        private RelayCommand _createInstance;

        private ObservableCollection<string> _gameVersions;
        private string _selectedVersion;

        private ObservableCollection<string> _modloaderVersion;
        private string _selectedModloaderVersion;

        private bool _isModloaderSelected = false;

        private MainViewModel _mainViewModel;

        #region prop

        private ImportViewModel _importViewModel;
        public ImportViewModel ImportVM 
        {
            get => _importViewModel; set 
            {
                _importViewModel = value;
                OnPropertyChanged();
            }
        }

        public bool IsModloaderSelected
        {
            get => _isModloaderSelected; set
            {
                _isModloaderSelected = value;
                OnPropertyChanged(nameof(IsModloaderSelected));
            }
        }
        public ObservableCollection<string> GameVersions
        {
            get => _gameVersions; set
            {
                _gameVersions = value;
                OnPropertyChanged(nameof(GameVersions));
            }
        }

        public ObservableCollection<string> ModloaderVersions
        {
            get => _modloaderVersion; set
            {
                _modloaderVersion = value;
                OnPropertyChanged(nameof(ModloaderVersions));
            }
        }

        public string SelectedModloaderVersion
        {
            get => _selectedModloaderVersion; set
            {
                _selectedModloaderVersion = value;
                Console.WriteLine(_selectedModloaderVersion);
                OnPropertyChanged(nameof(SelectedModloaderVersion));
            }
        }

        public string SelectedVersion
        {
            get => _selectedVersion; set
            {
                _selectedVersion = value;
                OnPropertyChanged(nameof(SelectedVersion));

                if (Model.ModloaderType != ModloaderType.None) 
                { 
                    Lexplosion.Run.TaskRun(() =>
                    {
                        ModloaderVersions = new ObservableCollection<string>(ToServer.GetModloadersList(SelectedVersion, Model.ModloaderType));
                        if (ModloaderVersions.Count > 0)
                            SelectedModloaderVersion = ModloaderVersions[0];
                    });
                }
            }
        }

        public InstanceFactoryModel Model { get; }

        #endregion props

        #region commands

        public RelayCommand SwitchModloaderType
        {
            get
            {
                return _switchModloaderType ?? (new RelayCommand(obj =>
                {
                    Model.ModloaderType = (ModloaderType)obj;

                    IsModloaderSelected = Model.ModloaderType != ModloaderType.None;

                    Lexplosion.Run.TaskRun(() =>
                    {
                        var versions = ToServer.GetModloadersList(SelectedVersion, Model.ModloaderType);

                        ModloaderVersions = new ObservableCollection<string>(versions);

                        if (ModloaderVersions.Count > 0)
                            SelectedModloaderVersion = ModloaderVersions[0];
                    });
                }));
            }
        }
        /// <summary>
        /// Создание самого модпака начинается здесь.
        /// </summary>
        public override RelayCommand Action
        {
            get
            {
                return _createInstance ?? (new RelayCommand(obj =>
                {
                    var instanceClient = InstanceClient.CreateClient(
                        Model.Name ?? "New Client", InstanceSource.Local, SelectedVersion, Model.ModloaderType, Model.LogoPath, SelectedModloaderVersion);

                    _mainViewModel.Model.LibraryInstances.Add(new InstanceFormViewModel(_mainViewModel, instanceClient));
                    _mainViewModel.ModalWindowVM.IsOpen = false;
                }));
            }
        }

        public override RelayCommand CloseModalWindow
        {
            get => new RelayCommand(obj =>
            {
                _mainViewModel.ModalWindowVM.IsOpen = false;
                _mainViewModel.ModalWindowVM.ChangeCurrentModalContent(null);
            });
        }

        public RelayCommand LogoImportCommand
        {
            get => new RelayCommand(obj =>
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
            });
        }

        #endregion commands

        public FactoryGeneralViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;

            GameVersions = new ObservableCollection<string>(MainViewModel.GameVersions.ToList());
            Model = new InstanceFactoryModel();
            Model.ModloaderType = ModloaderType.None;
            SelectedVersion = GameVersions[0];
            

            _importViewModel = new ImportViewModel(_mainViewModel, this);
        }
    }
}
