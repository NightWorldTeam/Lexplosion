using Lexplosion.Gui;
using Lexplosion.Gui.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Controls
{
    public class ToastMessageModel
    {
        public string Header { get; set; }
        public string Message { get; set; }
        public ToastMessageState State { get; set; }

        public RelayCommand CloseToastMessage 
        {
            get => new RelayCommand(obj => 
            {
                MainViewModel.Messages.Remove(this);
            });
        }

        public ToastMessageModel(string header, string message, ToastMessageState state = ToastMessageState.Notification) 
        {
            Header = header;
            Message = message;
            State = state;
        }
    }
}
