using Lexplosion.Controls;
using Lexplosion.Logic.Management;
using Lexplosion.Tools;
using System;
using static Lexplosion.Logic.Management.Player;

namespace Lexplosion.Common.Models.Objects
{
    public sealed class PlayerWrapper : VMBase, INotifiable
    {
        private readonly Player _player;
        private Action<Player> _unkickedAction = null;


        private DoNotificationCallback _doNotification;
        public DoNotificationCallback DoNotification
        {
            get => _doNotification; private set
            {
                _doNotification = value ?? ((header, message, time, type) => { });
            }
        }


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


        public PlayerWrapper(Player player, DoNotificationCallback doNotification = null)
        {
            Runtime.DebugWrite("Create wrapper " + player.Nickname + " " + player.GetHashCode());
            DoNotification = doNotification ?? DoNotification;
            _player = player;
        }


        #endregion Constructors


        #region Public Methods


        public RelayCommand AccessChangeAction
        {
            get => new RelayCommand(obj =>
            {
                var action = AccessChange();
                if (action == UserAction.Unkick)
                {
                    DoNotification(ResourceGetter.GetString("actionOnThePlayer"), String.Format(ResourceGetter.GetString("playerCanJoinToServerAgain"), Nickname), 5, 0);
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
