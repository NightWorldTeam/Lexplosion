using Lexplosion.Gui;
using Lexplosion.Gui.ViewModels;
using System;

namespace Lexplosion.Controls
{
    public abstract class MessageModel
    {
        public string Header { get; set; }
        public string Message { get; set; }
        public Enum State { get; set; }

        public RelayCommand CloseToastMessage
        {
            get => new RelayCommand(obj =>
            {
                MainViewModel.Messages.Remove(this);
            });
        }

        public MessageModel(string header, string message, Enum state)
        {
            Header = header;
            Message = message;
            State = state;
        }
    }
}
