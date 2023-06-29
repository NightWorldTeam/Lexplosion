using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.ViewModels.Authorization
{
    public class PasswordResetViewModel : VMBase
    {
        private readonly INavigationStore<VMBase> _navigationStore;


        #region Commands


        private RelayCommand _continueCommand;
        public ICommand ContinueCommand
        {
            get => _continueCommand ?? (_continueCommand = new RelayCommand(obj =>
            {
                _navigationStore.Open(new DigitCodeConfirmationVIewModel());
            }));
        }


        #endregion Commands


        #region Constructors


        public PasswordResetViewModel(INavigationStore<VMBase> navigationStore)
        {
            _navigationStore = navigationStore;
        }


        #endregion Constructors
    }
}
