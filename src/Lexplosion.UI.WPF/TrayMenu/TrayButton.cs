using Lexplosion.UI.WPF.Commands;
using System;
using System.Drawing;
using System.Windows.Media;

namespace Lexplosion.UI.WPF.TrayMenu
{
    public class TrayButton : TrayComponentBase
    {
        private readonly Action _actionMethod;

        /// <summary>
        /// Текст кнопки.
        /// </summary>
        public string Text { get; }


        private RelayCommand _actionMethodCommand;
        /// <summary>
        /// Команда которая сработает при клике по кнопке.
        /// </summary>
        public RelayCommand ActionMethodCommand
        {
            get => _actionMethodCommand ?? (_actionMethodCommand = new RelayCommand(obj =>
            {
                _actionMethod?.Invoke();
                RuntimeApp.TrayMenuElementClickExecute();
            }));
        }

        public TrayButton(int id, string name, Action actionMethod) : base(id)
        {
            Text = name;
            _actionMethod = actionMethod;
        }
    }
}
