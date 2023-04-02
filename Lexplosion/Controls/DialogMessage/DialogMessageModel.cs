using Lexplosion.Gui;
using System;

namespace Lexplosion.Controls
{
    public class DialogMessageModel : MessageModel
    {
        private Action _leftButtonCommand;
        public Action _rightButtonCommand;

        public DialogMessageModel(
            string header,
            string message,
            Action leftButtonCommand,
            Action rightButtonCommand,
            string leftButtonContent,
            string rightButtonContent,
            ToastMessageState state = ToastMessageState.Notification
            ) : base(header, message, state)
        {
            _leftButtonCommand = leftButtonCommand;
            _rightButtonCommand = rightButtonCommand;
            LeftButtonContent = leftButtonContent;
            RightButtonContent = rightButtonContent;
        }

        public string LeftButtonContent { get; set; }
        public string RightButtonContent { get; set; }

        public RelayCommand LeftButtonCommand
        {
            get => new RelayCommand(obj =>
            {
                _leftButtonCommand();
            });
        }

        public RelayCommand RightButtonCommand
        {
            get => new RelayCommand(obj =>
            {
                _rightButtonCommand();
            });
        }
    }
}
