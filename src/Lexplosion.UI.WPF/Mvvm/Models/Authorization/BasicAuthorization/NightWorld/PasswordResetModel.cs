using Lexplosion.UI.WPF.Core;

namespace Lexplosion.UI.WPF.Mvvm.Models.Authorization.BasicAuthorization.NightWorld
{
    public class PasswordResetModel : VMBase
    {
        private readonly AppCore _appCore;


        // TODO: Change to email
        public string Email { get; set; }


        public PasswordResetModel(AppCore appCore)
        {
            _appCore = appCore;
        }


        public void GetCode()
        {
            if (string.IsNullOrEmpty(Email))
            {

            }
            else
            {
                _appCore.MessageService.Info("Code field is empty");
            }
        }
    }
}
