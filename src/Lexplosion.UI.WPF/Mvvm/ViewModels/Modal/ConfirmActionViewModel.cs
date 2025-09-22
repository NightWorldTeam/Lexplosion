using Lexplosion.UI.WPF.Core.Modal;
using System;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.Modal
{
    public class ConfirmActionViewModel : ActionModalViewModelBase
    {
        public string Title { get; }
        public string Text { get; }
        public string ActionButtonText { get; }


        /// <summary>
        /// Идея передавать ключ, который будет тянутся из ресурсов приложения.
        /// Например значение ключа может быть datatemplate.
        /// </summary>
        /// TODO: Сделать возможно флага danger, при его наличии кнопку нужно красить в danger цвет.
        public ConfirmActionViewModel(string title, string text, string actionButtonText, Action<object> action)
        {
            Title = title;
            Text = text;
            ActionButtonText = actionButtonText;
            ActionCommandExecutedEvent += action;
        }
    }
}
