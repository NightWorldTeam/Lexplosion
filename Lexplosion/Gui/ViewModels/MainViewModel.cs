using Lexplosion.Gui.Models;
using Lexplosion.Gui.Stores;
using Lexplosion.Gui.ViewModels.MainMenu;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Network;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace Lexplosion.Gui.ViewModels
{
    public class ImmutableList<T> : IEnumerable<T>
    {
        private readonly List<T> _list;

        public ImmutableList(List<T> list)
        {
            _list = list;
        }

        public List<T> ToList() => _list; 
        
        public T this[int index] 
        {
            get => _list[index];
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class MainViewModel : VMBase
    {
        public static readonly NavigationStore NavigationStore = new NavigationStore();
        public static MainMenuViewModel MainMenuVM { get; private set; }

        private ObservableCollection<string> _gameVersions;

        public static bool IsInstanceRunning = false;

        private RelayCommand _closeCommand;
        private RelayCommand _hideCommand;


        /// <summary>
        /// Данное свойство содержит в себе версии игры.
        /// Является static, т.к эксемпляр MainViewModel создаётся в единственном эксемляре, в начале запуска лаунчер, до появляния начального окна.
        /// </summary>
        public static ImmutableList<string> GameVersions { get; private set; }

        #region props

        public MainModel Model { get; }

        public VMBase CurrentViewModel => NavigationStore.CurrentViewModel;

        private static bool _isShowInfoBar;
        public bool IsShowInfoBar 
        { 
            get => _isShowInfoBar; set 
            {
                _isShowInfoBar = value;
                OnPropertyChanged();
            }
        }

        private bool _isAuthorized;
        public bool IsAuthorized
        {
            get => _isAuthorized; set
            {
                _isAuthorized = value;
                IsShowInfoBar = value;
                OnPropertyChanged(nameof(IsAuthorized));
            }
        }

        private string _nickname;
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
            get => _closeCommand ?? (_closeCommand = new RelayCommand(obj => Run.Exit()));
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
            List<string> versions = new List<string>();
            Lexplosion.Run.TaskRun(() =>
            {
                foreach (var v in ToServer.GetVersionsList())
                {
                    if (v.type == "release") versions.Add(v.id);
                }
                GameVersions = new ImmutableList<string>(versions);
            });
            Model = new MainModel();
            MainMenuVM = new MainMenuViewModel(this);
            NavigationStore.CurrentViewModel = new AuthViewModel(this, LibraryInstanceLoading);
            NavigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
        }

        #region methods
        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }

        private void LibraryInstanceLoading()
        {
            foreach (var instanceClient in InstanceClient.GetInstalledInstances()) 
            { 
                MainModel.LibraryInstances.Add(
                    instanceClient, new InstanceFormViewModel(instanceClient)
                );
            }
        }
        #endregion
    }
}