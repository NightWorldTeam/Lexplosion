using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Gui
{
    public interface IModal
    {
        /// <summary>
        /// Устанавливает ширину модального окна.
        /// </summary>
        public double Width { get; }

        /// <summary>
        /// Устанавливает высоту модального окна.
        /// </summary>
        public double Height { get; }

        /// <summary>
        /// Команда на закрытие модального окна.
        /// </summary>
        public RelayCommand CloseModalWindow { get; }

        /// <summary>
        /// Действие которые должно выполниться, например экспорт, создание сборки и n
        /// </summary>
        public RelayCommand Action { get; }
    }
}
