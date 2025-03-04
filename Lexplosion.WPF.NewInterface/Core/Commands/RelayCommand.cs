﻿using System;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public static ICommand GetCommand(ref RelayCommand variable, Action action, Func<object, bool> canExecute = null)
        {
            return GetCommand(ref variable, (obj) => action(), canExecute);
        }

        public static ICommand GetCommand(ref RelayCommand variable, Action<object> action, Func<object, bool> canExecute = null)
        {
            return variable ?? (variable = new RelayCommand(action, canExecute));
        }

        public static ICommand GetCommand<T>(ref RelayCommand variable, Action<T> action, Func<object, bool> canExecute = null)
        {
            return variable ?? (variable = new RelayCommand((obj) =>
            {
                action((T)obj);
            }, canExecute));
        }
    }
}
