using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Lexplosion.Gui
{
    public sealed class ButtonConstructor : VMBase
    {
        /// <summary>
        /// Метод который выполниться при клике по кнопке.
        /// </summary>
        /// 
        private readonly Action _action;
        public delegate void ClickAction(ButtonConstructor constructor);
        private ClickAction _clickAction;

        #region props


        private object _content;
        /// <summary>
        /// Текст кнопки. 
        /// </summary>
        public object Content
        {
            get => _content; set
            {
                _content = value;
                OnPropertyChanged();
            }
        }

        private double _width;
        /// <summary>
        /// Ширина кнопки.
        /// </summary>
        public double Width
        {
            get => _width; set
            {
                _width = value;
                OnPropertyChanged();
            }
        }

        private double _height;
        /// <summary>
        /// Высота кнопки.
        /// </summary>
        public double Height
        {
            get => _height; set
            {
                _height = value;
                OnPropertyChanged();
            }
        }

        private Style _style;
        /// <summary>
        /// Стиль который будет использоваться в кнопке.
        /// <para>Style - System.Windows (WPF).</para>
        /// </summary>
        public Style Style
        {
            get => _style;
            set
            {
                _style = value;
                OnPropertyChanged();
            }
        }

        private Thickness _margin;
        /// <summary>
        /// Задаёт margin(отступы) для кнопки.
        /// </summary>
        public Thickness Margin
        {
            get => _margin; set
            {
                _margin = value;
                OnPropertyChanged();
            }
        }

        private bool _isVisible = true;
        /// <summary>
        /// Задаёт видимость кнопки. Видимость зависит от converter.
        /// <para>Стандатные значения</para>
        /// <c>Visible - true</c>
        /// <br><c>Hidden - false</c></br>
        /// </summary>
        public bool IsVisible
        {
            get => _isVisible; set
            {
                _isVisible = value;
                OnPropertyChanged();
            }
        }

        public IValueConverter _converter = new BooleanToVisibilityConverter();
        /// <summary>
        /// Конвертер для visibility
        /// <para>BooleanToVisibilityConverter - стандартное значение.</para>
        /// </summary>
        public IValueConverter Converter
        {
            get => _converter; set
            {
                if (IsValidConverter(value)) 
                { 
                    _converter = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _stage;
        public int Stage 
        {
            get => _stage; set 
            {
                _stage = value;
                OnPropertyChanged();
            }
        }
        #endregion props


        #region commands

        private RelayCommand _actionCommand;
        /// <summary>
        /// Команда которая будет выполняться при клике на кнопку.
        /// <para><c>_action() - метод который будет выполняться при вызове команды.</c></para>
        /// </summary>
        public RelayCommand ActionCommand
        {
            get => _actionCommand ?? new RelayCommand(obj =>
            {
                _action?.Invoke();
                //_clickAction?.Invoke(this);
            });
        }

        #endregion commands


        public ButtonConstructor(object content, Action action, Style style = null)
        {
            Content = content;
            _action = action;
            //_clickAction = clickAction;
            Style = style;
        }

        #region methods

        private bool IsValidConverter(IValueConverter converter)
        {
            var value = converter.Convert(true, typeof(Visibility), null, null);
            return (value is Visibility.Visible) || (value is Visibility.Collapsed);
        }

        #endregion methods

    }
}
