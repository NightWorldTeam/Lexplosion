using System;
using System.Security.Cryptography;
using System.Windows.Media;

namespace Lexplosion.Gui.TrayMenu
{
    public class TrayButton : TrayCompontent
    {
        private readonly Action _actionMethod;

        /// <summary>
        /// Текст кнопки.
        /// </summary>
        public string Text { get; }
        /// <summary>
        /// Иконка кнопки перед текстом.
        /// </summary>
        public Geometry Icon { get; }

        private RelayCommand _actionMethodCommand;
        /// <summary>
        /// Команда которая сработает при клике по кнопке.
        /// </summary>
        public RelayCommand ActionMethodCommand 
        {
            get => _actionMethodCommand ?? (_actionMethodCommand = new RelayCommand(obj =>
            {
                _actionMethod?.Invoke();
            })); 
        }

        public TrayButton(int id, string name, string icon, Action actionMethod)
        {
            _id = id;
            Text = name;
            Icon = Geometry.Parse(icon);
            _actionMethod = actionMethod;
        }
    }
}
