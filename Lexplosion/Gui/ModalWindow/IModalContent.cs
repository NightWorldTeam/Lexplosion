using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Gui
{
    public interface IModalContent
    {
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
