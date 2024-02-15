using Lexplosion.Common.Models.GameExtensions;
using Lexplosion.Common.ViewModels;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Management.Instances;
using System;

namespace Lexplosion.Common.Models.InstanceFactory
{
    public class InstanceFactoryModel : VMBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly Action<ClientType> _changeSelectedClientType;


        #region Properties


        /// <summary>
        /// Название сборки.
        /// </summary>
        private string _name;
        public string Name
        {
            get => _name; set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Текст плейсхолдера.
        /// </summary>
        public string Placeholder
        {
            get
            {
                if (_name != null) return _name;

                if (GameType == GameType.Vanilla)
                {
                    return Version + " Vanilla";
                }
                else
                {
                    return Version + " " + ModloaderModel.GameExtension;
                }
            }
        }

        /// <summary>
        /// Версия игры.
        /// </summary>
        private MinecraftVersion _version;
        public MinecraftVersion Version
        {
            get => _version; set
            {
                _version = value ?? GameVersions[0];
               
                if (ModloaderModel != null)
                {
                    ModloaderModel = new ModloaderModel(GameExtension.Fabric, Version.Id, OnAvailiableChanged);
                    ModloaderModel = new ModloaderModel(GameExtension.Quilt, Version.Id, OnAvailiableChanged);
                    ModloaderModel = new ModloaderModel(GameExtension.Forge, Version.Id, OnAvailiableChanged);
                    ModloaderModel = new ModloaderModel(ModloaderModel.GameExtension, _version.Id, OnAvailiableChanged);
                }
                if (OptifineModel != null)
                {
                    OptifineModel = new OptifineModel(GameExtension.Optifine, _version.Id);
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(Placeholder));
            }
        }

        private void OnAvailiableChanged(bool value)
        {
            if (!value)
            {
                ChangeClientType(GameType.Vanilla, GameExtension.Optifine);
                _changeSelectedClientType(ClientType.Vanilla);
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
                    OptifineModel.IsEnable = true;
                }
                else
                {
                    ModloaderModel.IsEnable = false;
                    OptifineModel.IsEnable = true;
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(Placeholder));
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


        /// <summary>
        /// Путь до картинки.
        /// </summary>
        private string _logoPath;
        public string LogoPath
        {
            get => _logoPath; set
            {
                _logoPath = value;
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
                OnPropertyChanged(nameof(Placeholder));
            }
        }

        private bool _isSodium;
        public bool IsSodium
        {
            get => _isSodium && (ModloaderModel.GameExtension == GameExtension.Fabric || ModloaderModel.GameExtension == GameExtension.Quilt); set
            {
                _isSodium = value;
                OnPropertyChanged();
            }
        }

        /*** Outside Data ***/


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
                if (value.Length > 0) 
                { 
                    Version = value?[0];
                }
            }
        }


        #endregion Properties


        #region Constructors

        public InstanceFactoryModel(MainViewModel mainViewModel, Action<ClientType> changeSelectedClientType)
        {
            _mainViewModel = mainViewModel;
            _changeSelectedClientType = changeSelectedClientType;

            UpdateVersions();

            Runtime.DebugWrite(Version.Id);

            if (Version != null) 
            {
                OptifineModel = new OptifineModel(GameExtension.Optifine, Version.Id);
                ModloaderModel = new ModloaderModel(GameExtension.Fabric, Version.Id, OnAvailiableChanged);
                ModloaderModel = new ModloaderModel(GameExtension.Quilt, Version.Id, OnAvailiableChanged);
                ModloaderModel = new ModloaderModel(GameExtension.Forge, Version.Id, OnAvailiableChanged);
            }

            ModloaderModel.IsEnable = false;
            OptifineModel.IsEnable = false;
        }

        #endregion Constructors


        #region Private Methods

        private void UpdateVersions()
        {
            GameVersions = IsShowSnapshots ? MainViewModel.AllGameVersions : MainViewModel.ReleaseGameVersions;
            if (GameVersions.Length > 0) 
            { 
                Version = GameVersions[0];
            }
        }

        #endregion Private Methods


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

        public void CreateInstance()
        {
            if (GameType == GameType.Vanilla)
            {
                if (OptifineModel.IsEnable && string.IsNullOrEmpty(OptifineModel.Version))
                {
                    return;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(ModloaderModel.Version))
                {
                    return;
                }
            }

            CreateLocalInstance(_mainViewModel, this);
        }


        #endregion Public & Private Methods


        #region Public Static Methods


        public static void CreateLocalInstance(MainViewModel mainViewModel, string name, MinecraftVersion version, string logoPath,
            ClientType modloaderType, string modloaderVersion = null, string optifineVersion = null, bool isSodium = false
            )
        {
            var instance = InstanceClient.CreateClient(
                name,
                InstanceSource.Local,
                version,
                modloaderType,
                logoPath,
                modloaderVersion,
                optifineVersion,
                isSodium
                );

            MainModel.Instance.LibraryController.AddInstance(new InstanceFormViewModel(mainViewModel, instance));
        }

        public static void CreateLocalInstance(MainViewModel mainViewModel, InstanceFactoryModel model)
        {
            if (model.GameType == GameType.Vanilla)
            {
                CreateLocalInstance(
                    mainViewModel,
                    name: model.Name ?? model.Version.ToString(),
                    version: model.Version,
                    logoPath: model.LogoPath,
                    ClientType.Vanilla,
                    //modloaderVersion: model.ModloaderVersion,
                    optifineVersion: model.OptifineModel.IsEnable ? model.OptifineModel.Version : null
                    );
            }
            else
            {
                CreateLocalInstance(
                    mainViewModel,
                    name: model.Name ?? model.Version.ToString() + " " + model.ModloaderModel.GameExtension,
                    version: model.Version,
                    logoPath: model.LogoPath,
                    (ClientType)model.ModloaderModel.GameExtension,
                    modloaderVersion: model.ModloaderModel.Version,
                    isSodium: model.IsSodium
                    );
                Runtime.DebugWrite(model.IsSodium);
            }
        }


        #endregion Public Static Methods
    }
}