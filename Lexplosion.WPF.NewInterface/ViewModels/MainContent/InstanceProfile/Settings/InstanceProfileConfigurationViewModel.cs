using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.GameExtensions;
using Lexplosion.WPF.NewInterface.Models.InstanceModel;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.ViewModels.MainContent.InstanceProfile
{
    public sealed class InstanceProfileConfigurationModel : ViewModelBase 
    {
        private readonly InstanceModelBase _instanceModelBase;


        #region Properties


        /// <summary>
        /// Основная информация о сборке.
        /// </summary>
        public BaseInstanceData InstanceData { get; set; }
        public BaseInstanceData OldInstanceData { get; set; }


        /// <summary>
        /// True если пользователь изменил информацию.
        /// </summary>
        public bool HasChanges 
        {
            get => HasVersionChanged();
        }


        /// <summary>
        /// Список версий майнкрафта
        /// </summary>
        public string[] GameVersions 
        {
            get => IsShowSnapshots ? MainViewModel.AllGameVersions : MainViewModel.ReleaseGameVersions;
        }

        /// <summary>
        /// Список версий майнкрафта
        /// </summary>
        //public MinecraftVersion[] GameVersions1
        //{
        //    get => IsShowSnapshots ? MainViewModel.AllGameVersions1 : MainViewModel.ReleaseGameVersions1;
        //}

        /// <summary>
        /// Версия сборки
        /// </summary>
        private string _version;
        public string Version 
        {
            get => _version; set
            {
                _version = value;
                VersionChanged();
                OnPropertyChanged();
            }
        }

        private bool _isShowSnapshots;
        public bool IsShowSnapshots
        {
            get => _isShowSnapshots; set 
            {
                _isShowSnapshots = value;
                OnPropertyChanged();

                // Убираем пролаг при клике
                Lexplosion.Runtime.TaskRun(() => OnPropertyChanged(nameof(GameVersions)));
            }
        }



        #endregion Properties


        #region Constructors


        public InstanceProfileConfigurationModel(InstanceModelBase instanceModelBase)
        {
            _instanceModelBase = instanceModelBase;

            InstanceData = instanceModelBase.InstanceData;
            OldInstanceData = instanceModelBase.InstanceData;
            
            Version = InstanceData.GameVersion ?? GameVersions[0];
        }


        #endregion Constructors


        #region Public Methods

        #endregion Public Methods


        #region Private Methods


        private void VersionChanged() 
        {
            OnPropertyChanged(nameof(HasChanges));
        }


        private bool HasVersionChanged() 
        {
            return OldInstanceData.GameVersion != Version?.Replace("snapshot ", "").Replace("release ", "");
        }


        #endregion Private Methods
    }

    public sealed class InstanceProfileConfigurationViewModel : ViewModelBase
    {
        #region Properties


        public InstanceProfileConfigurationModel Model { get; }


        private bool _isVanilla = true;
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


        #endregion Properties


        #region Constructors


        public InstanceProfileConfigurationViewModel(InstanceModelBase instanceModelBase)
        {
            Model = new InstanceProfileConfigurationModel(instanceModelBase);

            //switch (Model.GetInstanceExtenstion())
            //{
            //    case ClientType.Vanilla: IsVanilla = true; break;
            //    case ClientType.Forge: IsForge = true; break;
            //    case ClientType.Fabric: IsFabric = true; break;
            //    case ClientType.Quilt: IsQuilt = true; break;
            //}
        }


        #endregion Constructors


        #region Commands


        private RelayCommand _saveChangesCommand;
        public ICommand SaveChangesCommand
        {
            get => RelayCommand.GetCommand(ref _saveChangesCommand, (obj) => 
            { 
                //Model.SaveData(); 
            });
        }

        private RelayCommand _rebootChangesCommand;
        public ICommand RebootChangesCommand
        {
            get => RelayCommand.GetCommand(ref _rebootChangesCommand, (obj) => { });
        }


        #endregion Commands
    }
}
