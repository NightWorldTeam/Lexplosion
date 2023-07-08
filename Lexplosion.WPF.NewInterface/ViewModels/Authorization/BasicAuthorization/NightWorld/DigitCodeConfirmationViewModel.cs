using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Models.Authorization.BasicAuthorization.NightWorld;
using Lexplosion.WPF.NewInterface.Stores;
using System;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.ViewModels.Authorization
{
    public class DigitCodeConfirmationViewModel : VMBase
    {
        private readonly INavigationStore<VMBase> _navigationStore;

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


        public DigitCodeConfirmationViewModel(INavigationStore<VMBase> navigationStore)
        {
            _navigationStore = navigationStore;
        }


        #endregion Constuctors
    }
}
