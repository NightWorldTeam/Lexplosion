using Lexplosion.WPF.NewInterface.Stores;
using Lexplosion.WPF.NewInterface.Tools;
using Lexplosion.WPF.NewInterface.ViewModels.Authorization;
using System;
using System.Windows;
using System.Windows.Media;

namespace Lexplosion.WPF.NewInterface.ViewModels
{
    public sealed class MainViewModel : VMBase
    {
        internal INavigationStore<VMBase> NavigationStore { get; } = new NavigationStore();
        public VMBase CurrentViewModel => NavigationStore.Content;


        public MainViewModel()
        {
            NavigationStore.CurrentViewModelChanged += NavigationStore_CurrentViewModelChanged;
            NavigationStore.Content = new AuthorizationMenuViewModel(NavigationStore);
        }

        private void NavigationStore_CurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }

        public static void ChangeColor(Color color)
        {
        }
    }
}
