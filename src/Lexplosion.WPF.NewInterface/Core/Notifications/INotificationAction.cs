using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Core.Notifications
{
    public interface INotificationAction
    {
        /// <summary>
        /// Ключ/Название
        /// </summary>
        public string Key { get; }
        /// <summary>
        /// Выполняемое действие
        /// </summary>
        public ICommand ActionCommand { get; }
    }
}
