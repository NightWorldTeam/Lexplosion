using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Authorization.BasicAuthorization.NightWorld;
using Lexplosion.WPF.NewInterface.Stores;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Authorization
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


        public PasswordResetViewModel(AppCore appCore, INavigationStore navigationStore)
        {
            Model = new PasswordResetModel(appCore);
            _navigationStore = navigationStore;
        }


        #endregion Constructors
    }
}
