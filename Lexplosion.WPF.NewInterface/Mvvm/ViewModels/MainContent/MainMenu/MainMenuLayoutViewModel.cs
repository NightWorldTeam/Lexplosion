using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Args;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Limited;
using Lexplosion.WPF.NewInterface.Stores;
using System;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu
{
    public sealed class MainMenuLayoutViewModel : ViewModelBase, ILayoutViewModel
    {
        private readonly ViewModelBase _catalogViewModel;
        private readonly ViewModelBase _libraryViewModel;
        private readonly ViewModelBase _multiplayerLayoutViewModel;
        private readonly ViewModelBase _friendsLayoutViewModel;
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


        public Action<InstanceModelBase> ToInstanceProfile { get; }


        #endregion Properties


        #region Constructors


        public MainMenuLayoutViewModel(AppCore appCore, INavigationStore navigationStore, ModalNavigationStore modalNavStore, MainModel mainModel)
        {
            Func<ViewModelBase> s = () => this;
            var ToMainMenuLayoutCommand = new NavigateCommand<ViewModelBase>(navigationStore, s);

            // Catalog Section
            _catalogViewModel = new CatalogViewModel(appCore, navigationStore, ToMainMenuLayoutCommand, mainModel.CatalogController);

            // Library Section
            _libraryViewModel = new LibraryViewModel(appCore, navigationStore, ToMainMenuLayoutCommand, modalNavStore, mainModel.LibraryController, OpenCatalog);
            var toLibraryCommand = new NavigateCommand<ViewModelBase>(navigationStore, () => _libraryViewModel);

            // Multiplayer Section
            var selectInstanceForServerArgs = new SelectInstanceForServerArgs(() => mainModel.LibraryController.Instances, (ic) => 
            {
                var instanceModel = mainModel.LibraryController.Add(ic);
                var item = LeftPanel.SelectItem(1);
                (item.Content as LibraryViewModel).IsScrollToEnd = true;
                return instanceModel;
            });
            var multiplayerLayoutArgs = new MultiplayerLayoutArgs(OpenAccountFactory, selectInstanceForServerArgs);
            _multiplayerLayoutViewModel = new NightWorldLimitedContentLayoutViewModel(new MultiplayerLayoutViewModel(appCore, ToMainMenuLayoutCommand, multiplayerLayoutArgs));

            // Friends Section
            _friendsLayoutViewModel = new NightWorldLimitedContentLayoutViewModel(new FriendsLayoutViewModel(appCore, OpenAccountFactory));

            // Settings Section
            _generalSettingsLayoutViewModel = new GeneralSettingsLayoutViewModel(appCore);

            Content = _catalogViewModel;

            LeftPanel = new LeftPanelViewModel();
            LeftPanel.SelectedItemChanged += OnLeftPanelSelectedItemChanged;

            ToInstanceProfile = (instanceModel) => (_libraryViewModel as LibraryViewModel).OpenInstanceProfileMenuCommand.Execute(instanceModel);

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

        private void OpenAccountFactory() 
        {
            LeftPanel.GetByContentType(typeof(GeneralSettingsLayoutViewModel)).IsSelected = true;
            var generalSettingsLayout = _generalSettingsLayoutViewModel as GeneralSettingsLayoutViewModel;
            var accountsSettingsTIM = generalSettingsLayout.GetByTypeOfContent(typeof(AccountsSettingsViewModel));
            accountsSettingsTIM.IsSelected = true;
            var accountsSettings = accountsSettingsTIM.Content as AccountsSettingsViewModel;
            accountsSettings?.OpenAccountFactoryCommand.Execute(null);
        }

        private void OpenCatalog() 
        {
            LeftPanel.GetByContentType(typeof(CatalogViewModel)).IsSelected = true;
        }

        public void Refresh()
        {
            //_multiplayerLayoutViewModel = new 
        }


        #endregion Private Methods
    }
}
