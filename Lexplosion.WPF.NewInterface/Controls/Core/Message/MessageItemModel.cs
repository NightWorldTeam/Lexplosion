using Lexplosion.WPF.NewInterface.Controls.Message.Core.Types;
using System;

namespace Lexplosion.WPF.NewInterface.Controls.Message.Core
{
    public struct MessageItemModel
    {
        public string Text { get; set; }
        public MessageType Type { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
