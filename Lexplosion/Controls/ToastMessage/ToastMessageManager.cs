using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.Controls
{
    public class ToastMessageManager
    {
        private readonly ObservableCollection<MessageModel> _toastMessages;
        public IEnumerable<MessageModel> ToastMessages => _toastMessages;

        public ToastMessageManager()
        {
            _toastMessages = new ObservableCollection<MessageModel>();
        }

        public void AddMessage(MessageModel message) 
        {
            _toastMessages.Add(message);
        }
    }
}
