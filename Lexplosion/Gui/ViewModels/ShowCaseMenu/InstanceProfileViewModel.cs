using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Network;
using Lexplosion.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.Gui.ViewModels.ShowCaseMenu
{
    public class InstanceProfileViewModel : VMBase
    {
        public InstanceProfileViewModel(InstanceClient instanceClient)
        {
            CurrentInstanceClient = instanceClient;
            BaseInstanceData = CurrentInstanceClient.GetBaseData;
            IsModloader = BaseInstanceData.Modloader != ModloaderType.Vanilla;
            ModloaderType = BaseInstanceData.Modloader;
            ModloaderVersion = BaseInstanceData.ModloaderVersion;
            GameVersion = BaseInstanceData.GameVersion ?? GameVersions[0];
            LogoBytes = CurrentInstanceClient.Logo;
        }

        /// <summary>
        /// Версия игры.
        /// </summary>
        private string _gameVersion;
        public string GameVersion 
        {
            get => _gameVersion; set
            {
                _gameVersion = BaseInstanceData.GameVersion = value;

                if (ModloaderType != ModloaderType.Vanilla) 
                {
                    GetModloaderVersions(value, ModloaderType);
                }

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Ссылка на изменяемую сборку.
        /// </summary>
        public InstanceClient CurrentInstanceClient { get; }

        /// <summary>
        /// Основная информация об изменяемой сборке.
        /// </summary>
        public BaseInstanceData BaseInstanceData { get; }

        /// <summary>
        /// Список версий майнкрафта.
        /// </summary>
        public List<string> GameVersions { get; } = MainViewModel.GameVersions.ToList();

        /// <summary>
        /// Список версий конкретного modloader, для конкретной версии.
        /// </summary>
        private ObservableCollection<string> _modloaderVersions;
        public ObservableCollection<string> ModloaderVersions 
        {
            get => _modloaderVersions; set 
            {
                _modloaderVersions = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Свойство содержит путь к выбранной картинке
        /// </summary>
        private string _logoPath;
        public string LogoPath
        {
            get => _logoPath; set
            {
                _logoPath = value;
                LogoBytes = ImageTools.GetImageBytes(value);
                OnPropertyChanged();
            }
        }

        private byte[] _logoBytes;
        public byte[] LogoBytes 
        {
            get => _logoBytes; set 
            {
                _logoBytes = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Свойство которое содержит результат проверки наличия modloader'a
        /// Используется для скрытия поля версии modloader'a, если он отсутствует.
        /// </summary>

        private bool _isModloader;
        public bool IsModloader 
        {
            get => _isModloader; set 
            {
                _isModloader = value;
                OnPropertyChanged();
            } 
        }

        private ModloaderType modloaderType;
        public ModloaderType ModloaderType
        {
            get => modloaderType; set 
            {
                modloaderType = BaseInstanceData.Modloader = value;

                // проверяем выбран ли modloader
                IsModloader = modloaderType != ModloaderType.Vanilla;

                // загружаем версии modloader для конкретной версии.
                GetModloaderVersions(BaseInstanceData.GameVersion, value);

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Выбранная версия modloader
        /// </summary>
        private string _modloaderVersion;
        public string ModloaderVersion 
        {
            get => _modloaderVersion; set 
            {
                _modloaderVersion = BaseInstanceData.ModloaderVersion = value;
                OnPropertyChanged();
            }
        }

        #region commands

        /// <summary>
        /// Команда выполняющаяся при нажатии кнопки "Сохранить"
        /// </summary>
        public RelayCommand SaveDataCommand
        {
            get => new RelayCommand(obj =>
            {
                CurrentInstanceClient.ChangeParameters(BaseInstanceData, LogoPath);
            });
        }

        /// <summary>
        /// Команда выполняющаяся при нажатии кнопки "Загрузить своё изображение"
        /// </summary>
        public RelayCommand UploadLogoCommand
        {
            get => new RelayCommand(obj => 
            {
                using (var dialog = new System.Windows.Forms.OpenFileDialog()) 
                { 
                    dialog.Filter = "Image files|*.bmp;*.jpg;*.gif;*.png;*.tif|All files|*.*";

                    // Process open file dialog box results
                    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) 
                    {
                        LogoPath = dialog.FileName;
                    }
                }
            });
        }

        #endregion commands

        #region methods

        private void GetModloaderVersions(string gameVersion, ModloaderType modloader)
        {
            Lexplosion.Run.TaskRun(() =>
            {
                ModloaderVersions = new ObservableCollection<string>(ToServer.GetModloadersList(gameVersion, modloader));
                if (CurrentInstanceClient.GetBaseData.Modloader != ModloaderType.Vanilla)
                {
                    ModloaderVersion = CurrentInstanceClient.GetBaseData.ModloaderVersion;
                }
                else 
                {
                    if (ModloaderVersions.Count > 0)
                        ModloaderVersion = ModloaderVersions[0];
                    Console.WriteLine(gameVersion + " " + modloader);
                }
            });
        }

        #endregion
    }
}
