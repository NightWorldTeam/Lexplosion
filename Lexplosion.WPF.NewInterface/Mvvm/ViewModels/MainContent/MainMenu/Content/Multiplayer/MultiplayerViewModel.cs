using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.MainMenu;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu
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

        public MultiplayerViewModel()
        {
            Model = new MultiplayerModel();
        }


        #endregion Constructors
    }
}
