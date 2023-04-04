using System.Windows;
using System.Windows.Data;

namespace Lexplosion.Common
{
    public sealed class ButtonParameters : VMBase
    {
        public delegate void ClickAction();
        public ClickAction ActionClick { get; set; }

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

        private bool IsValidConverter(IValueConverter converter)
        {
            var value = converter.Convert(true, typeof(Visibility), null, null);
            return (value is Visibility.Visible) || (value is Visibility.Collapsed);
        }
    }

    public sealed class StageController : VMBase
    {
        private int _stageCount;
        /// <summary>
        /// Количество стадии.
        /// <br>Стандартное значение 1</br>
        /// </summary>
        public int StageCount
        {
            get => _stageCount; set
            {
                _stageCount = value;
                OnPropertyChanged();
            }
        }

        private int _currentStage = 0;
        /// <summary>
        /// Номер активной стации.
        /// </summary>
        public int CurrentStage
        {
            get => _currentStage; set
            {
                _currentStage = value;
                OnPropertyChanged();
            }
        }

        private ButtonParameters[] _buttonParametersList;

        private void StageUp(ButtonConstructor constructor)
        {
            CurrentStage++;
            constructor.ButtonParameters = _buttonParametersList[CurrentStage];

        }

        private void StageDown(ButtonConstructor constructor)
        {
            CurrentStage--;
            constructor.ButtonParameters = _buttonParametersList[CurrentStage];
        }

        public StageController(ButtonParameters[] buttonsParametersList)
        {
            StageCount = buttonsParametersList.Length;
            _buttonParametersList = buttonsParametersList;
        }

        public void StageSwitch(ButtonConstructor constructor)
        {
            if (_currentStage < _stageCount)
                StageUp(constructor);
            else StageDown(constructor);
        }
    }

    public class ButtonConstructor : VMBase
    {
        /// <summary>
        /// Метод который выполниться при клике по кнопке.
        /// </summary>
        /// 

        #region props

        private ButtonParameters _buttonParameters;
        public ButtonParameters ButtonParameters
        {
            get => _buttonParameters; set
            {
                _buttonParameters = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Контроллер стадий.
        /// </summary>
        private StageController _stageController;


        #endregion props


        #region commands

        private RelayCommand _actionCommand;
        /// <summary>
        /// Команда которая будет выполняться при клике на кнопку.
        /// <para><c>_action() - метод который будет выполняться при вызове команды.</c></para>
        /// </summary>
        public RelayCommand ActionCommand
        {
            get => _actionCommand ?? (_actionCommand = new RelayCommand(obj =>
            {
                _buttonParameters.ActionClick?.Invoke();
                _stageController.StageSwitch(this);
            }));
        }

        #endregion commands


        public ButtonConstructor(ButtonParameters[] buttonParametersList, int currentStage = 0)
        {
            _stageController = new StageController(buttonParametersList);
            _buttonParameters = buttonParametersList[currentStage];
            _stageController.CurrentStage = currentStage;
        }
    }
}
