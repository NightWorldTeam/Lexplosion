using Lexplosion.Global;
using Lexplosion.Gui.Models;
using Lexplosion.Gui.Models.InstanceForm;
using Lexplosion.Gui.Stores;
using Lexplosion.Gui.ViewModels.CurseforgeMarket;
using Lexplosion.Gui.ViewModels.MainMenu;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using System.Collections.Generic;
using System.Windows;

namespace Lexplosion.Gui.ViewModels
{
    public class MainViewModel : VMBase
    {
        public static readonly NavigationStore NavigationStore = new NavigationStore();
        public static readonly MainMenuViewModel MainMenuVM = new MainMenuViewModel();

        public static bool IsInstanceRunning = false;

        private string _nickname;
        private bool _isAuthorized;

        private RelayCommand _closeCommand;
        private RelayCommand _hideCommand;
        

        #region props
        public VMBase CurrentViewModel => NavigationStore.CurrentViewModel;
        
        public bool IsAuthorized
        {
            get => _isAuthorized; set
            {
                _isAuthorized = value;
                OnPropertyChanged(nameof(IsAuthorized));
            }
        }

        public MainModel Model { get; }

        public string Nickname 
        {
            get => _nickname; set 
            {
                _nickname = value;
                OnPropertyChanged(nameof(Nickname));
            }
        }

        public object InstanceForms { get; private set; }
        #endregion

        #region commands
        public RelayCommand CloseCommand 
        {
            get => _closeCommand ?? (_closeCommand = new RelayCommand(obj => 
            {
                Run.Exit();
            }));
        }

        public RelayCommand HideCommand 
        {
            get => _hideCommand ?? (_hideCommand = new RelayCommand(obj => 
            {
                Application.Current.MainWindow.WindowState = WindowState.Minimized;
            }));
        }
        #endregion

        public MainViewModel()
        {
            Model = new MainModel();
            NavigationStore.CurrentViewModel = new AuthViewModel(this, InstancesLoading);
            NavigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
        }

        #region methods
        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }

        private void InstancesLoading()
        {
            foreach (var instanceClient in InstanceClient.GetInstalledInstances()) 
            { 
                MainModel.LibraryInstances.Add(
                    new InstanceFormViewModel(
                        instanceClient
                    )
                );
            }
        }
        #endregion
    }
}