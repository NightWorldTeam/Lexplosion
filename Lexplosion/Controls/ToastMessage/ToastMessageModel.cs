using Lexplosion.Gui;
using Lexplosion.Gui.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Controls
{
    public class ToastMessageModel : MessageModel
    {
        public ToastMessageModel(string header, string message, ToastMessageState state) : base(header, message, state) { }
    }
}
