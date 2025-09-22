using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.Modal;
using System;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.Modal
{
    public class AskServerInstanceInstallingModel 
    {
        private readonly Action<bool> _action;


        public bool IsAutoConnectToServer { get; set; }


        public AskServerInstanceInstallingModel(Action<bool> action)
        {
            _action = action;
        }

        public void Apply() 
        {
            _action(IsAutoConnectToServer);
        }
    }

    public class AskServerInstanceInstallingViewModel : ActionModalViewModelBase
    {
        public AskServerInstanceInstallingModel Model { get; }

        public AskServerInstanceInstallingViewModel(AppCore _appCore, Action<bool> action)
        {
            Model = new(action);
            ActionCommandExecutedEvent += (obj) => 
            { 
                Model.Apply();
            };
        }
    }
}
