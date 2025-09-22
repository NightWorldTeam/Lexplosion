using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.Modal;
using Lexplosion.UI.WPF.Mvvm.Models.Authorization.Modal;
using System;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.Authorization
{
    public class MicrosoftManualInputViewModel : ActionModalViewModelBase
    {
        public event Action<string> TokenEntered;
        public event Action Closed;

        public MicrosoftManualInputModel Model { get; }

        public MicrosoftManualInputViewModel(AppCore appCore)
        {
            Model = new MicrosoftManualInputModel(appCore);

            ActionCommandExecutedEvent += (obj) =>
            {
                TokenEntered?.Invoke(Model.MicrosoftToken);
            };

            CloseCommandExecutedEvent += (obj) => 
            {
                Closed?.Invoke();
            };
        }
    }
}
 