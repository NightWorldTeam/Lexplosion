using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Models.Authorization.BasicAuthorization.NightWorld;
using Lexplosion.WPF.NewInterface.Stores;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.ViewModels.Authorization
{
    public class PasswordResetViewModel : VMBase
    {
        private readonly INavigationStore<VMBase> _navigationStore;

        public PasswordResetModel Model { get; }


        #region Commands


        private RelayCommand _continueCommand;
        public ICommand ContinueCommand
        {
            get => _continueCommand ?? (_continueCommand = new RelayCommand(obj =>
            {
                Model.GetCode();
                _navigationStore.Open(new DigitCodeConfirmationViewModel(_navigationStore));
            }));
        }


        #endregion Commands


        #region Constructors


        public PasswordResetViewModel(INavigationStore<VMBase> navigationStore)
        {
            Model = new PasswordResetModel();
            _navigationStore = navigationStore;
        }


        #endregion Constructors
    }
}
