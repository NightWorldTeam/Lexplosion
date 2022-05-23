using Lexplosion.Gui.Models.ShowCaseMenu;
using Lexplosion.Logic.Management.Instances;
using System;
using System.Collections.Generic;

namespace Lexplosion.Gui.ViewModels.ShowCaseMenu
{
    public class OverviewViewModel : VMBase
    {
        private readonly Dictionary<string, double> tagSizes = new Dictionary<string, double>()
        {
            { "Tech", 36.5333333333333 },
            { "Magic", 46.9233333333333},
            { "Sci-Fi", 49.88},
            { "Adventure and RPG", 132.433333333333},
            { "Exploration", 80.4466666666667},
            { "Mini Game", 77.18},
            { "Quests", 51.3233333333333},
            { "Hardcore", 66.5366666666667},
            { "Map Based", 78.2066666666667},
            { "Small / Light", 88.28},
            { "Extra Large", 78.74},
            { "Combat / PvP", 95.0533333333333},
            { "Multiplayer", 80.5133333333333},
            { "FTB Official Pack", 113.16},
            { "Skyblock", 64.4766666666667},
            { "Vanilla+", 49.71}
        };

        private bool _isLoaded = true;
        private double _categoryPanelBorderHeight;
        private OverviewModel _overviewModel;
        
        #region props
        public OverviewModel OverviewModel 
        { 
            get => _overviewModel; set 
            {
                _overviewModel = value;
                OnPropertyChanged(nameof(OverviewModel));
            } 
        }

        public double CategoryPanelBorderHeight
        {
            get => _categoryPanelBorderHeight; set 
            {
                _categoryPanelBorderHeight = value;
                OnPropertyChanged(nameof(CategoryPanelBorderHeight));
            }
        }

        public bool IsLoaded
        {
            get => _isLoaded;
            set
            {
                _isLoaded = value;
                OnPropertyChanged(nameof(IsLoaded));
            }
        }
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
                CategoryPanelBorderHeight = CalcCategoryBorderHeight();
                IsLoaded = false;
            });
        }

        public double CalcCategoryBorderHeight() 
        {
            var childWidth = 0.0;
            foreach (var item in OverviewModel.InstanceData.Categories)
            {
                childWidth += tagSizes[item.name];
            }
            if (childWidth < 326.5)
                return 40;
            else
                return 60;
        }
    }
}
