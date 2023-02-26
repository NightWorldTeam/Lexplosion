using Lexplosion.Global;
using Lexplosion.Gui.Commands;
using Lexplosion.Gui.ViewModels.MainMenu;
using Lexplosion.Logic.Network;
using System;
using System.Windows.Input;
using Lexplosion.Tools;
using Lexplosion.Logic.Management.Authentication;
using Lexplosion.Gui.Models;

namespace Lexplosion.Gui.ViewModels
{
    public class AuthViewModel : VMBase
    {
        public AuthModel Model { get; }

        private bool _isManualInput;
        public bool IsManualInput 
        { 
            get => _isManualInput; set
            { 
                _isManualInput = value; 
                OnPropertyChanged(); 
            } 
        }

        #region Commands


        /// <summary>
        /// Команда навигации меняет viewmodel.
        /// </summary>
        public readonly ICommand NavigationCommand;

        private RelayCommand _signUpCommand;
        /// <summary>
        /// Вызывается при клике по кнопке [Войти]
        /// </summary>
        public RelayCommand SignUpCommand
        {
            get => _signUpCommand ?? (new RelayCommand(obj =>
            {
                Model.SignIn();
            }));
        }

        private RelayCommand _singUpMicrosoftCommand;
        /// <summary>
        /// Редиректит на сайт авторизации майков.
        /// </summary>
        public RelayCommand SingUpMicrosoftCommand
        {
            get => _singUpMicrosoftCommand ?? (_singUpMicrosoftCommand = new RelayCommand(obj =>
            {
                Model.FollowToMicrosoft();
            }));
        }

        private RelayCommand _cancelMicrosoftAuthCommand;
        public RelayCommand CancelMicrosoftAuthCommand
        {
            get => _cancelMicrosoftAuthCommand ?? (_cancelMicrosoftAuthCommand = new RelayCommand(obj =>
            {
                Model.CancelMicrosoftAuth();
                IsManualInput = false;
            }));
        }

        private RelayCommand _manualInputMicrosoftDataFormOpenCommand;
        public RelayCommand ManualInputMicrosoftDataFormOpenCommand
        {
            get => _manualInputMicrosoftDataFormOpenCommand ?? (_manualInputMicrosoftDataFormOpenCommand = new RelayCommand(obj => 
            {
                IsManualInput = true;
            }));
        }

        private RelayCommand _manualInputMicrosoftDataCommand;
        public RelayCommand ManualInputMicrosoftDataCommand
        {
            get => _manualInputMicrosoftDataCommand ?? (_manualInputMicrosoftDataCommand = new RelayCommand(obj =>
            {
                Model.MicrosoftManualInput();
                IsManualInput = false;
            }));
        }


        #endregion Commands


        #region Constructors


        public AuthViewModel(MainViewModel viewModel)
        {
            NavigationCommand = new NavigateCommand<MainMenuViewModel>(
                MainViewModel.NavigationStore, () => viewModel.MainMenuVM ?? viewModel.InitMainMenuViewModel(new MainMenuViewModel(viewModel)));

            Model = new AuthModel(viewModel, ref NavigationCommand);
        }


        #endregion Constructors
    }
}
