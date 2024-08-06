using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Notifications;
using System;
using System.Threading.Tasks;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel
{
    public sealed class LaunchModel : ViewModelBase
    {
        private readonly InstanceClient _instanceClient;
        private NotifyCallback _notify;

        /// <summary>
        /// Игра запускается
        /// </summary>
        public event Action LaunchStarted;
        /// <summary>
        /// Результат запуска игры.<br/>
        /// bool - Успешно или нет.
        /// </summary>
        public event Action<bool> LaunchCompleted;
        /// <summary>
        /// Игра закрывается
        /// </summary>
        public event Action Closed;


        private bool _isLaunching;
        /// <summary>
        /// Запускается ли игра.
        /// </summary>
        public bool IsLaunching 
        { 
            get => _isLaunching; private set 
            {
                _isLaunching = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Запущена ли игра.
        /// </summary>
        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning; private set
            {
                _isRunning = value;
                OnPropertyChanged();
            }
        }


        #region Constructors


        public LaunchModel(InstanceClient instanceClient, NotifyCallback notify)
        {
            _notify = notify;
            _instanceClient = instanceClient;
            // эвент завершения запуска.
            _instanceClient.LaunchComplited += OnLaunchFinished;
            // эвент закрытия игры.
            _instanceClient.GameExited += OnGameExited;
        }


        #endregion Constructors


        #region Public Methods


        /// <summary>
        /// Запускает сборку.
        /// </summary>
        public async void Run()
        {
            if (!IsLaunching) 
            { 
                IsLaunching = true;
                LaunchStarted?.Invoke();
                await Task.Run(() => { _instanceClient.Run(); });
            }
        }

        /// <summary>
        /// Закрывает сборку. 
        /// После того как метод отработает вызывается эвент InstanceModel GameExited.
        /// </summary>
        public void Close()
        {
            _instanceClient.StopGame();
            IsLaunching = false;
            Runtime.DebugWrite("Game Close Func");
        }


        #endregion Public Methods


        #region Private Methods


        /// <summary>
        /// Обратывает результат запуска.
        /// </summary>
        /// <param name="id">ID сборки</param>
        /// <param name="isSuccessful">Успешен ли запуск?</param>
        private void OnLaunchFinished(string id, bool isSuccessful)
        {
            LaunchCompleted?.Invoke(isSuccessful);
            if (isSuccessful)
            {
                IsLaunching = false;
                IsRunning = true;
            }
            else
            {
                IsLaunching = false;
                _notify.Invoke(new SimpleNotification($"Не удалось запустить {_instanceClient.Name}", "Причины хз"));
            }
            Runtime.DebugWrite("Game Launch Finished");
        }


        /// <summary>
        /// Отрабатывает после закрытия сборки.
        /// </summary>
        /// <param name="id">id закрытой сборки</param>
        private void OnGameExited(string id)
        {
            IsRunning = false;
            Closed?.Invoke();
            Runtime.DebugWrite("Game Exited");
        }


        #endregion Private Methods
    }
}
