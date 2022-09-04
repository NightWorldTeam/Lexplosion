using Lexplosion.Gui;

namespace Lexplosion.Logic.Management
{
    public partial class Player : VMBase
    {   
        public RelayCommand AccessChangeAction
        {
            get => new RelayCommand(obj =>
            {
                AccessChange();
            });
        }    
    }
}
