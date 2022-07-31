using Lexplosion.Gui.Models.ShowCaseMenu;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using System;
using System.Collections.Generic;

namespace Lexplosion.Gui.ViewModels.ShowCaseMenu
{
    public class OverviewViewModel : VMBase
    {
        #region props

        private OverviewModel _overviewModel;
        public OverviewModel OverviewModel 
        { 
            get => _overviewModel; set 
            {
                _overviewModel = value;
                OnPropertyChanged(nameof(OverviewModel));
            } 
        }

        private double _categoryPanelBorderHeight;
        public double CategoryPanelBorderHeight
        {
            get => _categoryPanelBorderHeight; set 
            {
                _categoryPanelBorderHeight = value;
                OnPropertyChanged(nameof(CategoryPanelBorderHeight));
            }
        }

        private bool _isLoaded = true;
        public bool IsLoaded
        {
            get => _isLoaded;
            set
            {
                _isLoaded = value;
                OnPropertyChanged(nameof(IsLoaded));
            }
        }

        public bool IsCategoriesExist { get; set; }
        public bool IsDownloadsCountExists { get; set; }

        #endregion props

        public RelayCommand CurseforgePageCommand
        {
            get => new RelayCommand(obj =>
            {
                try
                {
                    System.Diagnostics.Process.Start(OverviewModel.InstanceData.WebsiteUrl);
                }
                catch 
                {
                    // message box here.
                }
            });
        }

        public OverviewViewModel(InstanceClient instanceClient, ISubmenu submenuViewModel)
        {
            Lexplosion.Run.TaskRun(() => { 
                OverviewModel = new OverviewModel(instanceClient, submenuViewModel);

                if (OverviewModel.InstanceData.TotalDownloads == null)
                {
                    IsDownloadsCountExists = false;
                }
                else IsDownloadsCountExists = true;


                CategoryPanelBorderHeight = CalcCategoryBorderHeight();
                IsLoaded = false;
            });
        }

        public double CalcCategoryBorderHeight() 
        {
            if (OverviewModel.InstanceData.Categories == null || OverviewModel.InstanceData.Categories.Count == 0)
            {
                IsCategoriesExist = false;
                return 0.0;
            }

            IsCategoriesExist = true;

            var childWidth = 0.0;
            foreach (var item in _overviewModel.InstanceData.Categories)
            {
                childWidth += Constants.TagSizes[item.name];
            }
            if (childWidth < 326.5)
                return 40;
            else
                return 60;
        }
    }
}
