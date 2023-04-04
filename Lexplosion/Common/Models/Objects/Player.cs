using Lexplosion.Common.ViewModels;
using Lexplosion.Logic.Management;
using System;

namespace Lexplosion.Common.Models.Objects
{

    public class PlayerClub : Player
    {
        public PlayerClub(string uuid, Action kickMethod, Action unkickMethod) : base(uuid, kickMethod, unkickMethod)
        {

        }

        public RelayCommand AccessChangeAction
        {
            get => new RelayCommand(obj =>
            {
                var action = AccessChange();
                if (action == UserAction.Unkick)
                    //TODO сделать перевод
                    MainViewModel.ShowToastMessage("Действие на игроком", "У игрока " + this.Nickname + " появилась возможность снова зайти на ваш сервер.");
            });
        }
    }
}
