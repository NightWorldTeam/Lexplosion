using Lexplosion.Global;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Network.Services;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.GameExtensions;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.InstanceProfile.Settings
{
    public sealed class InstanceProfileConfigurationModel : ViewModelBase
    {
        /// <summary>
        /// Модель сборки.
        /// </summary>
        private readonly InstanceModelBase _instanceModelBase;
        /// <summary>
        /// Основная информация о сборке.
        /// </summary>
        private BaseInstanceData _instanceData;
        /// <summary>
        /// Последняя сохранённая версия информации о сборке.
        /// </summary>
        private BaseInstanceData _oldInstanceData;


        public event Action<ClientType> GameTypeChanged;


        #region Properties


        /// <summary>
        /// True если пользователь изменил информацию.
        /// </summary>
        public bool HasChanges { get => HasIntermediateChanged(); }

        public bool IsExternal { get; }


        #region Versions


        /// <summary>ы
        /// Список версий майнкрафта
        /// </summary>
        public MinecraftVersion[] GameVersions { get => IsShowSnapshots ? MainViewModel.AllGameVersions : MainViewModel.ReleaseGameVersions; }
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

                // Убираем пролаг при клике
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
                OnPropertyChanged(nameof(HasChanges));
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
                _instanceData.ModloaderVersion = value;
                OnPropertyChanged(nameof(HasChanges));
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
                _instanceData.OptifineVersion = _optifineVersion;
                OnPropertyChanged(nameof(HasChanges));
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
                _instanceData.OptifineVersion = value;
                OnPropertyChanged(nameof(HasChanges));
                OnPropertyChanged();
            }
        }


        #endregion OptimizationMods


        #region NWClient


        /// <summary>
        /// Modloader Manager (for fabric, forge...)
        /// </summary>
        private bool _isNWClientEnabled;
        public bool IsNWClientEnabled 
        { 
            get => _isNWClientEnabled; set 
            {
                _isNWClientEnabled = value;
                _instanceData.IsNwClient = value;
                OnPropertyChanged(nameof(HasChanges));
                OnPropertyChanged();
            }
        }

        public bool IsNWClientAvailable { get; private set; }


        public HashSet<string> NWClientSupportedVersions { get; private set; } = [];


        #endregion  NWClient


        #endregion Properties


        #region Constructors


        public InstanceProfileConfigurationModel(InstanceModelBase instanceModelBase)
        {
            IsExternal = 
                instanceModelBase.Source == InstanceSource.Modrinth ||
                instanceModelBase.Source == InstanceSource.Curseforge ||
                instanceModelBase.Source == InstanceSource.Nightworld; 
            _instanceModelBase = instanceModelBase;

            _instanceData = instanceModelBase.BaseData;
            _oldInstanceData = instanceModelBase.BaseData;

            IsShowSnapshots = _instanceData.GameVersion.Type == MinecraftVersion.VersionType.Snapshot;

            Version = _instanceData.GameVersion ?? GameVersions[0];

            ModloaderManager = new ModloaderManager(GameExtension.Forge, Version);

            ClientType = _instanceData.Modloader;
            LoadInstanceDefaultExtension(ClientType);

            Runtime.TaskRun(() =>
            {
                NWClientSupportedVersions = NetworkServicesManager.MinecraftInfo.GetNwClientGameVersions();
                OnPropertyChanged(nameof(NWClientSupportedVersions));

                IsNWClientAvailable = NWClientSupportedVersions.FirstOrDefault(verStr => verStr == Version.Id) != null;
                OnPropertyChanged(nameof(IsNWClientAvailable));

                IsNWClientEnabled = GlobalData.GeneralSettings.NwClientByDefault == true;
                OnPropertyChanged(nameof(IsNWClientEnabled));
            });
        }


        private void LoadInstanceDefaultExtension(ClientType type)
        {
            GameTypeChanged?.Invoke(type);
            if (ClientType == ClientType.Vanilla)
            {
                IsOptifine = !string.IsNullOrEmpty(_oldInstanceData.OptifineVersion);
                UpdateOptimizationModManager(Version, _oldInstanceData.OptifineVersion);
            }

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

        #endregion Constructors


        #region Public Methods


        public void ChangeGameType(object parameter)
        {
            if (parameter == null)
            {
                _instanceData.Modloader = default(ClientType);
            }

            if (parameter is ClientType)
            {
                _instanceData.Modloader = (ClientType)parameter;

                ClientType = _instanceData.Modloader;

                if (_instanceData.Modloader != ClientType.Vanilla)
                {
                    UpdateModloaderManager(_instanceData.Modloader, _instanceData.GameVersion);
                }
            }
        }

        /// <summary>
        /// Сохраняет промежуточные изменения.
        /// </summary>
        public void SaveData()
        {
            if (ClientType == ClientType.Vanilla)
            {
                ModloaderVersion = string.Empty;
            }
            else
            {
                OptifineVersion = IsOptifine ? OptifineVersion : string.Empty;
            }

            if (!IsOptifine)
            {
                _instanceData.OptifineVersion = null;
            }

            _instanceModelBase.ChangeOverviewParameters(_instanceData);
            
           /* _instanceData = _instanceModelBase.InstanceData;*/
            _oldInstanceData = _instanceModelBase.BaseData;
            OnPropertyChanged(nameof(HasChanges));
        }

        /// <summary>
        /// Отменяет промежуточные изменения изменения.
        /// </summary>
        public void ResetChanges()
        {
            _instanceData = _instanceModelBase.BaseData;
            IsShowSnapshots = _instanceData.GameVersion.Type == MinecraftVersion.VersionType.Snapshot;
            Version = _instanceData.GameVersion ?? GameVersions[0];
            ClientType = _instanceData.Modloader;
            IsNWClientEnabled = _instanceData.IsNwClient;
            _isOptifine = _instanceData.OptifineVersion != null;
            OnPropertyChanged(nameof(_isOptifine));
            LoadInstanceDefaultExtension(ClientType);
            OnPropertyChanged(nameof(HasChanges));
        }


        #endregion Public Methods


        #region Private Methods


        /// <summary>
        /// Вызывается при изменении версии игры. Изменяет данные версии игры в BaseInstanceData<br/>
        /// Вызывает метод OnPropertyChanged() для свойства HasChanges, вызывает метод UpdateModloaderManager.
        /// </summary>
        private void VersionChanged()
        {
            _instanceData.GameVersion = Version;
            OnPropertyChanged(nameof(HasChanges));
            UpdateModloaderManager(_instanceData.Modloader, Version);
            UpdateOptimizationModManager(Version);

            IsNWClientAvailable = NWClientSupportedVersions.FirstOrDefault(verStr => verStr == Version.Id) != null;
            OnPropertyChanged(nameof(IsNWClientAvailable));
        }

        /// <summary>
        /// Появились ли промежуточные изменения.
        /// </summary>
        /// <returns>True/False</returns>
        private bool HasIntermediateChanged()
        {
            if (!_oldInstanceData.GameVersion.Equals(Version))
                return true;
            if (!_oldInstanceData.Modloader.Equals(ClientType))
                return true;
            if (ModloaderVersion != null && !_oldInstanceData.ModloaderVersion.Equals(ModloaderVersion) && ClientType != ClientType.Vanilla)
                return true;
            if (_oldInstanceData.OptifineVersion != (IsOptifine ? OptifineVersion : null))
                return true;
            if (_oldInstanceData.IsNwClient != IsNWClientEnabled)
                return true;
            
            return false;
        }

        /// <summary>
        /// Изменяет выбранный модлоадер в зависимости его типа и версии игры.<br/>
        /// Переводит ClientType в GameExtension вызывает метод UpdateModloaderManager(GameExtension, MinecraftVersion);
        /// </summary>
        /// <param name="type">Тип модлоадера (игнорирует ClientType.Vanilla)</param>
        /// <param name="version">Версия игры</param>
        private void UpdateModloaderManager(ClientType type, MinecraftVersion version, string modloaderVersion = null)
        {
            if (type == ClientType.Vanilla)
                return;

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
            if (type == GameExtension.Optifine)
                return;

            // (пере)Создаём менеджер
            ModloaderManager = new ModloaderManager(type, version);

            if (ExtensionManagerBase.IsExtensionLoaded(type, version))
            {
                if (ModloaderManager.CurrentMinecraftExtension.IsAvaliable && string.IsNullOrEmpty(modloaderVersion))
                {
                    ModloaderVersion = ModloaderManager.CurrentMinecraftExtension.Versions[0];
                }
                else if (!string.IsNullOrEmpty(modloaderVersion))
                {
                    ModloaderVersion = modloaderVersion;
                }
            }

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
            OnPropertyChanged(nameof(HasChanges));
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

            if (ExtensionManagerBase.IsExtensionLoaded(GameExtension.Optifine, minecraftVersion)) 
            {
                if (OptimizationModManager.CurrentMinecraftExtension.IsAvaliable && string.IsNullOrEmpty(optifimizationModVersion))
                {
                    OptifineVersion = OptimizationModManager.CurrentMinecraftExtension.Versions[0];
                }
                else if (!string.IsNullOrEmpty(optifimizationModVersion))
                {
                    OptifineVersion = optifimizationModVersion;
                }
            }

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
            OnPropertyChanged(nameof(HasChanges));
        }


        #endregion Private Methods
    }
}
