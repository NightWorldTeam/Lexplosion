using Lexplosion.Common.Models.GameExtensions;
using Lexplosion.Common.ViewModels;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Tools;
using System;

namespace Lexplosion.Common.Models.ShowCaseMenu
{
    public class InstanceProfileModel : VMBase
    {
        private readonly Action<ClientType> _changeSelectedClientType;

        public InstanceProfileModel(InstanceClient instanceClient, Action<ClientType> changeSelectedClientType)
        {
            CurrentInstanceClient = instanceClient;
            _changeSelectedClientType = changeSelectedClientType;

            IsShowSnapshots = instanceClient?.GameVersion.Type == MinecraftVersion.VersionType.Snapshot;

            BaseInstanceData = CurrentInstanceClient.GetBaseData;
            UpdateVersions();
            Version = BaseInstanceData.GameVersion ?? GameVersions[0];
            OptifineModel = new OptifineModel(GameExtension.Optifine, Version.Id);

            if (BaseInstanceData.Modloader != ClientType.Vanilla)
            {
                ModloaderModel = new ModloaderModel((GameExtension)BaseInstanceData.Modloader, Version.Id, OnAvailiableChanged);
                OptifineModel.IsEnable = false;
            }
            else
            {
                ModloaderModel = new ModloaderModel(GameExtension.Forge, Version.Id, OnAvailiableChanged);
                OptifineModel.IsEnable = BaseInstanceData.OptifineVersion != null;
            }

            GameType = BaseInstanceData.Modloader == ClientType.Vanilla ? GameType.Vanilla : GameType.Modded;

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
        private MinecraftVersion _version;
        public MinecraftVersion Version
        {
            get => _version; set
            {
                // TODO: !!! IMPORTANT !!! ~~~ ПЕРЕДАВАТЬ СЮДА КОНКРЕТНЫЙ ТИП ВЕРСИИ
                // сейчас только release.
                BaseInstanceData.GameVersion = value;
                _version = value;
                if (ModloaderModel != null)
                {
                    ModloaderModel = new ModloaderModel(ModloaderModel.GameExtension, _version.Id, OnAvailiableChanged);
                }
                if (OptifineModel != null)
                {
                    OptifineModel = new OptifineModel(GameExtension.Optifine, _version.Id);
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
        private MinecraftVersion[] _gameVersions;
        public MinecraftVersion[] GameVersions
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
                if (_gameType == GameType.Vanilla)
                {
                    ModloaderModel.IsEnable = false;
                    OptifineModel.IsEnable = BaseInstanceData.OptifineVersion != null;
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


        private void OnAvailiableChanged(bool value)
        {
            if (!value)
            {
                ChangeClientType(GameType.Vanilla, GameExtension.Optifine);
                _changeSelectedClientType(ClientType.Vanilla);
            }  
        }

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
                ModloaderModel = new ModloaderModel(extension, Version.Id, OnAvailiableChanged);
            }
        }

        public void Save()
        {
            if (GameType == GameType.Vanilla)
            {
                BaseInstanceData.Modloader = ClientType.Vanilla;
                BaseInstanceData.OptifineVersion = OptifineModel.IsEnable ? OptifineModel.Version : null;
            }
            else
            {
                BaseInstanceData.Modloader = (ClientType)ModloaderModel.GameExtension;
                BaseInstanceData.ModloaderVersion = ModloaderModel.Version;
            }
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
            GameVersions = IsShowSnapshots ? MainViewModel.AllGameVersions : MainViewModel.ReleaseGameVersions;
        }

        #endregion Private Methods
    }
}
