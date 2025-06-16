using Lexplosion.Logic.Management;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.GameExtensions;
using Lexplosion.WPF.NewInterface.Core.ViewModel;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal;
using System;

namespace Lexplosion.WPF.NewInterface.Mvvm.Model.Modal
{
    public class InstanceCopyModel : ObservableObject
    {
        public event Action<ClientType> GameTypeChanged;

        private readonly AppCore _appCore;
        private readonly ClientsManager _clientsManager;
        private readonly InstanceModelBase _instanceModelBase;
        private readonly BaseInstanceData _instanceData;


        #region Properties


        private bool _isCopyWithoutChanges = true;
        public bool IsCopyWithoutChanges
        {
            get => _isCopyWithoutChanges; set
            {
                _isCopyWithoutChanges = value;
                OnPropertyChanged();
            }
        }

        public bool HasModloaderByDefault { get; }

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

        private ClientType _clientType;
        public ClientType ClientType
        {
            get => _clientType; set
            {
                _clientType = value;
                OnPropertyChanged();
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


        #endregion Properties


        #region Constructors


        public InstanceCopyModel(AppCore appCore, ClientsManager clientsManager, InstanceModelBase instanceModelBase)
        {
            _appCore = appCore;
            _clientsManager = clientsManager;
            _instanceModelBase = instanceModelBase;

            Version = instanceModelBase.GameVersion;

            _instanceData = instanceModelBase.BaseData;

            ClientType = _instanceData.Modloader;
            HasModloaderByDefault = ClientType != ClientType.Vanilla;
            LoadInstanceDefaultExtension(ClientType);
            ModloaderVersion = _instanceData.ModloaderVersion;
        }


        #endregion Constructors


        #region Public Methods


        public InstanceClient Copy()
        {
            var instanceClient = _instanceModelBase.InstanceClient;
            if (IsCopyWithoutChanges)
            {
                return _clientsManager.CopyClient(instanceClient);
            }
            else 
            {
                return _clientsManager.CopyClient(instanceClient, Version, ClientType, ModloaderVersion, (uncopiedAddons) => 
                {
                    _appCore.ModalNavigationStore.Open(new InstanceCopyErrorsViewModel(_appCore, instanceClient.Name, uncopiedAddons));
                });
            }
        }


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


        #endregion  Public Methods


        #region Private Methods


        private void LoadInstanceDefaultExtension(ClientType type)
        {
            GameTypeChanged?.Invoke(type);

            if (_instanceData.Modloader == ClientType.Forge)
            {
                UpdateModloaderManager(GameExtension.Forge, Version, _instanceData.ModloaderVersion);
            }
            else if (_instanceData.Modloader == ClientType.Fabric)
            {
                UpdateModloaderManager(GameExtension.Fabric, Version, _instanceData.ModloaderVersion);
            }
            else if (_instanceData.Modloader == ClientType.Quilt)
            {
                UpdateModloaderManager(GameExtension.Quilt, Version, _instanceData.ModloaderVersion);
            }
            else if (_instanceData.Modloader == ClientType.NeoForge)
            {
                UpdateModloaderManager(GameExtension.Neoforge, Version, _instanceData.ModloaderVersion);
            }
        }

        /// <summary>
        /// Вызывается при изменении версии игры. Изменяет данные версии игры в BaseInstanceData<br/>
        /// Вызывает метод OnPropertyChanged() для свойства HasChanges, вызывает метод UpdateModloaderManager.
        /// </summary>
        private void VersionChanged()
        {
            UpdateModloaderManager(ClientType, Version);
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
        /// <param name="type">Тип модлоадера (игнорирует GameExtension.Optifine, GameExtension.NWClient)</param>
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


        #endregion Private Methods
    }
}