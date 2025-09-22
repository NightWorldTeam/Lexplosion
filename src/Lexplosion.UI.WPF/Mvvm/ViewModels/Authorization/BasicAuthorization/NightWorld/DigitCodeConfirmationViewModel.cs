using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Mvvm.Models.Authorization.BasicAuthorization.NightWorld;
using Lexplosion.UI.WPF.Stores;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.Authorization
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
