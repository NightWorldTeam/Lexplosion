using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Mvvm.Models.Profile;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.Profile
{
    public sealed class ProfileViewModel : ViewModelBase
    {
        public ProfileModel Model { get; }

        public ProfileViewModel(ProfileInfo profileModel)
        {
            Model = new(profileModel);
        }
    }
}
