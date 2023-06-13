using Lexplosion.Common.Models.GameExtensions;
using Lexplosion.Common.ViewModels;
using Lexplosion.Common.Views.Windows;
using Lexplosion.Logic.Management.Instances;

namespace Lexplosion.Common.Models.InstanceFactory
{
    public class InstanceFactoryModel : VMBase
    {
        private readonly MainViewModel _mainViewModel;


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
        private string _version;
        public string Version
        {
            get => _version; set
            {
                _version = value;
                if (ModloaderModel != null)
                {
                    ModloaderModel = new ModloaderModel(ModloaderModel.GameExtension, _version);
                }
                if (OptifineModel != null)
                {
                    OptifineModel = new OptifineModel(GameExtension.Optifine, _version);
                }

                var splitedValue = value.Split('.');

                OnPropertyChanged();
                OnPropertyChanged(nameof(Placeholder));
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
        private string[] _gameVersions;
        public string[] GameVersions
        {
            get => _gameVersions; set
            {
                _gameVersions = value;
                OnPropertyChanged();
            }
        }


        #endregion Properties


        #region Constructors

        public InstanceFactoryModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            UpdateVersions();
            OptifineModel = new OptifineModel(GameExtension.Optifine, Version);
            ModloaderModel = new ModloaderModel(GameExtension.Forge, Version);
            new ModloaderModel(GameExtension.Fabric, Version);
            new ModloaderModel(GameExtension.Quilt, Version);
            ModloaderModel.IsEnable = false;
            OptifineModel.IsEnable = false;
        }

        #endregion Constructors


        #region Private Methods

        private void UpdateVersions()
        {
            string[] gameVersions;
            if (IsShowSnapshots)
            {
                gameVersions = MainViewModel.AllGameVersions;
            }
            else gameVersions = MainViewModel.ReleaseGameVersions;
            Version = gameVersions != null && gameVersions.Length != 0 ? gameVersions[0] : "1.19.2";
            GameVersions = gameVersions;
            Version = gameVersions[0];
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
                ModloaderModel = new ModloaderModel(extension, Version);
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


        public static void CreateLocalInstance(MainViewModel mainViewModel, string name, string version, string logoPath,
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
            var instanceVersion = model.Version.Replace("snapshot ", "").Replace("release ", "");

            if (model.GameType == GameType.Vanilla)
            {
                CreateLocalInstance(
                    mainViewModel,
                    name: model.Name ?? model.Version,
                    version: instanceVersion,
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
                    name: model.Name ?? model.Version + " " + model.ModloaderModel.GameExtension,
                    version: instanceVersion,
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