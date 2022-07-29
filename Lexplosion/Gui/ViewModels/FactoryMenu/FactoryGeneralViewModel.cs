using Lexplosion.Global;
using Lexplosion.Gui.Commands;
using Lexplosion.Gui.Models;
using Lexplosion.Gui.Models.InstanceFactory;
using Lexplosion.Gui.Models.InstanceForm;
using Lexplosion.Gui.ViewModels.MainMenu;
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
    public class FactoryGeneralViewModel : VMBase
    {
        private RelayCommand _switchModloaderType;
        private RelayCommand _createInstance;

        private ObservableCollection<string> _gameVersions;
        private string _selectedVersion;

        private ObservableCollection<string> _modloaderVersion;
        private string _selectedModloaderVersion;

        private bool _isModloaderSelected = false;

        public ICommand NavigationMainMenuCommand { get; set; }

        #region prop
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
                OnPropertyChanged(nameof(SelectedModloaderVersion));
            }
        }

        public string SelectedVersion
        {
            get => _selectedVersion; set
            {
                _selectedVersion = value;
                OnPropertyChanged(nameof(SelectedVersion));
                Lexplosion.Run.TaskRun(() =>
                {
                    ModloaderVersions = new ObservableCollection<string>(ToServer.GetModloadersList(SelectedVersion, Model.ModloaderType));
                    if (ModloaderVersions.Count > 0)
                        SelectedModloaderVersion = ModloaderVersions[0];
                });
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
                    if (Model.ModloaderType != ModloaderType.None)
                    {
                        IsModloaderSelected = true;
                    }
                    else IsModloaderSelected = false;
                    Lexplosion.Run.TaskRun(() =>
                    {
                        ModloaderVersions = new ObservableCollection<string>(ToServer.GetModloadersList(SelectedVersion, Model.ModloaderType));
                        if (ModloaderVersions.Count > 0)
                            SelectedModloaderVersion = ModloaderVersions[0];
                    });
                }));
            }
        }

        public RelayCommand CreateInstance
        {
            get
            {
                return _createInstance ?? (new RelayCommand(obj =>
                {
                    NavigationMainMenuCommand.Execute(null);

                    var instanceClient = InstanceClient.CreateClient(
                        Model.Name ?? "CustomInstance", InstanceSource.Local, SelectedVersion, Model.ModloaderType, SelectedModloaderVersion);

                    MainModel.LibraryInstances.Add(instanceClient, new InstanceFormViewModel(null, instanceClient));
                }));
            }
        }

        public RelayCommand LogoImportCommand 
        {
            get => new RelayCommand(obj =>
            {
                var dialog = new System.Windows.Forms.OpenFileDialog();

                // Process open file dialog box results
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {

                }
            });
        }
        #endregion commands

        public FactoryGeneralViewModel()
        {
            NavigationMainMenuCommand = new NavigateCommand<MainMenuViewModel>(
                 MainViewModel.NavigationStore, () => MainViewModel.MainMenuVM);

            GameVersions = new ObservableCollection<string>(MainViewModel.GameVersions.ToList());
            SelectedVersion = GameVersions[0];
            Model = new InstanceFactoryModel();
        }
    }
}
