
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Controls.Message.Core;
using Lexplosion.WPF.NewInterface.Core.Services;
using Lexplosion.WPF.NewInterface.Stores;
using System;
using System.Threading;
using System.Windows.Documents;
using System.Windows.Input;

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
    }
}
