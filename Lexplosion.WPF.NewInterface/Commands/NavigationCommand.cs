using Lexplosion.WPF.NewInterface.Stores;
using System;

namespace Lexplosion.WPF.NewInterface.Commands
{
    public sealed class NavigateCommand<T> : CommandBase where T : VMBase
    {
        private readonly NavigationStore _navigationStore;
        private readonly Func<T> _createViewModel;

        public NavigateCommand(NavigationStore navigationStore, Func<T> createViewModel)
        {
            _navigationStore = navigationStore;
            _createViewModel = createViewModel;
        }

        public override void Execute(object parameter)
        {
            _navigationStore.Content = _createViewModel();
        }
    }
}
