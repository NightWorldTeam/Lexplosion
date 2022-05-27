using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lexplosion.Gui.Models.InstanceForm
{
    public class InstanceFormModel : VMBase
    {
        private string _overviewField;
        private List<Category> _categories = new List<Category>();

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

        public List<Category> Categories
        {
            get => _categories; set
            {
                _categories = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public InstanceFormModel(InstanceClient instanceClient)
        {

            InstanceClient = instanceClient;

            // set categories to list
            // add game version like category
            Categories.Add(new Category { name = instanceClient.GameVersion });
            foreach (var category in InstanceClient.Categories)
            {
                Categories.Add(category);
            }

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