using Lexplosion.Logic.Management;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core.ViewModel;
using System;
using System.Windows.Input;
using static Lexplosion.Logic.Management.Player;

namespace Lexplosion.WPF.NewInterface.Core.Objects
{
    public class PlayerWrapper : ObservableObject
    {
        private readonly Player _player;
        private Action<Player> _unkickedAction = null;


        #region Properties


        public string UUID { get => _player.UUID; }

        public string Nickname
        {
            get => _player.Nickname; private set
            {
                _player.Nickname = value;
                OnPropertyChanged();
            }
        }

        public bool IsKicked
        {
            get => _player.IsKicked; private set
            {
                _player.IsKicked = value;
                OnPropertyChanged();
            }
        }

        public byte[] Skin
        {
            get => _player.Skin; private set
            {
                _player.Skin = value;
                OnPropertyChanged();
            }
        }


        #endregion Properties


        #region Constructors


        public PlayerWrapper(Player player)
        {
            _player = player;
        }

        public PlayerWrapper(string name)
        {
            _player = new Player("sdf"); 
            _player.Nickname = name;
        }


        #endregion Constructors


        #region Public Methods


        private RelayCommand _accessChangeAction;
        public ICommand AccessChangeAction
        {
            get => RelayCommand.GetCommand(ref _accessChangeAction, () =>
            {
                var action = AccessChange();
                if (action == UserAction.Unkick)
                {
                    // Notification
                }
            });
        }


        /// <summary>
        /// Вызывает метод Kick или Unkick взависимости от статуса пользователя.
        /// </summary>
        private UserAction AccessChange()
        {
            if (_player.IsKicked)
            {
                Unkick();
                return UserAction.Unkick;
            }
            else
            {
                Kick();
                return UserAction.Kick;
            }
        }


        /// <summary>
        /// Присваивает значение делегату. Поменять значение можно только единажды.
        /// </summary>
        /// <param name="action"></param>
        public void SetUnkickedAction(Action<Player> action)
        {
            if (_unkickedAction != null)
                return;

            _unkickedAction = action;
        }

        public override bool Equals(object obj)
        {
            PlayerWrapper wrapperObj = obj as PlayerWrapper;
            return wrapperObj?._player.Equals(_player) ?? false;
        }

        public override int GetHashCode()
        {
            return _player.GetHashCode();
        }


        #endregion Public Methods


        #region Private Methods


        /// <summary>
        /// Кикает пользователя
        /// </summary>
        private void Kick()
        {
            IsKicked = true;
            _player._kickMethod();
        }

        /// <summary>
        /// Снимает кик.
        /// </summary>
        private void Unkick()
        {
            _player._unkickMethod();
            _unkickedAction?.Invoke(_player);
            IsKicked = false;
        }


        #endregion Private Methods
    }
}
