using Lexplosion.Gui;
using Lexplosion.Gui.ViewModels;

namespace Lexplosion.Logic.Management
{
    public partial class Player : VMBase
    {
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
