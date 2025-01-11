using Lexplosion.WPF.NewInterface.Controls.Message.Core.Types;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.WPF.NewInterface.Controls.Message.Core
{
    public class MessageService : IMessageService
    {
        private readonly ObservableCollection<MessageItemModel> _messages = new();
        public IEnumerable<MessageItemModel> Messages => _messages;

        public void Error(string message)
        {
            App.Current.Dispatcher.Invoke(() => 
            {
                _messages.Add(new()
                {
                    Text = message,
                    Type = MessageType.Error,
                    CreationDate = System.DateTime.Now
                });
            });
        }

        public void Info(string message)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                _messages.Add(new()
                {
                    Text = message,
                    Type = MessageType.Info,
                    CreationDate = System.DateTime.Now
                });
            });
        }

        public void Success(string message)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                _messages.Add(new()
                {
                    Text = message,
                    Type = MessageType.Success,
                    CreationDate = System.DateTime.Now
                });
            });
        }

        public void Warning(string message)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                _messages.Add(new()
                {
                    Text = message,
                    Type = MessageType.Warning,
                    CreationDate = System.DateTime.Now
                });
            });
        }
    }
}
