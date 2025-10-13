using Lexplosion.Logic.Management;
using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core.ViewModel;
using System;
using System.Windows.Input;
using static Lexplosion.Logic.Management.Player;

namespace Lexplosion.UI.WPF.Core.Objects
{
    public class PlayerWrapper : ObservableObject
    {
        private Action<Player> _unkickedAction;

        public Player Player { get; }


        #region Properties


        public string UUID { get => Player.UUID; }

        public bool IsKicked
        {
            get => Player.IsKicked; private set
            {
                Player.IsKicked = value;
                OnPropertyChanged();
            }
        }


        #endregion Properties


        #region Constructors


        public PlayerWrapper(Player player)
        {
            Player = player;
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
            if (Player.IsKicked)
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
            return wrapperObj?.Player.Nickname.Equals(Player.Nickname) ?? false;
        }

        public override int GetHashCode()
        {
            return Player.GetHashCode();
        }


        #endregion Public Methods


        #region Private Methods


        /// <summary>
        /// Кикает пользователя
        /// </summary>
        private void Kick()
        {
            IsKicked = true;
            Player._kickMethod();
        }

        /// <summary>
        /// Снимает кик.
        /// </summary>
        private void Unkick()
        {
            Player._unkickMethod();

#if DEBUG
            if (_unkickedAction == null)
            {
                Runtime.DebugWrite("Unkicked Action is null");
            }
#endif
            _unkickedAction?.Invoke(Player);
            IsKicked = false;
        }


        #endregion Private Methods
    }
}
