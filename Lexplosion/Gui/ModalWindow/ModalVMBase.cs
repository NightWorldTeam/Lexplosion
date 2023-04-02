namespace Lexplosion.Gui.ModalWindow
{
    public abstract class ModalVMBase : VMBase, IModal
    {
        protected const double DefaultWidth = 360;
        protected const double DefaultHeight = 420;

        public virtual RelayCommand CloseModalWindowCommand { get; }
        public virtual RelayCommand ActionCommand { get; }
        public virtual RelayCommand HideModalWindowCommand { get; }

        public virtual double Width => DefaultWidth;

        public virtual double Height => DefaultHeight;
    }
}
