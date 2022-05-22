using Lexplosion.Logic.Management.Instances;
using System.Diagnostics;

namespace Lexplosion.Gui.Models.InstanceForm
{
    public class InstanceFormModel : VMBase
    {
        private string _overviewField;


        #region props
        public InstanceClient InstanceClient { get; set; }
        public DownloadModel DownloadModel { get; set; }
        public MultibuttonModel ButtonModel { get; set; }
        public LaunchModel LaunchModel { get; set; }

        public string OverviewField 
        {
            get => _overviewField; set
            {
                _overviewField = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public InstanceFormModel(InstanceClient instanceClient)
        {

            InstanceClient = instanceClient;
            OverviewField = instanceClient.Description;
            ButtonModel = new MultibuttonModel();
            DownloadModel = new DownloadModel(this)
            {
                DownloadProgress = 0,
                Stage = 0,
                StagesCount = 0
            };
            LaunchModel = new LaunchModel(this);

            if (InstanceClient.IsInstalled && InstanceClient.InLibrary)
            {
                ButtonModel.ChangeFuncPlay();
            }
            else ButtonModel.ChangeFuncDownload(InstanceClient.IsInstalled);
        }

        public void OpenInstanceFolder() 
        {
            Process.Start("explorer", InstanceClient.GetDirectoryPath());
        }
    }
}