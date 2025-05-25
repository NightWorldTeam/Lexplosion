using Lexplosion.Global;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Management;
using Lexplosion.WPF.NewInterface.Core.GameExtensions;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.Modal
{
    public sealed class InstanceFactoryModel : ViewModelBase
    {
        public event Action<ClientType> GameTypeChanged;
        private readonly ClientsManager _clientsManager = Runtime.ClientsManager;


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


        #region NWClient


        /// <summary>
        /// Modloader Manager (for fabric, forge...)
        /// </summary>
        public bool IsNWClientEnabled { get; set; }

        public bool IsNWClientAvailable { get; private set; }


        public HashSet<string> NWClientSupportedVersions { get; private set; } = [];


        #endregion  NWClient


        public IReadOnlyCollection<InstancesGroup> InstancesGroups { get; }
        public InstancesGroup SelectedGroup { get; set; }


        #endregion Properties


        #region Constructors


        public InstanceFactoryModel(IReadOnlyCollection<InstancesGroup> instancesGroups, InstancesGroup defaultGroup)
        {
            InstancesGroups = instancesGroups;
            SelectedGroup = defaultGroup;

            if (GameVersions.Length == 0)
            {
                return;
            }

            ModloaderManager = new ModloaderManager(GameExtension.Forge, Version);
            Version = GameVersions[0];
            ClientType = ClientType.Vanilla;

            Runtime.TaskRun(() =>
            {
                NWClientSupportedVersions = Runtime.ServicesContainer.MinecraftService.GetNwClientGameVersions();
                OnPropertyChanged(nameof(NWClientSupportedVersions));

                IsNWClientAvailable = NWClientSupportedVersions.FirstOrDefault(verStr => verStr == Version.Id) != null;
                OnPropertyChanged(nameof(IsNWClientAvailable));

                IsNWClientEnabled = GlobalData.GeneralSettings.NwClientByDefault == true;
                OnPropertyChanged(nameof(IsNWClientEnabled));
            });
            InstancesGroups = instancesGroups;
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
            var instanceName = InstanceName?.Trim();
            if (string.IsNullOrWhiteSpace(instanceName)) instanceName = $"{Version.ToString()} {ClientType}";

            var newInstance = _clientsManager.CreateClient(
                instanceName,
                InstanceSource.Local,
                Version,
                ClientType,
                IsNWClientAvailable ? IsNWClientEnabled : false,
                null,
                ModloaderVersion,
                ClientType == ClientType.Vanilla && IsOptifine ? OptifineVersion : null,
                false
                );

            if (!SelectedGroup.IsDefaultGroup) 
            {
                SelectedGroup.AddInstance(newInstance);
                SelectedGroup.SaveGroupInfo();
            }

            return newInstance;
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

            IsNWClientAvailable = NWClientSupportedVersions.FirstOrDefault(verStr => verStr == Version.Id) != null;
            OnPropertyChanged(nameof(IsNWClientAvailable));
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
}
