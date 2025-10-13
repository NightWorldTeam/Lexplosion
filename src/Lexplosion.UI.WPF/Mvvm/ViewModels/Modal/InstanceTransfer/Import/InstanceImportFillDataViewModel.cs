using Lexplosion.Logic.Management;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.GameExtensions;
using Lexplosion.UI.WPF.Core.Modal;
using Lexplosion.UI.WPF.Core.ViewModel;
using System;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.Modal.InstanceTransfer
{
    public sealed class InstanceImportFillDataModel : ObservableObject
    {
        public event Action<ClientType> GameTypeChanged;


        private readonly AppCore _appCore;


        #region Constructors


        public InstanceImportFillDataModel(AppCore appCore)
        {
            _appCore = appCore;

            ModloaderManager = new ModloaderManager(GameExtension.Forge, Version);
            Version = GameVersions[0];
            ClientType = ClientType.Vanilla;
        }


        #endregion Constructors


        #region Properties


        public bool AllFieldsFilled { get => !string.IsNullOrEmpty(InstanceName) && (ClientType == ClientType.Vanilla || (ClientType != ClientType.Vanilla && !string.IsNullOrEmpty(ModloaderVersion))); }


        private string _instanceName;
        public string InstanceName
        {
            get => _instanceName; set
            {
                _instanceName = value;
                OnPropertyChanged(nameof(AllFieldsFilled));
            }
        }



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
                OnPropertyChanged(nameof(AllFieldsFilled));
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
                OnPropertyChanged(nameof(AllFieldsFilled));

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
                OnPropertyChanged(nameof(AllFieldsFilled));
            }
        }
        /// <summary>
        /// Modloader Manager (for fabric, forge...)
        /// </summary>
        public ModloaderManager ModloaderManager { get; private set; }

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
                OnPropertyChanged(nameof(AllFieldsFilled));
            }
        }


        #endregion Modloader


        #endregion Properties


        #region Public Methods


        /// <summary>
        /// Собирает класс baseinstancedata, для дальнейшего импорта.
        /// </summary>
        /// <returns></returns>
        public BaseInstanceData BuildBaseInstanceData()
        {
            return new BaseInstanceData()
            {
                Name = InstanceName,
                GameVersion = Version,
                Modloader = ClientType,
                ModloaderVersion = ModloaderVersion ?? ""
            };
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


        #endregion Public Methods


        #region Private Methods


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


        #endregion Private Methods
    }

    public sealed class InstanceImportFillDataViewModel : ActionModalViewModelBase
    {
        public InstanceImportFillDataModel Model { get; }


        #region Properties


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


        #region Commands


        private RelayCommand _changeInstanceClientTypeCommand;
        public ICommand ChangeInstanceClientTypeCommand
        {
            get => RelayCommand.GetCommand(ref _changeInstanceClientTypeCommand, Model.ChangeGameType);
        }


        #endregion Commands


        #region Constructors


        public InstanceImportFillDataViewModel(AppCore appCore, Action<BaseInstanceData> apply, Action cancelImport)
        {
            Model = new InstanceImportFillDataModel(appCore);

            ActionCommandExecutedEvent += (obj) =>
            {
                apply(Model.BuildBaseInstanceData());
            };

            CloseCommandExecutedEvent += (cancel) =>
            {
                if (cancel != null && (bool)cancel)
                {
                    cancelImport.Invoke();
                }
            };

            Model.GameTypeChanged += UpdateSelectedGameType;
            UpdateSelectedGameType(ClientType.Vanilla);
        }


        #endregion Constructors


        private void UpdateSelectedGameType(ClientType clientType)
        {
            switch (clientType)
            {
                case ClientType.Vanilla: IsVanilla = true; break;
                case ClientType.Forge: IsForge = true; break;
                case ClientType.Fabric: IsFabric = true; break;
                case ClientType.Quilt: IsQuilt = true; break;
                case ClientType.NeoForge: IsNeoForged = true; break;
            }
        }
    }
}
