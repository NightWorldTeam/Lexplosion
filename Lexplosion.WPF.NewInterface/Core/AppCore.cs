
using Lexplosion.WPF.NewInterface.Controls.Message.Core;
using Lexplosion.WPF.NewInterface.Core.Services;
using Lexplosion.WPF.NewInterface.Stores;
using System;
using System.Threading;
using System.Windows.Documents;

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

        public INavigationStore NavigationStore { get; } = new NavigationStore();


        public IMessageService MessageService { get; }


        #endregion Properties


        public AppCore(Action<Action> uiThread)
        {
            UIThread = uiThread;
            MessageService = new MessageService();

            Runtime.TaskRun(() =>
            {
                for (var i = 0; i < 4; i++) 
                {
                    Thread.Sleep(1000);
                    if (i == 0) 
                        MessageService.Info("This is info message");
                    else if (i == 1)
                        MessageService.Success("This is success message");
                    else if (i == 2)
                        MessageService.Warning("This is warning message");                    
                    else
                        MessageService.Error("This is error message");
                }
            });
        }
    }
}
