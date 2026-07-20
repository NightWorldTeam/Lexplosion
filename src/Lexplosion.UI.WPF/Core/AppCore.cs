
using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Controls.Message.Core;
using Lexplosion.UI.WPF.Core.Notifications;
using Lexplosion.UI.WPF.Core.Services;
using Lexplosion.UI.WPF.Stores;
using System;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Core
{
    public sealed class AppCore
    {
        public event Action<GlobalLoadingArgs> GlobalLoadingStarted;


        private readonly Action _restartApp;


        /// <summary>
        /// Метод для выполнения кода в потоке приложения.
        /// Требуется для возможности работать с разными MVVM фремворками
        /// без четкой зависимости на них.
        /// </summary>
        public readonly Action<Action> UIThread;
        /// <summary>
        /// Метод для получения ресурсов приложения по ключу.
        /// </summary>
        public readonly Func<object, object> Resources;


        #region Properties


        /// <summary>
        /// Настройки приложения
        /// </summary>
        public AppSettings Settings { get; set; }
        /// <summary>
        /// Диалог сервис
        /// </summary>
        /// <summary>
        /// Навигация модалок
        /// </summary>
        public ModalNavigationStore ModalNavigationStore { get; } = new();

        public INavigationStore NavigationStore { get; } = new NavigationStore();

        public IMessageService MessageService { get; }

        public INotificationService NotificationService { get; }


        public Gallery GalleryManager { get; } = new();


        #endregion Properties


        public AppCore(Action<Action> uiThread, Func<object, object> getResource, Action restartApp)
        {
            _restartApp = restartApp;
            Resources = getResource;
            UIThread = uiThread;
            MessageService = new MessageService();
            NotificationService = new NotificationService();
        }


        public ICommand BuildNavigationCommand(ViewModelBase viewModel, Action<ViewModelBase> action = null) 
        {
            return BuildNavigationCommand<ViewModelBase>(viewModel, action);
        }

        public ICommand BuildNavigationCommand<T>(T viewModel, Action<T> action = null) where T : ViewModelBase 
        {
            return new NavigateCommand<ViewModelBase>(NavigationStore, () =>
            {
                action?.Invoke(viewModel);
                return viewModel;
            });
        }

        public void SetGlobalLoadingStatus(bool status, string processDescription = "", bool isProcessDescriptionKey = false) 
        {
            var description = processDescription;

            if (isProcessDescriptionKey) 
            {
                description = Resources(processDescription) as string;
            }

            GlobalLoadingStarted?.Invoke(new GlobalLoadingArgs(status, description));
        }

        public void RestartApp() 
        {
            _restartApp();
        }
    }
}
