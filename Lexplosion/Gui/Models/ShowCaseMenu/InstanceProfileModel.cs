using Lexplosion.Gui.Models.GameExtensions;
using Lexplosion.Gui.ViewModels;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Tools;
using System.Collections.Generic;
 
namespace Lexplosion.Gui.Models.ShowCaseMenu
{
    public class InstanceProfileModel : VMBase
    {
        public InstanceProfileModel(InstanceClient instanceClient)
        {
            CurrentInstanceClient = instanceClient;
            BaseInstanceData = CurrentInstanceClient.GetBaseData;
            UpdateVersions();
            Version = BaseInstanceData.GameVersion ?? GameVersions[0];
            OptifineModel = new OptifineModel(GameExtension.Optifine, Version);

            if (BaseInstanceData.Modloader != ClientType.Vanilla)
            {
                ModloaderModel = new ModloaderModel((GameExtension)BaseInstanceData.Modloader, Version);
            }
            else 
            {
                ModloaderModel = new ModloaderModel(GameExtension.Forge, Version);
                OptifineModel.IsEnable = false;
            }

            GameType = BaseInstanceData.Modloader == ClientType.Vanilla ? GameType.Vanilla : GameType.Modded;
            Runtime.DebugWrite(GameType);
            LogoBytes = CurrentInstanceClient.Logo;
            if (BaseInstanceData.OptifineVersion != null) 
            { 
                OptifineModel.Version = BaseInstanceData.OptifineVersion;
            }
            if (BaseInstanceData.ModloaderVersion != null) 
            { 
                ModloaderModel.Version = BaseInstanceData.ModloaderVersion;
            }
        }


        #region Properties



        #region Extensions

        /// <summary>
        /// Модель для модлоадеров
        /// </summary>
        private ExtensionModel _modloaderModel;
        public ExtensionModel ModloaderModel
        {
            get => _modloaderModel; set
            {
                _modloaderModel = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Модель для оптифайна
        /// </summary>
        private ExtensionModel _optifineModel;
        public ExtensionModel OptifineModel
        {
            get => _optifineModel; set
            {
                _optifineModel = value;
                OnPropertyChanged();
            }
        }

        #endregion Extensions


        /// <summary>
        /// Версия игры.
        /// </summary>
        private string _version;
        public string Version
        {
            get => _version; set
            {
                _version = BaseInstanceData.GameVersion = value;
                if (ModloaderModel != null)
                {
                    ModloaderModel = new ModloaderModel(ModloaderModel.GameExtension, _version);
                }
                if (OptifineModel != null)
                {
                    OptifineModel = new OptifineModel(GameExtension.Optifine, _version);
                }

                OnPropertyChanged();
            }
        }

        #region what ever
        /// <summary>
        /// Ссылка на изменяемую сборку.
        /// </summary>
        public InstanceClient CurrentInstanceClient { get; }

        /// <summary>
        /// Основная информация об изменяемой сборке.
        /// </summary>
        public BaseInstanceData BaseInstanceData { get; }

        /// <summary>
        /// Массив с версиями игры
        /// </summary>
        private string[] _gameVersions;
        public string[] GameVersions
        {
            get => _gameVersions; set
            {
                _gameVersions = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Позволить выбирать снапшоты?
        /// </summary>
        private bool _isShowSnapshots;
        public bool IsShowSnapshots
        {
            get => _isShowSnapshots; set
            {
                _isShowSnapshots = value;
                UpdateVersions();
                OnPropertyChanged();
            }
        }

        #endregion


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
        /// Тип игры.
        /// </summary>
        private GameType _gameType;
        public GameType GameType
        {
            get => _gameType; set 
            {
                _gameType = value;
                BaseInstanceData.Modloader = (ClientType)value;
                if (_gameType == GameType.Vanilla)
                {
                    ModloaderModel.IsEnable = false;
                    OptifineModel.IsEnable = true;
                }
                else
                {
                    ModloaderModel.IsEnable = false;
                    OptifineModel.IsEnable = true;
                }
                OnPropertyChanged();
            }
        }


        #endregion Properties


        #region Public & Protected Methods


        public void ChangeClientType(GameType gameType, GameExtension extension)
        {
            GameType = gameType;
            if (gameType == GameType.Vanilla)
            {
                ModloaderModel.IsEnable = false;
                return;
            }

            if (extension != GameExtension.Optifine)
            {
                ModloaderModel = new ModloaderModel(extension, Version);
            }
        }

        public void Save()
        {
            CurrentInstanceClient.ChangeParameters(BaseInstanceData, LogoPath);
        }

        public void UploadLogo(string fileName)
        {
            LogoPath = fileName;
        }


        public ClientType GetInstanceExtenstion() 
        {
            return GameType == GameType.Vanilla ? ClientType.Vanilla : (ClientType)ModloaderModel.GameExtension;
        }

        #endregion Public & Protected Methods


        #region Private Methods


        private void DisableModloader()
        {
            BaseInstanceData.Modloader = ClientType.Vanilla;
        }

        private void DisableOptifine()
        {
            BaseInstanceData.OptifineVersion = null;
        }

        private void UpdateVersions()
        {
            string[] gameVersions;
            if (IsShowSnapshots)
            {
                gameVersions = MainViewModel.AllGameVersions.ToArray();
            }
            else gameVersions = MainViewModel.ReleaseGameVersions.ToArray();
            GameVersions = gameVersions;
        }

        #endregion Private Methods
    }
}
