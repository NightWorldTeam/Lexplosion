using Lexplosion.Global;
using Lexplosion.Gui.Commands;
using Lexplosion.Gui.Models;
using Lexplosion.Gui.Models.InstanceFactory;
using Lexplosion.Gui.Models.InstanceForm;
using Lexplosion.Gui.ViewModels.MainMenu;
using Lexplosion.Logic.Management;
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

        #endregion


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
                        var id = ManageLogic.CreateInstance(
                            Model.Name, InstanceSource.Local, SelectedVersion, Model.ModloaderType, SelectedModloaderVersion
                        );
                        NavigationMainMenuCommand.Execute(null);

                        MainModel.AddedInstanceForms.Add(
                            new InstanceFormViewModel(
                                new InstanceFormModel(
                                    new InstanceProperties
                                    {
                                        Name = Model.Name ?? "Custom Instance",
                                        Type = InstanceSource.Local,
                                        Id = string.Empty,
                                        LocalId = id,
                                        Categories = new List<Category>()
                                        {
                                            new Category()
                                            {
                                                categoryId = -1,
                                                name = SelectedVersion
                                            }
                                        },
                                        Logo = Utilities.GetImage("pack://application:,,,/assets/images/icons/non_image.png"),
                                        InstanceAssets = new InstanceAssets()
                                        {
                                            description = "This modpack is not have description...",
                                            author = "by NightWorld",
                                        },
                                        IsInstalled = false,
                                        IsDownloadingInstance = false,
                                        UpdateAvailable = false,
                                        IsInstanceAddedToLibrary = true
                                    }
                                    )
                                )
                            );
                }));
            }
        }
        #endregion

        public FactoryGeneralViewModel()
        {
            NavigationMainMenuCommand = new NavigateCommand<MainMenuViewModel>(
                 MainViewModel.NavigationStore, () => MainViewModel.MainMenuVM);

            List<string> versions = new List<string>();
            Lexplosion.Run.TaskRun(() =>
            {
                foreach (var v in ToServer.GetVersionsList())
                {
                    if (v.type == "release") versions.Add(v.id);
                }
                GameVersions = new ObservableCollection<string>(versions);
                SelectedVersion = GameVersions[0];
            });
            Model = new InstanceFactoryModel();
        }
    }
}
