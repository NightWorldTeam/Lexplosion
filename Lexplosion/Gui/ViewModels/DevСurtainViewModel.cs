using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Gui.ViewModels
{
    /// <summary>
    /// Создано для того, чтобы в любой точке кода которая 
    /// разрабатывается можно было вывести, что блок разрабатываться
    /// </summary>
    public class DevСurtainViewModel : VMBase
    {
        private const string DefaultMessage = "/* Данный раздел находиться а разработке. */";
        public const string DefaultMessageWithName = "/* Раздел {0} находиться а разработке. */";

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
            Message = DefaultMessage;
        }
    }
}
