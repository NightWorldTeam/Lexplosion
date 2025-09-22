using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Authorization.BasicAuthorization.NightWorld;
using Lexplosion.WPF.NewInterface.Stores;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Authorization
{
    public class DigitCodeConfirmationViewModel : ViewModelBase
    {
        private readonly INavigationStore _navigationStore;

        public DigitCodeConfimationModel Model { get; }


        #region Commands


        private RelayCommand _checkCodeCommand;
        public ICommand CheckCodeCommand
        {
            get => _checkCodeCommand ?? (_checkCodeCommand = new RelayCommand(obj =>
            {

            }));
        }



        #endregion Commands


        #region Constuctors


        public DigitCodeConfirmationViewModel(INavigationStore navigationStore)
        {
            _navigationStore = navigationStore;
        }


        #endregion Constuctors
    }
}
