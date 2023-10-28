using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu
{
    public sealed class MultiplayerLayoutViewModel : ContentLayoutViewModelBase
    {
        private readonly ViewModelBase _generalMultiplayerViewModel = new MultiplayerViewModel();

        public MultiplayerLayoutViewModel() : base()
        {
            _tabs.Add(new TabItemModel { TextKey = "General", Content = _generalMultiplayerViewModel, IsSelected = true });
        }
    }
}
