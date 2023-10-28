using Lexplosion.WPF.NewInterface.Commands;
using System;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Core.Objects
{
    public sealed class FrameworkElementModel
    {
        public double Width { get; }
        public double Height { get; }

        public string IconKey { get; }
        public string TextKey { get; }

        private Action _action;
        private RelayCommand _executeAction;
        public ICommand ExecuteAction
        {
            get => RelayCommand.GetCommand(ref _executeAction, (obj) => { _action(); });
        }

        public bool IsActive { get; set; }

        public FrameworkElementModel(string textKey, Action action, string iconKey = "", double width = 20, double height = 20)
        {
            TextKey = textKey;
            _action = action;
            IconKey = iconKey;
            Width = width;
            Height = height;
        }
    }
}
