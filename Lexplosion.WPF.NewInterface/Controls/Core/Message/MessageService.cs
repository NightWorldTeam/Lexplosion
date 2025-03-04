using Lexplosion.WPF.NewInterface.Controls.Message.Core.Types;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.WPF.NewInterface.Controls.Message.Core
{
    public class MessageService : IMessageService
    {
        private readonly ObservableCollection<MessageItemModel> _messages = new();


        public IEnumerable<MessageItemModel> Messages => _messages;


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

        private void Publish(string message, MessageType type, bool isResourceKey)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                _messages.Add(new()
                {
                    Text = !isResourceKey ? message : (string)App.Current.Resources[message],
                    Type = MessageType.Warning,
                    CreationDate = System.DateTime.Now
                });
            });
        }

        public void PublishFormatterString(string message, MessageType type, bool isResourceKey = false, params object[] formatParams)
        {
            var messageResult = !isResourceKey ? message : (string)App.Current.Resources[message];

            messageResult = string.Format(messageResult, formatParams);

            App.Current.Dispatcher.Invoke(() =>
            {
                _messages.Add(new()
                {
                    Text = messageResult,
                    Type = type,
                    CreationDate = System.DateTime.Now
                });
            });
        }
    }
}
