using Lexplosion.Logic.Management.Instances;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Gui.ViewModels.ShowCaseMenu
{
    public class InstanceCreationViewModel : VMBase
    {
        public InstanceClient CurrentInstanceClient { get; }

        public BaseInstanceData BaseInstanceData { get; }

        public List<string> GameVersions { get; } = MainViewModel.GameVersions.ToList();

        public RelayCommand SaveDataCommand
        {
            get => new RelayCommand(obj =>
            {
                CurrentInstanceClient.ChangeParameters(BaseInstanceData);
            });
        }

        public string[] ModpacksCategoryNames { get; } = new string[16]
        {
            "Tech",
            "Magic",
            "Sci-Fi",
            "Adventure and RPG",
            "Exploration",
            "Mini Game",
            "Quests",
            "Hardcore",
            "Map Based",
            "Small/Ligth",
            "Extra/Large",
            "Combat / PVP",
            "Multiplayer",
            "FTB Offical Pack",
            "Skyblock",
            "Vanilla+"
        };

        public InstanceCreationViewModel(InstanceClient instanceClient)
        {
            CurrentInstanceClient = instanceClient;
            BaseInstanceData = CurrentInstanceClient.GetBaseData;
        }
    }
}
