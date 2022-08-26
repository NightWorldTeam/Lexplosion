using Lexplosion.Gui.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Gui.ModalWindow
{
    public abstract class ModalVMBase : VMBase, IModal
    {
        protected const double DefaultWidth = 360;
        protected const double DefaultHeight = 420;

        public virtual RelayCommand CloseModalWindow { get; }
        public virtual RelayCommand Action { get; }

        public virtual double Width => DefaultWidth;

        public virtual double Height => DefaultHeight;
    }
}
