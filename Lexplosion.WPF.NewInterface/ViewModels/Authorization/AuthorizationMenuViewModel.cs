using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Stores;

namespace Lexplosion.WPF.NewInterface.ViewModels.Authorization
{
    public class AccountTypeMenuItem
    {
        public string Name { get; set; }
        public byte[] Logo { get; set; }
    }
     

    public sealed class AuthorizationMenuViewModel : VMBase
    {
        private readonly INavigationStore<VMBase> _navigationStore;


        #region Commands


        private RelayCommand _openAccountAuthFormCommand;
        public RelayCommand OpenAccountAuthFormCommand
        {
            get => _openAccountAuthFormCommand ?? (_openAccountAuthFormCommand = new RelayCommand(obj => 
            {
                _navigationStore.Open(new NightWorldAuthFormViewModel(_navigationStore));
            }));
        }

        #endregion Commands


        #region Constructors


        public AuthorizationMenuViewModel(INavigationStore<VMBase> navigationStore) 
        {
            _navigationStore = navigationStore;
        }


        #endregion Constructors
    }
}
