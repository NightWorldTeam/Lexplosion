using Lexplosion.Global;
using Lexplosion.Logic.Objects;
using System.Diagnostics;

namespace Lexplosion.Gui.Models.InstanceForm
{
    public class InstanceFormModel : VMBase
    {
        #region props
        public InstanceModel Instance { get; set; }
        public DownloadModel DownloadModel { get; set; }
        public MultibuttonModel ButtonModel { get; set; }
        public LaunchModel LaunchModel { get; set; }
        #endregion

        public InstanceFormModel(InstanceProperties properties)
        {

            Instance = new InstanceModel(properties);
            ButtonModel = new MultibuttonModel();
            if (Instance.IsInstalled)
                ButtonModel.ChangeFuncPlay();
            else 
                ButtonModel.ChangeFuncDownload(Instance.IsInstanceAddedToLibrary);

            DownloadModel = new DownloadModel(Instance, ButtonModel)
            {
                DownloadProgress = 0,
                Stage = 0,
                StagesCount = 0
            };
            LaunchModel = new LaunchModel(Instance, DownloadModel, ButtonModel)
            {

            };
        }

        public void OpenInstanceFolder() 
        {
            Process.Start("explorer", @"" + UserData.GeneralSettings.GamePath.Replace("/", @"\") + @"\instances\" + Instance.Properties.Id);
        }
    }
}