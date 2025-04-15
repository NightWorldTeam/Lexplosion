using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Modal;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Authorization.Modal;
using System;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Authorization
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
 