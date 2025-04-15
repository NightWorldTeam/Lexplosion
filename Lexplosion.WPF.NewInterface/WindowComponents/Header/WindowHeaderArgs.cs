using System;

namespace Lexplosion.WPF.NewInterface.WindowComponents.Header
{
    public interface IWindowHeaderArgs
    {
        public Action Close { get; }
        public Action Maximized { get; }
        public Action Minimized { get; }
    }

    public class WindowHeaderArgs : IWindowHeaderArgs
    {
        public Action Close { get; }
        public Action Maximized { get; }
        public Action Minimized { get; }

        public WindowHeaderArgs(Action close, Action maximized, Action minimazed)
        {
            Close = close;
            Maximized = maximized;
            Minimized = minimazed;
        }
    }
}
