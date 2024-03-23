using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.MainMenu;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu
{
    public sealed class MultiplayerViewModel : ViewModelBase
    {
        public MultiplayerModel Model { get; }

        public MultiplayerViewModel()
        {
            Model = new MultiplayerModel();
        }
    }
}
