using Lexplosion.Common.Models.ShowCaseMenu;
using Lexplosion.Logic.Management.Instances;
using System.Collections;

namespace Lexplosion.Common.ViewModels.ShowCaseMenu
{
    public class OverviewViewModel : VMBase
    {
        #region Properties


        private OverviewModel _overviewModel;
        public OverviewModel Model
        {
            get => _overviewModel; private set
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
            get => _isLoading; private set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public bool IsCategoriesExist;


        #endregion Properties


        #region Commands


        //TODO: Переименовать команду
        private RelayCommand _curseforgePageCommand;
        public RelayCommand CurseforgePageCommand
        {
            get => _curseforgePageCommand ?? (_curseforgePageCommand = new RelayCommand(obj =>
            {
                try
                {
                    System.Diagnostics.Process.Start(Model.InstanceData.WebsiteUrl);
                }
                catch
                {
                    // message box here.
                }
            }));
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
                return 0.0;
            }

            var childWidth = 0.0;
            foreach (var item in _overviewModel.InstanceData.Categories)
            {
                double width;
                if (Constants.TagSizes.TryGetValue(item.Name, out width))
                {
                    childWidth += Constants.TagSizes[item.Name];
                }
            }

            return childWidth < 326.5 ? 40 : 60;
        }


        #endregion Private Methods
    }
}
