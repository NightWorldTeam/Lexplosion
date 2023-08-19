using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Stores;
using System;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Core.Modal
{
    public abstract class ModalViewModelBase : ViewModelBase, IModalViewModel
    {
        public bool IsCloseWhenActionCommandExecuted { get; set; } = true;

        /// <summary>
        /// Делегат который выполниться после вызова CloseCommand
        /// </summary>
        public Action CloseCommandAction;

        private RelayCommand _closeCommand;
        public ICommand CloseCommand
        {
            get => _closeCommand ?? (_closeCommand = new RelayCommand(obj =>
            {
                ModalNavigationStore.Instance.Close();
                CloseCommandAction?.Invoke();
            }));
        }

        /// <summary>
        /// Делегат который выполниться после вызова ActionCommand
        /// </summary>
        public Action<object> ActionCommandAction;

        private RelayCommand _actionCommand;
        public ICommand ActionCommand
        {
            get => _actionCommand ?? (_actionCommand = new RelayCommand(obj =>
            {
                ActionCommandAction?.Invoke(obj);
                if (IsCloseWhenActionCommandExecuted)
                {
                    ModalNavigationStore.Instance.Close();
                }
            }));
        }

        public virtual ICommand HideCommand { get; }
    }
}
