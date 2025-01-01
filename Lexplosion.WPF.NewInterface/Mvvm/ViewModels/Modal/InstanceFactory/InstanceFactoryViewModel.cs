using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Management;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.GameExtensions;
using Lexplosion.WPF.NewInterface.Core.Modal;
using System;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal
{
    public class InstanceFactoryModel : ViewModelBase 
    {
        public event Action<ClientType> GameTypeChanged;


        #region Properties


        public string InstanceName { get; set; }



        #region Versions


        /// <summary>
        /// Список версий майнкрафта
        /// </summary>
        public MinecraftVersion[] GameVersions { get => IsShowSnapshots ? MainViewModel.AllGameVersions ?? new MinecraftVersion[1] : MainViewModel.ReleaseGameVersions ?? new MinecraftVersion[1]; }
        /// <summary>
        /// Если ли хоть одна версия майнкрафта.
        /// </summary>
        public bool IsGameVersionsAvaliable { get => GameVersions?.Length > 0; }

        /// <summary>
        /// Версия сборки
        /// </summary>
        private MinecraftVersion _version;
        public MinecraftVersion Version
        {
            get => _version; set
            {
                _version = value;
                VersionChanged();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Показывать ли снапшоты в массиве GameVersions.
        /// </summary>
        private bool _isShowSnapshots;
        public bool IsShowSnapshots
        {
            get => _isShowSnapshots; set
            {
                _isShowSnapshots = value;
                OnPropertyChanged();

                // Убираем пролаг при клике.
                // P.S Это не должно делаться через виртуализацию
                Lexplosion.Runtime.TaskRun(() =>
                {
                    OnPropertyChanged(nameof(GameVersions));
                    OnPropertyChanged(nameof(IsGameVersionsAvaliable));
                });
            }
        }


        #endregion Versions


        #region Modloader


        private ClientType _clientType;
        public ClientType ClientType
        {
            get => _clientType; set
            {
                _clientType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsOptifine));
            }
        }
        /// <summary>
        /// Modloader Manager (for fabric, forge...)
        /// </summary>
        public ModloaderManager ModloaderManager { get; private set; }
        /// <summary>
        /// Тип выбранного модлоадера.
        /// </summary>
        private GameExtension _modloaderType;
        public GameExtension ModloaderType
        {
            get => _modloaderType; set
            {
                _modloaderType = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Версия выбранного модлоадера.
        /// </summary>
        private string _modloaderVersion = string.Empty;
        public string ModloaderVersion
        {
            get => _modloaderVersion; set
            {
                _modloaderVersion = value;
                OnPropertyChanged();
            }
        }


        #endregion Modloader


        #region OptimizationMods


        /// <summary>
        /// Optimization Mod Manager (for optifine, sodium, optifabric, ...)
        /// </summary>
        public OptimizationModManager OptimizationModManager { get; private set; }
        /// <summary>
        /// Включен ли оптифайн.
        /// </summary>
        private bool _isOptifine;
        public bool IsOptifine
        {
            get => _isOptifine && ClientType == ClientType.Vanilla; set
            {
                _isOptifine = value;
                OnPropertyChanged(nameof(OptifineVersion));
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Выбранная версия оптифайна.
        /// </summary>
        private string _optifineVersion;
        public string OptifineVersion
        {
            get => _optifineVersion; set
            {
                _optifineVersion = value;
                OnPropertyChanged();
            }
        }


        #endregion OptimizationMods


        #endregion Properties


        #region Constructors


        public InstanceFactoryModel()
        {
            //_instanceModelBase = instanceModelBase;

            //_instanceData = instanceModelBase.InstanceData;
            //_oldInstanceData = instanceModelBase.InstanceData;

            ModloaderManager = new ModloaderManager(GameExtension.Forge, Version);
            Version = GameVersions[0];
            ClientType = ClientType.Vanilla;
        }


        #endregion Constructors


        #region Public Methods


        public void ChangeGameType(object parameter)
        {
            ClientType modloader; 
            if (parameter == null)
                modloader = default(ClientType);
            
            if (parameter is ClientType)
            {
                ClientType = (ClientType)parameter;

                if (ClientType != ClientType.Vanilla)
                {
                    UpdateModloaderManager(ClientType, Version);
                }
            }
        }

        /// <summary>
        /// Сохраняет промежуточные изменения.
        /// </summary>
        public InstanceClient CreateInstance()
        {
            return InstanceClient.CreateClient(
                InstanceName ?? Version.ToString(),
                InstanceSource.Local,
                Version,
                ClientType,
                null,
                ModloaderVersion,
                ClientType == ClientType.Vanilla && IsOptifine ? OptifineVersion : null,
                false
                );
        }


        #endregion Public Methods


        #region Private Methods


        /// <summary>
        /// Вызывается при изменении версии игры. Изменяет данные версии игры в BaseInstanceData<br/>
        /// Вызывает метод OnPropertyChanged() для свойства HasChanges, вызывает метод UpdateModloaderManager.
        /// </summary>
        private void VersionChanged()
        {
            UpdateModloaderManager(ClientType, Version);
            UpdateOptimizationModManager(Version);
        }

        /// <summary>
        /// Изменяет выбранный модлоадер в зависимости его типа и версии игры.<br/>
        /// Переводит ClientType в GameExtension вызывает метод UpdateModloaderManager(GameExtension, MinecraftVersion);
        /// </summary>
        /// <param name="type">Тип модлоадера (игнорирует ClientType.Vanilla)</param>
        /// <param name="version">Версия игры</param>
        private void UpdateModloaderManager(ClientType type, MinecraftVersion version, string modloaderVersion = null)
        {
            UpdateModloaderManager((GameExtension)type, version, modloaderVersion);
        }

        /// <summary>
        /// Изменяет выбранный модлоадер в зависимости его типа и версии игры.<br/>
        /// Вызывает метод OnPropertyChanged() для свойства ModloaderManager и HasChanges.
        /// </summary>
        /// <param name="type">Тип модлоадера (игнорирует GameExtension.Optifine)</param>
        /// <param name="version">Версия игры</param>
        private void UpdateModloaderManager(GameExtension type, MinecraftVersion version, string modloaderVersion = null)
        {
            // (пере)Создаём менеджер
            ModloaderManager = new ModloaderManager(type, version);

            ModloaderManager.MinecraftExtensionLoaded += (mExtension) =>
            {
                if (ModloaderManager.CurrentMinecraftExtension.IsAvaliable && string.IsNullOrEmpty(modloaderVersion))
                {
                    ModloaderVersion = ModloaderManager.CurrentMinecraftExtension.Versions[0];
                }
                else if (!string.IsNullOrEmpty(modloaderVersion))
                {
                    ModloaderVersion = modloaderVersion;
                }
                else
                {
                    GameTypeChanged?.Invoke(ClientType.Vanilla);
                }
            };

            ModloaderManager.UpdateAllProperties();

            // Если есть версия и версии модлоадера по умолчанию нет, то устанавливаем первую версию по умолчанию.
            // Иначе устанавиливаем значение из аргумента.
            OnPropertyChanged(nameof(ModloaderManager));
        }


        /// <summary>
        /// Изменяет выбранный оптимизирующий мод в зависимости его типа и версии игры.<br/>
        /// Вызывает метод OnPropertyChanged() для свойства OptimizationModManager и HasChanges.
        /// </summary>
        /// <param name="type">Тип расширения (пока не принимает данный параметр)</param>
        /// <param name="version">Версия игры</param>
        private void UpdateOptimizationModManager(MinecraftVersion minecraftVersion, string optifimizationModVersion = null)
        {
            OptimizationModManager = new OptimizationModManager(minecraftVersion);

            OptimizationModManager.MinecraftExtensionLoaded += (mExtension) =>
            {
                if (OptimizationModManager.CurrentMinecraftExtension.IsAvaliable && string.IsNullOrEmpty(optifimizationModVersion))
                {
                    OptifineVersion = OptimizationModManager.CurrentMinecraftExtension.Versions[0];
                }
                else if (!string.IsNullOrEmpty(optifimizationModVersion))
                {
                    OptifineVersion = optifimizationModVersion;
                }
            };

            // Если есть версия и версии оптифайна по умолчанию нет, то устанавливаем первую версию по умолчанию.
            // Иначе устанавиливаем значение из аргумента.
            OnPropertyChanged(nameof(OptimizationModManager));
        }


        #endregion Private Methods
    }

    public sealed class InstanceFactoryViewModel : ActionModalViewModelBase
    {
        #region Properties

        public InstanceFactoryModel Model { get; }


        private bool _isVanilla;
        public bool IsVanilla
        {
            get => _isVanilla; set
            {
                _isVanilla = value;
                OnPropertyChanged();
            }
        }

        private bool _isForge;
        public bool IsForge
        {
            get => _isForge; set
            {
                _isForge = value;
                OnPropertyChanged();
            }
        }

        private bool _isFabric;
        public bool IsFabric
        {
            get => _isFabric; set
            {
                _isFabric = value;
                OnPropertyChanged();
            }
        }

        private bool _isQuilt;
        public bool IsQuilt
        {
            get => _isQuilt; set
            {
                _isQuilt = value;
                OnPropertyChanged();
            }
        }

        private bool _isNeoForged;
        public bool IsNeoForged
        {
            get => _isNeoForged; set
            {
                _isNeoForged = value;
                OnPropertyChanged();
            }
        }


        #endregion Properties

        private void UpdateSelectedGameType(ClientType clientType)
        {
            switch (clientType)
            {
                case ClientType.Vanilla: IsVanilla = true; break;
                case ClientType.Forge: IsForge = true; break;
                case ClientType.Fabric: IsFabric = true; break;
                case ClientType.Quilt: IsQuilt = true; break;
                case ClientType.NeoForged: IsNeoForged = true; break;
            }
        }

        private RelayCommand _changeInstanceClientTypeCommand;
        public ICommand ChangeInstanceClientTypeCommand
        {
            get => RelayCommand.GetCommand(ref _changeInstanceClientTypeCommand, Model.ChangeGameType);
        }

        public InstanceFactoryViewModel(Action<InstanceClient> addToLibrary, ICommand closeModalMenu) : base()
        {
            IsCloseAfterCommandExecuted = true;
            Model = new InstanceFactoryModel();

            ActionCommandExecutedEvent += (obj) => 
            {
                addToLibrary(Model.CreateInstance());
                closeModalMenu.Execute(obj);
            };

            Model.GameTypeChanged += UpdateSelectedGameType;
            UpdateSelectedGameType(ClientType.Vanilla);
        }
    }
}
