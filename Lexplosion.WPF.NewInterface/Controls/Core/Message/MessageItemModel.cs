using Lexplosion.WPF.NewInterface.Controls.Message.Core.Types;
using System;

namespace Lexplosion.WPF.NewInterface.Controls.Message.Core
{
    public sealed class MessageItemModel
    {
        public event Action<MessageItemModel, bool> IsViewedChanged;

        public string Text { get; set; }
        public MessageType Type { get; set; }
        public DateTime CreationDate { get; set; }

        private bool _isViewed;
        public bool IsViewed
        {
            get => _isViewed; set
            {
                if (_isViewed != value)
                {
                    _isViewed = value;
                    IsViewedChanged?.Invoke(this, _isViewed);
                }
            }
        }

        public MessageItemModel()
        {

        }
    }
}
