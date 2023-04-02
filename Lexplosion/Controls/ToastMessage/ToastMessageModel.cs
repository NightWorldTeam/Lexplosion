using System;

namespace Lexplosion.Controls
{
    internal class ToastMessageModel : MessageModel
    {
        public ToastMessageModel(string header, string message, ToastMessageState state, TimeSpan? time = null) : base(header, message, state, time) { }
    }
}
