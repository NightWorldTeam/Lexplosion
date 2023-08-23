using Lexplosion.WPF.NewInterface.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.WPF.NewInterface.ViewModels.MainContent.MainMenu
{
    public sealed class AccountsSettingsModel : ViewModelBase 
    {
    
    }
 
    public class AccountsSettingsViewModel : ViewModelBase
    {
        public AccountsSettingsModel  Model { get; }


        public AccountsSettingsViewModel()
        {
            Model = new AccountsSettingsModel();
        }
    }
}
