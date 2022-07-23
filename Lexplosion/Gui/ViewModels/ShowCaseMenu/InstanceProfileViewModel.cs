using Lexplosion.Logic.Management.Instances;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Gui.ViewModels.ShowCaseMenu
{
    public class InstanceProfileViewModel : VMBase
    {
        public InstanceClient CurrentInstanceClient { get; }

        public BaseInstanceData BaseInstanceData { get; }

        public List<string> GameVersions { get; } = MainViewModel.GameVersions.ToList();

        /// <summary>
        /// Свойство которое содержит результат проверки наличия modloader'a
        /// Используется для скрытия поля версии modloader'a, если он отсутствует.
        /// </summary>
        public bool IsModloader { get => BaseInstanceData.Modloader != ModloaderType.None; }

        #region commands

        /// <summary>
        /// Команда выполняющаяся при нажатии кнопки "Сохранить"
        /// </summary>
        public RelayCommand SaveDataCommand
        {
            get => new RelayCommand(obj =>
            {
                CurrentInstanceClient.ChangeParameters(BaseInstanceData);
            });
        }

        /// <summary>
        /// Команда выполняющаяся при нажатии кнопки "Загрузить своё изображение"
        /// </summary>
        public RelayCommand UploadLogoCommand
        {
            get => new RelayCommand(obj => 
            {
                var ofd = new OpenFileDialog();
                ofd.Filter = "Image files|*.bmp;*.jpg;*.gif;*.png;*.tif|All files|*.*";
                ofd.FilterIndex = 1;

                if (ofd.ShowDialog() == true) 
                {
                    //BaseInstanceData.Logo = 
                }
            });
        }

        #endregion commands

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

        public InstanceProfileViewModel(InstanceClient instanceClient)
        {
            CurrentInstanceClient = instanceClient;
            BaseInstanceData = CurrentInstanceClient.GetBaseData;
        }
    }
}
