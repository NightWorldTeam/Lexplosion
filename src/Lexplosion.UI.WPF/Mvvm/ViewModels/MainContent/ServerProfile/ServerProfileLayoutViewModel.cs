using Lexplosion.Logic.Objects;
using Lexplosion.UI.WPF.Core;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.ServerProfile
{
    public sealed class ServerProfileLayoutViewModel : ViewModelBase, ILayoutViewModel
    {
        private readonly AppCore _appCore;


        private ServerProfileOverviewLayoutViewModel _overviewViewModel;


        #region Properties


        private ServerProfileLeftPanelViewModel _leftPanel;
        public ServerProfileLeftPanelViewModel LeftPanel
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


        public ServerProfileLayoutViewModel(AppCore appCore, ICommand backCommand, MinecraftServerInstance minecraftServerInstance)
        {
            _appCore = appCore;

            LeftPanel = new ServerProfileLeftPanelViewModel(appCore, minecraftServerInstance, backCommand);
            LeftPanel.SelectedItemChanged += OnLeftPanelSelectedItemChanged;

            InitDefaultLeftPanelTabs(minecraftServerInstance);
        }


        #endregion Constructors


        #region Private Methods


        private void InitDefaultLeftPanelTabs(MinecraftServerInstance minecraftServerInstance)
        {
            _overviewViewModel = new ServerProfileOverviewLayoutViewModel(_appCore, minecraftServerInstance);

            LeftPanel.AddTabItem("Overview", "Services", _overviewViewModel);
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
