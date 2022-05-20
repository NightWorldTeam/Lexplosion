using Lexplosion.Global;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using System.Diagnostics;

namespace Lexplosion.Gui.Models.InstanceForm
{
    public class InstanceFormModel : VMBase
    {
        #region props
        public InstanceClient InstanceClient { get; set; }
        public DownloadModel DownloadModel { get; set; }
        public MultibuttonModel ButtonModel { get; set; }
        public LaunchModel LaunchModel { get; set; }
        #endregion

        public InstanceFormModel(InstanceClient instanceClient)
        {

            InstanceClient = instanceClient;
            ButtonModel = new MultibuttonModel();
            DownloadModel = new DownloadModel(InstanceClient, ButtonModel)
            {
                DownloadProgress = 0,
                Stage = 0,
                StagesCount = 0
            };
            LaunchModel = new LaunchModel(InstanceClient, DownloadModel, ButtonModel);

            if (InstanceClient.InLibrary)
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