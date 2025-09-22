using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Mvvm.Models.Authorization.BasicAuthorization.NightWorld;
using Lexplosion.UI.WPF.Stores;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.Authorization
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
