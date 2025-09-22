using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Mvvm.Models.MainContent.MainMenu;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.MainMenu
{
    public sealed class MultiplayerViewModel : ViewModelBase
    {
        public MultiplayerModel Model { get; }


        #region Commands


        private RelayCommand _rebootCommand;
        public ICommand RebootCommand
        {
            get => RelayCommand.GetCommand(ref _rebootCommand, Model.Reboot);
        }


        #endregion Commands


        #region Constructors

        public MultiplayerViewModel(AppCore appCore)
        {
            Model = new MultiplayerModel(appCore);
        }


        #endregion Constructors
    }
}
