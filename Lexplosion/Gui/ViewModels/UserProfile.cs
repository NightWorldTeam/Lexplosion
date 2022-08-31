using System;

namespace Lexplosion.Gui.ViewModels
{
    public sealed class UserProfile : VMBase
    {
        /// <summary>
        /// Данное свойство содержить информации - о том показан ли InfoBar.
        /// </summary>
        private static bool _isShowInfoBar;
        public bool IsShowInfoBar
        {
            get => _isShowInfoBar; set
            {
                _isShowInfoBar = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Данное свойство содержить информации - о том авторизован ли пользователь.
        /// </summary>
        private bool _isAuthorized;
        public bool IsAuthorized
        {
            get => _isAuthorized; set
            {
                _isAuthorized = value;
                IsShowInfoBar = value;
                OnPropertyChanged(nameof(IsAuthorized));
            }
        }

        /// <summary>
        /// Данное свойство содержить ник пользователя.
        /// </summary>
        private string _nickname;
        public string Nickname
        {
            get => _nickname; set
            {
                _nickname = value;
                OnPropertyChanged(nameof(Nickname));
            }
        }

        public RelayCommand ChangeStatusCommand
        {
            get => new RelayCommand(obj =>
            {
                if (obj == null)
                    return;

                ActivityStatus newStatus;

                Enum.TryParse((string)obj, out newStatus);
                Global.UserData.User.ChangeBaseStatus(newStatus);
            });
        }
    }
}
