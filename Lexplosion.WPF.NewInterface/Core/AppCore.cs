using Lexplosion.WPF.NewInterface.Core.Notifications;
using Lexplosion.WPF.NewInterface.Core.Services;
using Lexplosion.WPF.NewInterface.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.WPF.NewInterface.Core
{
    public sealed class AppCore
    {
        /// <summary>
        /// Метод для выполнения кода в потоке приложения.
        /// Требуется для возможности работать с разными MVVM фремворками
        /// без четкой зависимости на них.
        /// </summary>
        public readonly Action<Action> UIThread;


        #region Properties


        /// <summary>
        /// Настройки приложения
        /// </summary>
        public AppSettings Settings { get; }
        /// <summary>
        /// Диалог сервис
        /// </summary>
        /// <summary>
        /// Навигация модалок
        /// </summary>
        public ModalNavigationStore ModalNavigationStore { get; } = new();


        #endregion Properties


        public AppCore(Action<Action> uiThread)
        {
            UIThread = uiThread;
        }
    }
}
