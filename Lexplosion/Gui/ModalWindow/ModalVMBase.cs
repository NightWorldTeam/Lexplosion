using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Gui.ModalWindow
{
    public abstract class ModalVMBase : VMBase, IModalContent
    {
        public virtual RelayCommand CloseModalWindow { get; }
        public virtual RelayCommand Action { get; }
    }
}
