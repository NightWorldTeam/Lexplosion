using Lexplosion.Tools;

namespace Lexplosion.Gui.ViewModels
{
    /// <summary>
    /// Создано для того, чтобы в любой точке кода которая 
    /// разрабатывается можно было вывести, что блок разрабатываться
    /// </summary>
    public class DevСurtainViewModel : VMBase
    {
        private string _defaultMessage;
        private string _defaultMessageWithName = ResourceGetter.GetString("devCurtainsMessage1");

        private string _message;
        public string Message
        {
            get => _message; set
            {
                _message = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Будет использоваться стандартное сообщение.
        /// </summary>
        public DevСurtainViewModel()
        {
            _defaultMessage = ResourceGetter.GetString("devCurtainMessage");
            Message = _defaultMessage;
        }
    }
}
