using Lexplosion.Gui.ViewModels;
using Lexplosion.Gui.Views.Windows;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Network;
using Lexplosion.Tools.Immutable;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Lexplosion.Gui.Models.InstanceFactory
{
    interface IExtensionModel
    {
        public string GameVersion { get; }
        public GameExtension GameExtension { get; }
        public bool IsAvaliable { get; }
        public string Version { get; set; }
        public ImmutableArray<string> Versions { get; }
    }

    public abstract class ExtensionModel : VMBase, IExtensionModel
    {
        private static readonly Dictionary<GameExtension, ConcurrentDictionary<string, ImmutableArray<string>>> _extensionVersions;

        #region Properties

        public bool IsEnable { get; set; }
        public string GameVersion { get; }

        public GameExtension GameExtension { get; set; }


        /// <summary>
        /// Доступно ли расшерение на эту версию игры.
        /// </summary>
        private bool _isAvaliable;
        public bool IsAvaliable
        {
            get => _isAvaliable; private set
            {
                _isAvaliable = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Версия расширения;
        /// </summary>
        private string _version;
        public string Version
        {
            get => _version; set
            {
                _version = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Массив с версиями расширения
        /// </summary>
        private ImmutableArray<string> _versions;
        public ImmutableArray<string> Versions
        {
            get => _versions; private set
            {
                _versions = value;
                OnPropertyChanged();
            }
        }

        #endregion Properties

        static ExtensionModel() 
        {
            _extensionVersions = new Dictionary<GameExtension, ConcurrentDictionary<string, ImmutableArray<string>>>()
            {
                { GameExtension.Optifine, new ConcurrentDictionary<string, ImmutableArray<string>>() },
                { GameExtension.Forge, new ConcurrentDictionary<string, ImmutableArray<string>>() },
                { GameExtension.Fabric, new ConcurrentDictionary<string, ImmutableArray<string>>() },
                { GameExtension.Quilt, new ConcurrentDictionary<string, ImmutableArray<string>>() },
            };
        } 

        protected ExtensionModel(GameExtension extension, string gameVersion)
        {
            GameVersion = gameVersion;
            Lexplosion.Runtime.TaskRun(() => {
                Runtime.DebugWrite(extension);
                GameExtension = extension;
                Versions = LoadExtensionVersions(extension, gameVersion).Result;
                if (Versions.Count > 0)
                {
                    IsAvaliable = true;
                    Version = Versions[0];
                }
                else
                {
                    IsAvaliable = false;
                }
            });
        }

        /// <summary>
        /// Возвращает неизменяемый массив с версиями расширениями.
        /// Также сохраняет массив в словарь.
        /// </summary>
        /// <param name="GameExtension"></param>
        /// <param name="gameVersion"></param>
        /// <returns></returns>
        public async static Task<ImmutableArray<string>> LoadExtensionVersions(GameExtension extension, string gameVersion)
        {
            return await Task.Run(() =>
            {
                Runtime.DebugWrite(extension);
                if (_extensionVersions[extension].ContainsKey(gameVersion))
                    return _extensionVersions[extension][gameVersion];

                if (GameExtension.Optifine == extension)
                {
                    _extensionVersions[extension].TryAdd(gameVersion, new ImmutableArray<string>(ToServer.GetOptifineVersions(gameVersion)));
                }
                else 
                { 
                    _extensionVersions[extension].TryAdd(gameVersion, new ImmutableArray<string>(ToServer.GetModloadersList(gameVersion, (ClientType)extension)));
                }
                return _extensionVersions[extension][gameVersion];
            });
        }
    }

    public sealed class OptifineModel : ExtensionModel
    {
        public OptifineModel(GameExtension extension, string gameVersion) : base(extension, gameVersion)
        {
        }
    }

    public sealed class ModloaderModel : ExtensionModel
    {
        public ModloaderModel(GameExtension extension, string gameVersion) : base(extension, gameVersion)
        {
        }
    }

    public class InstanceFactoryModel : VMBase
    {
        private readonly MainViewModel _mainViewModel;

        #region Property

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
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Тип игры.
        /// </summary>
        private GameType _gameType;
        public GameType GameType 
        { 
            get => _gameType;
            set
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


        #endregion

        #region Constructors

        public InstanceFactoryModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            UpdateVersions();
            OptifineModel = new OptifineModel(GameExtension.Optifine, Version);
            ModloaderModel = new ModloaderModel(GameExtension.Forge, Version);
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
                gameVersions = MainViewModel.AllGameVersions.ToArray();
            }
            else gameVersions = MainViewModel.ReleaseGameVersions.ToArray();
            Version = gameVersions[0];
            GameVersions = gameVersions;
            Version = gameVersions[0];
        }

        #endregion Private Methods

        public void ChangeClientType(GameType gameType, GameExtension extension) 
        {
            GameType = gameType;
            if (gameType == GameType.Vanilla) 
            {
                ModloaderModel.IsEnable = false;
                return;
            }

            if (extension != GameExtension.Optifine) { 
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

        #region Public Static Methods


        public static void CreateLocalInstance(MainViewModel mainViewModel, string name, string version, string logoPath,
            ClientType modloaderType, string modloaderVersion = null, string optifineVersion = null
            )
        {
            var instance = InstanceClient.CreateClient(
                name,
                InstanceSource.Local,
                version,
                modloaderType,
                logoPath,
                modloaderVersion,
                optifineVersion
                );

            mainViewModel.Model.LibraryInstances.Add(new InstanceFormViewModel(mainViewModel, instance));
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
                    modloaderVersion: model.ModloaderModel.Version
                    );
            }
        }


        #endregion Public Static Methods
    }
}