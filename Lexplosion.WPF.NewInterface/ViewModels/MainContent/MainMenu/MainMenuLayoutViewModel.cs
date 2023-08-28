using Lexplosion.WPF.NewInterface.Core;

namespace Lexplosion.WPF.NewInterface.ViewModels.MainContent.MainMenu
{
    public sealed class MainMenuLayoutViewModel : ViewModelBase, ILayoutViewModel
    {
        private readonly ViewModelBase _catalogViewModel = new CatalogViewModel();
        private readonly ViewModelBase _libraryViewModel = new LibraryViewModel();
        private readonly ViewModelBase _multiplayerLayoutViewModel = new MultiplayerLayoutViewModel();
        private readonly ViewModelBase _friendsLayoutViewModel = new FriendsLayoutViewModel();
        private readonly ViewModelBase _generalSettingsLayoutViewModel = new GeneralSettingsLayoutViewModel();


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

        public ViewModelBase Content { get; private set; } = new CatalogViewModel();


        #endregion Properties


        #region Constructors


        public MainMenuLayoutViewModel()
        {
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
