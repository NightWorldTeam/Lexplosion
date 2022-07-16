using Lexplosion.Gui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Lexplosion.Controls
{
    public class DialogMessageModel : ToastMessageModel
    {
        private Action _leftButtonContent;
        public Action rightButtonContent;

        public DialogMessageModel(
            string header, 
            string message,
            Action leftButtonCommand,
            Action rightButtonCommand,
            string leftButtonContent,
            string rightButtonContent,
            ToastMessageState state = ToastMessageState.Notification
            ) : base(header, message,  state)
        {

        }

        public string LeftButtonContent { get; set; }
        public string RightButtonContent { get; set; }

        public RelayCommand LeftButtonCommand 
        {
            get => new RelayCommand(obj => 
            {
                //_leftButtonContent.Method.Invoke();
            });
        }
    }
}
