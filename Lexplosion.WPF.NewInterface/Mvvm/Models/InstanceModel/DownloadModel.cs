using Lexplosion.Logic.Management.Instances;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel
{
    internal sealed class DownloadModel
    {
        private readonly InstanceClient _instanceClient;


        #region Constructors


        public DownloadModel(InstanceClient instanceClient)
        {
            _instanceClient = instanceClient;
        }


        #endregion Constructors


        #region Public Methods


        /// <summary>
        /// Запускает скачивание сборки.
        /// </summary>
        public void Download(string version)
        {
            _instanceClient.Update();
        }

        /// <summary>
        /// Отменяет текущие скачивание
        /// </summary>
        public void DownloadCancel()
        {
            _instanceClient.CancelDownload();
        }


        #endregion Public Methods


        #region Private Methods


        private void OnDownloadComplited()
        {

        }


        #endregion Private Methods
    }
}
