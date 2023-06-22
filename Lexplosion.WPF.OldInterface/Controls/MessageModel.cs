using Lexplosion.Common;
using Lexplosion.Common.ViewModels;
using System;

namespace Lexplosion.Controls
{
    public abstract class MessageModel
    {
        public uint CollectionIndex { get; private set; }

        public string Header { get; }
        public string Message { get; }
        public ToastMessageState State { get; }
        public TimeSpan? Time { get; }

        public RelayCommand CloseToastMessage
        {
            get => new RelayCommand(obj =>
            {
                MainViewModel.Messages.Remove(this);
            });
        }

        public MessageModel(string header, string message, ToastMessageState state, TimeSpan? time = null)
        {
            Header = header;
            Message = message;
            State = state;
            Time = (time == null) ? TimeSpan.MaxValue : time;
        }


    }
}
