using Lexplosion.Common.Models.ShowCaseMenu;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Tools;
using System;

namespace Lexplosion.Common.ViewModels.ShowCaseMenu
{
    public sealed class InstanceProfileViewModel : VMBase
    {
        private readonly Action<string, string, uint, byte> _doNotification = (header, message, time, type) => { };


        #region Properties


        public InstanceProfileModel Model { get; }


        private bool _isSavedProperties;
        public bool IsSavedProperties
        {
            get => _isSavedProperties; private set
            {
                _isSavedProperties = value;
                OnPropertyChanged();
            }
        }

        // Нужно для того, чтобы не сбивались radiobutton
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


        #region Commands


        /// <summary>
        /// Команда выполняющаяся при нажатии кнопки "Сохранить"
        /// </summary>
        public RelayCommand SaveDataCommand
        {
            get => new RelayCommand(obj =>
            {
                _doNotification(ResourceGetter.GetString("everythingIsFine"), ResourceGetter.GetString("settingsSaved"), 5, 0);
                Model.Save();
            });
        }

        /// <summary>
        /// Команда выполняющаяся при нажатии кнопки "Загрузить своё изображение"
        /// </summary>
        public RelayCommand UploadLogoCommand
        {
            get => new RelayCommand(obj =>
            {
                OpenDialogWindowForUploadLogo();
            });
        }

        /// <summary>
        /// Переключение типа модлоадера
        /// </summary>
        private RelayCommand _switchClientType;
        public RelayCommand SwitchClientType
        {
            get => _switchClientType ?? (_switchClientType = new RelayCommand(obj =>
            {
                if ((ClientType)obj != ClientType.Vanilla)
                {
                    Model.ChangeClientType(GameType.Modded, (GameExtension)obj);
                }
                else
                {
                    Model.ChangeClientType(GameType.Vanilla, GameExtension.Optifine);
                }
            }));
        }

        #endregion Commands


        #region Constructors


        public InstanceProfileViewModel(InstanceClient instanceClient, Action<string, string, uint, byte> doNotification = null)
        {
            _doNotification = doNotification ?? _doNotification;
            Model = new InstanceProfileModel(instanceClient);
            switch (Model.GetInstanceExtenstion())
            {
                case ClientType.Vanilla: IsVanilla = true; break;
                case ClientType.Forge: IsForge = true; break;
                case ClientType.Fabric: IsFabric = true; break;
                case ClientType.Quilt: IsQuilt = true; break;
            }
        }


        #endregion Constructors


        #region Public & Protected Methods


        public void OpenDialogWindowForUploadLogo()
        {
            using (var dialog = new System.Windows.Forms.OpenFileDialog())
            {
                dialog.Filter = "Image files|*.bmp;*.jpg;*.gif;*.png;*.tif|All files|*.*";

                // Process open file dialog box results
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Model.UploadLogo(dialog.FileName);
                    IsSavedProperties = true;
                }
            }
        }


        #endregion Public & Protected Methods
    }
}
