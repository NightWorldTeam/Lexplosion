using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core.Modal;
using System;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal
{
    public sealed class DialogBoxModel : ModalViewModelBase
    {
        public string TitleKey { get; }
        public string DescriptionKey { get; }

        public Action<object> YesAction { get; }
        public Action<object> NoAction { get; }

        public DialogBoxModel(string titleKey, string descriptionKey, Action<object> yesAction, Action<object> noAction)
        {
            TitleKey = titleKey;
            DescriptionKey = descriptionKey;
            YesAction = yesAction;
            NoAction = noAction;
        }
    }

    public sealed class DialogBoxViewModel : ModalViewModelBase
    {
        public DialogBoxModel Model { get; }


        #region Commands


        private RelayCommand _yesAnswerCommand;
        public ICommand YesAnswerCommand
        {
            get => RelayCommand.GetCommand(ref _yesAnswerCommand, Model.YesAction);
        }

        private RelayCommand _noAnswerCommand;
        public ICommand NoAnswerCommand
        {
            get => RelayCommand.GetCommand(ref _noAnswerCommand, Model.NoAction);
        }


        #endregion Commands


        public DialogBoxViewModel(string titleKey, string descriptionKey, Action<object> yesAction, Action<object> noAction)
        {
            Model = new DialogBoxModel(titleKey, descriptionKey, yesAction, noAction);
        }
    }
}
