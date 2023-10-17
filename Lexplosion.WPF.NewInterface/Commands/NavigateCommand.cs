using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Stores;
using System;

namespace Lexplosion.WPF.NewInterface.Commands
{
    public sealed class NavigateCommand<T> : CommandBase where T : ViewModelBase
    {
        private readonly INavigationStore _navigationStore;
        private readonly Func<T> _createViewModel;

        public NavigateCommand(INavigationStore navigationStore, Func<T> createViewModel)
        {
            _navigationStore = navigationStore;
            _createViewModel = createViewModel;
        }

        public override void Execute(object parameter)
        {
            _navigationStore.CurrentViewModel = _createViewModel();
        }
    }
}
