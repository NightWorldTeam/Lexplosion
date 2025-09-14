using Lexplosion.WPF.NewInterface.Controls.Message.Core.Types;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.WPF.NewInterface.Controls.Message.Core
{
    public class MessageService : IMessageService
    {
        private readonly ObservableCollection<MessageItemModel> _messages = new();
        private readonly ObservableCollection<MessageItemModel> _unreadMessages = new();


        public IEnumerable<MessageItemModel> Messages => _messages;
        public IEnumerable<MessageItemModel> UnreadMessages => _unreadMessages;


        public void Error(string message, bool isResourceKey = false, params object[] formatParams)
        {
            if (formatParams.Length == 0)
            {
                Publish(message, MessageType.Info, isResourceKey);
            }
            else
            {
                PublishFormatterString(message, MessageType.Info, isResourceKey, formatParams);
            }
        }

        public void Info(string message, bool isResourceKey = false, params object[] formatParams)
        {
            if (formatParams.Length == 0)
            {
                Publish(message, MessageType.Info, isResourceKey);
            }
            else
            {
                PublishFormatterString(message, MessageType.Info, isResourceKey, formatParams);
            }
        }

        public void Success(string message, bool isResourceKey = false, params object[] formatParams)
        {
            if (formatParams.Length == 0)
            {
                Publish(message, MessageType.Success, isResourceKey);
            }
            else
            {
                PublishFormatterString(message, MessageType.Success, isResourceKey, formatParams);
            }
        }

        public void Warning(string message, bool isResourceKey = false, params object[] formatParams)
        {
            if (formatParams.Length == 0)
            {
                Publish(message, MessageType.Warning, isResourceKey);
            }
            else
            {
                PublishFormatterString(message, MessageType.Warning, isResourceKey, formatParams);
            }
        }

        private void Publish(string messageText, MessageType type, bool isResourceKey)
        {
            App.Current.Dispatcher.Invoke((System.Delegate)(() =>
            {
                MessageItemModel message = new()
                {
                    Text = !isResourceKey ? messageText : (string)App.Current.Resources[messageText],
                    Type = type,
                    CreationDate = System.DateTime.Now
                };

                message.IsViewedChanged += OnMessageIsViewedChanged;

                _messages.Add(message);
                _unreadMessages.Add(message);
            }));
        }

        public void PublishFormatterString(string messageText, MessageType type, bool isResourceKey = false, params object[] formatParams)
        {
            var messageResult = !isResourceKey ? messageText : (string)App.Current.Resources[messageText];

            messageResult = string.Format(messageResult, formatParams);

            App.Current.Dispatcher.Invoke((System.Delegate)(() =>
            {
                MessageItemModel message = new()
                {
                    Text = messageResult,
                    Type = type,
                    CreationDate = System.DateTime.Now
                };

                message.IsViewedChanged += OnMessageIsViewedChanged;

                _messages.Add(message);
                _unreadMessages.Add(message);
            }));
        }

        private void OnMessageIsViewedChanged(MessageItemModel sender, bool value)
        {
            sender.IsViewedChanged -= OnMessageIsViewedChanged;
            _unreadMessages.Remove(sender);
        }
    }
}
