using Lexplosion.WPF.NewInterface.ViewModels.MainContent.MainMenu;
using Lexplosion.WPF.NewInterface.Views.Pages.MainContent.MainMenu;

namespace Lexplosion.WPF.NewInterface.ViewModels.MainContent.MainMenu
{
    public sealed class MainMenuLayoutViewModel : VMBase
    {
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

        public VMBase Content { get; private set; } = new CatalogViewModel();


        #endregion Properties


        #region Constructors


        public MainMenuLayoutViewModel() 
        {
            LeftPanel = new LeftPanelViewModel();
            LeftPanel.SelectedItemChanged += OnLeftPanelSelectedItemChanged;

            LeftPanel.AddTabItem("catalog", "Catalog", new CatalogViewModel());
            LeftPanel.AddTabItem("library", "Catalog", new CatalogViewModel());
            LeftPanel.AddTabItem("multiplayer", "Catalog", new CatalogViewModel());
            LeftPanel.AddTabItem("friends", "Catalog", new CatalogViewModel());
            LeftPanel.AddTabItem("settings", "Catalog", new CatalogViewModel());
        }


        #endregion Constructors


        #region Private Methods


        private void OnLeftPanelSelectedItemChanged(VMBase content) 
        {
            Content = content;
            OnPropertyChanged(nameof(Content));
        }


        #endregion Private Methods
    }
}
