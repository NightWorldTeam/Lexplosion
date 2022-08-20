using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Lexplosion.Gui
{
    public class ButtonConstructor : VMBase
    {
        /// <summary>
        /// Метод который выполниться при клике по кнопке.
        /// </summary>
        private readonly Action _action;

        /// <summary>
        /// Текст кнопки. 
        /// </summary>
        private object _content;
        public object Content 
        {
            get => _content; set 
            {
                _content = value;
                OnPropertyChanged();
            } 
        }

        /// <summary>
        /// Ширина кнопки.
        /// </summary>
        private double _width;
        public double Width 
        {
            get => _width; set 
            {
                _width = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Высота кнопки.
        /// </summary>
        private double _height;
        public double Height
        {
            get => _height; set
            {
                _height = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Стиль который будет использоваться в кнопке.
        /// Style - System.Windows (WPF).
        /// </summary>
        private Style _style;
        public Style Style 
        { 
            get => _style;
            set 
            {
                _style = value;
                OnPropertyChanged();
            } 
        }

        /// <summary>
        /// Задаёт margin для кнопки.
        /// </summary>
        private Thickness _margin;
        public Thickness Margin 
        {
            get => _margin; set 
            {
                _margin = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Команда которая будет выполняться при клике на кнопку.
        /// _action() - метод который будет выполняться при вызове команды.
        /// </summary>
        private RelayCommand _actionCommand;
        public RelayCommand ActionCommand
        {
            get => _actionCommand ?? new RelayCommand(obj => 
            {
                _action();
            });
        }

        public ButtonConstructor(object content, Action action, Style style = null)
        {
            Content = content;
            _action = action;
            Style = style;
        }
    }
}
