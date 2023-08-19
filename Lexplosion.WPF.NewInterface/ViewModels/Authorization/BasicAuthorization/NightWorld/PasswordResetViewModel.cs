using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Models.Authorization.BasicAuthorization.NightWorld;
using Lexplosion.WPF.NewInterface.Stores;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.ViewModels.Authorization
{
    public class PasswordResetViewModel : ViewModelBase
    {
        private readonly INavigationStore _navigationStore;

        public PasswordResetModel Model { get; }


        #region Commands


        private RelayCommand _continueCommand;
        public ICommand ContinueCommand
        {
            get => _continueCommand ?? (_continueCommand = new RelayCommand(obj =>
            {
                Model.GetCode();
                _navigationStore.CurrentViewModel = new DigitCodeConfirmationViewModel(_navigationStore);
            }));
        }


        #endregion Commands


        #region Constructors


        public PasswordResetViewModel(INavigationStore navigationStore)
        {
            Model = new PasswordResetModel();
            _navigationStore = navigationStore;
        }


        #endregion Constructors
    }
}
