using Lexplosion.Logic.Management.Instances;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel
{
    internal sealed class LaunchModel
    {
        private readonly InstanceClient _instanceClient;


        #region Constructors


        public LaunchModel(InstanceClient instanceClient)
        {
            _instanceClient = instanceClient;
            _instanceClient.LaunchComplited += OnLaunchFinished;
        }


        #endregion Constructors


        #region Public Methods


        /// <summary>
        /// Запускает сборку.
        /// </summary>
        public void Run()
        {
            _instanceClient.Run();
        }

        /// <summary>
        /// Закрывает сборку. 
        /// После того как метод отработает вызывается эвент InstanceModel GameExited.
        /// </summary>
        public void Close()
        {
            _instanceClient.StopGame();
        }


        #endregion Public Methods


        #region Private Methods


        /// <summary>
        /// Обратывает результат запуска.
        /// </summary>
        /// <param name="isSuccessful">Успешен ли запуск?</param>
        private void OnLaunchFinished(string id, bool isSuccessful)
        {
            if (isSuccessful)
            {

            }
            else
            {

            }
        }

        /// <summary>
        /// Отрабатывает после закрытия сборки.
        /// </summary>
        public void OnGameExited()
        {

        }


        #endregion Private Methods
    }
}
