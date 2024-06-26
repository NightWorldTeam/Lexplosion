using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models;
using Lexplosion.WPF.NewInterface.Stores;
using System;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu
{
    public sealed class MainMenuLayoutViewModel : ViewModelBase, ILayoutViewModel
    {
        private readonly ViewModelBase _catalogViewModel;
        private readonly ViewModelBase _libraryViewModel;
        private readonly ViewModelBase _multiplayerLayoutViewModel = new MultiplayerLayoutViewModel();
        private readonly ViewModelBase _friendsLayoutViewModel = new FriendsLayoutViewModel();
        private readonly ViewModelBase _generalSettingsLayoutViewModel;


        #region Properties


        private LeftPanelViewModel _leftPanel;
        public LeftPanelViewModel LeftPanel
        {
            get => _leftPanel; set
            {
                _leftPanel = value;
                OnPropertyChanged();
            }
        }

        public ViewModelBase Content { get; private set; }


        #endregion Properties


        #region Constructors


        public MainMenuLayoutViewModel(INavigationStore navigationStore, ModalNavigationStore modalNavStore, MainModel mainModel)
        {
            Func<ViewModelBase> s = () => this;
            var ToMainMenuLayoutCommand = new NavigateCommand<ViewModelBase>(navigationStore, s);

            _catalogViewModel = new CatalogViewModel(navigationStore, ToMainMenuLayoutCommand, mainModel.CatalogController);
            _libraryViewModel = new LibraryViewModel(navigationStore, ToMainMenuLayoutCommand, modalNavStore, mainModel.LibraryController);

            _generalSettingsLayoutViewModel = new GeneralSettingsLayoutViewModel(modalNavStore);

            Content = _catalogViewModel;

            LeftPanel = new LeftPanelViewModel();
            LeftPanel.SelectedItemChanged += OnLeftPanelSelectedItemChanged;

            InitDefaultLeftPanelTabs();
        }


        #endregion Constructors


        #region Private Methods


        private void InitDefaultLeftPanelTabs()
        {
            LeftPanel.AddTabItem("Catalog", "Catalog", _catalogViewModel);
            LeftPanel.AddTabItem("Library", "Library", _libraryViewModel);
            LeftPanel.AddTabItem("Multiplayer", "Multiplayer", _multiplayerLayoutViewModel);
            LeftPanel.AddTabItem("Friends", "Friends", _friendsLayoutViewModel, 18, 20);
            LeftPanel.AddTabItem("Settings", "Settings", _generalSettingsLayoutViewModel);
            LeftPanel.SelectFirst();
        }

        private void OnLeftPanelSelectedItemChanged(ViewModelBase content)
        {
            Content = content;
            OnPropertyChanged(nameof(Content));
        }


        #endregion Private Methods
    }
}
