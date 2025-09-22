using Lexplosion.UI.WPF.Commands;
using System;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Core.Modal
{
    public abstract class ActionModalViewModelBase : ViewModelBase, IActionModelViewModel
    {
        public event Action<object> CloseCommandExecutedEvent;
        public event Action<object> ActionCommandExecutedEvent;

        public event Action Closed;


        #region Commands


        private RelayCommand _closeCommand;
        public ICommand CloseCommand
        {
            get => RelayCommand.GetCommand(ref _closeCommand, (obj) =>
            {
                CloseCommandExecutedEvent?.Invoke(obj);
            });
        }

        private RelayCommand _actionCommand;
        public ICommand ActionCommand
        {
            get => RelayCommand.GetCommand(ref _actionCommand, (obj) =>
            {
                ActionCommandExecutedEvent?.Invoke(obj);
                if (IsCloseAfterCommandExecuted)
                    CloseCommand.Execute(obj);
            });
        }


        #endregion Commands


        public bool IsCloseAfterCommandExecuted { get; set; } = true;

        public void ExecuteClosedEvent()
        {
            Closed?.Invoke();
        }
    }

    public abstract class ModalViewModelBase : ViewModelBase, IModalViewModel
    {
        public event Action<object> CloseCommandExecutedEvent;

        public event Action Closed;

        #region Commands


        private RelayCommand _closeCommand;
        public ICommand CloseCommand
        {
            get => RelayCommand.GetCommand(ref _closeCommand, (obj) => 
            {
                CloseCommandExecutedEvent?.Invoke(obj);
            });
        }


        #endregion Commands

        public void ExecuteClosedEvent()
        {
            Closed?.Invoke();
        }
    }
}
