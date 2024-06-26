using Lexplosion.WPF.NewInterface.Core.Modal;
using System;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal
{
    public class ConfirmActionViewModel : ActionModalViewModelBase
    {
        public string TitleKey { get; }
        public string MessageKey { get; }

        public ConfirmActionViewModel(string title, string messageKey, Action<object> action)
        {
            ActionCommandExecutedEvent += action;
        }
    }
}
