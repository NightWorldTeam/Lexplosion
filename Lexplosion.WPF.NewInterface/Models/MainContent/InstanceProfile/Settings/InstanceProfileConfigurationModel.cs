using Lexplosion.Logic.Management;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Models.InstanceModel;
using Lexplosion.WPF.NewInterface.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.WPF.NewInterface.Models.MainContent.InstanceProfile.Settings
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
        public MinecraftVersion[] GameVersions
        {
            get => IsShowSnapshots ? MainViewModel.AllGameVersions : MainViewModel.ReleaseGameVersions;
        }

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
                });
            }
        }



        #endregion Properties


        #region Constructors


        public InstanceProfileConfigurationModel(InstanceModelBase instanceModelBase)
        {
            _instanceModelBase = instanceModelBase;

            InstanceData = instanceModelBase.InstanceData;
            OldInstanceData = instanceModelBase.InstanceData;

            IsShowSnapshots = InstanceData.GameVersion.Type == MinecraftVersion.VersionType.Snapshot;

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
            return !OldInstanceData.GameVersion.Equals(Version);
        }


        #endregion Private Methods
    }

}
