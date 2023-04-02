using Lexplosion.Gui;
using Lexplosion.Gui.ViewModels;
using System;

namespace Lexplosion.Logic.Management
{
    public partial class PlayerClub : Player
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
                    MainViewModel.ShowToastMessage("Действие на игроком", "У игрока " + this.Nickname + " появилась возможность снова зайти на ваш сервер.");
            });
        }
    }
}
