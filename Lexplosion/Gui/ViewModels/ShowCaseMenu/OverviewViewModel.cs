using Lexplosion.Gui.Models.ShowCaseMenu;
using Lexplosion.Logic.Management.Instances;
using System.Collections;

namespace Lexplosion.Gui.ViewModels.ShowCaseMenu
{
    public class OverviewViewModel : VMBase
    {
        #region Properties

        private OverviewModel _overviewModel;
        public OverviewModel Model
        {
            get => _overviewModel; set
            {
                _overviewModel = value;
                OnPropertyChanged();
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

        private bool _isLoadedFailed;
        public bool IsLoadedFailed
        {
            get => _isLoadedFailed; set
            {
                _isLoadedFailed = value;
                OnPropertyChanged();
            }
        }


        private bool _isLoading = true;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public bool IsCategoriesExist { get; set; }

        #endregion Properties


        #region Commands


        public RelayCommand CurseforgePageCommand
        {
            get => new RelayCommand(obj =>
            {
                try
                {
                    System.Diagnostics.Process.Start(Model.InstanceData.WebsiteUrl);
                }
                catch
                {
                    // message box here.
                }
            });
        }


        #endregion Commands


        #region Constructors
        public OverviewViewModel(InstanceClient instanceClient, ISubmenu submenuViewModel)
        {
            Lexplosion.Runtime.TaskRun(() =>
            {
                Model = new OverviewModel(instanceClient, submenuViewModel, this);
                CategoryPanelBorderHeight = CalcCategoryBorderHeight();
                IsLoading = false;
            });
        }
        #endregion Constructors


        #region Private Methods
        private double CalcCategoryBorderHeight()
        {
            if (Model.InstanceData?.Categories == null || ((ICollection)Model.InstanceData.Categories).Count == 0)
            {
                IsCategoriesExist = false;
                return 0.0;
            }

            IsCategoriesExist = true;

            var childWidth = 0.0;
            foreach (var item in _overviewModel.InstanceData.Categories)
            {
                if (Constants.TagSizes.ContainsKey(item.Name)) 
                { 
                    childWidth += Constants.TagSizes[item.Name];
                }
            }

            if (childWidth < 326.5)
                return 40;
            else
                return 60;
        }

        #endregion Private Methods
    }
}
