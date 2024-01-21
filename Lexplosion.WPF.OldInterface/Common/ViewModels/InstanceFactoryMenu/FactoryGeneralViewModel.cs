using Lexplosion.Common.ModalWindow;
using Lexplosion.Common.Models.InstanceFactory;
using Lexplosion.Common.ViewModels.ModalVMs;

namespace Lexplosion.Common.ViewModels.FactoryMenu
{
    public sealed class FactoryGeneralViewModel : ModalVMBase
    {
        #region Properties


        public InstanceFactoryModel Model { get; }

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

        /// <summary>
        /// Создание самого модпака начинается здесь.
        /// </summary>
        private RelayCommand _actionCommand;
        public override RelayCommand ActionCommand
        {
            get => _actionCommand ?? (_actionCommand = new RelayCommand(obj =>
            {
                Model.CreateInstance();
                CloseModalWindowCommand.Execute(null);
            }));
        }

        /// <summary>
        /// Закрытие модального окна;
        /// </summary>
        private RelayCommand _closeModalWindowCommand;
        public override RelayCommand CloseModalWindowCommand
        {
            get => _closeModalWindowCommand ?? (_closeModalWindowCommand = new RelayCommand(obj =>
            {
                ModalWindowViewModelSingleton.Instance.Close();
            }));
        }

        private RelayCommand _logoImportCommand;
        public RelayCommand LogoImportCommand
        {
            get => _logoImportCommand ?? (_logoImportCommand = new RelayCommand(obj =>
            {

            }));
        }

        #endregion Commands


        #region Constructors


        public FactoryGeneralViewModel(MainViewModel mainViewModel)
        {
            Model = new InstanceFactoryModel(mainViewModel, ChangeSelectedClientType);
        }


        #endregion Constructors


        #region Private Methods


        private void ChangeSelectedClientType(ClientType clientType)
        {
            switch (clientType)
            {
                case ClientType.Vanilla: IsVanilla = true; break;
                case ClientType.Forge: IsForge = true; break;
                case ClientType.Fabric: IsFabric = true; break;
                case ClientType.Quilt: IsQuilt = true; break;
            }
        }


        #endregion Private Methods
    }
}
