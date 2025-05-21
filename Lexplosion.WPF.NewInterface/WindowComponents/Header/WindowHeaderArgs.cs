using System;

namespace Lexplosion.WPF.NewInterface.WindowComponents.Header
{
    public interface IWindowHeaderArgs
    {
        public Action Close { get; }
        public Action Maximized { get; }
        public Action Minimized { get; }
        /// <summary>
        /// Название Template'a например MacOS, WindowsOS
        /// </summary>
        public string TemplateName { get; set; }
        /// <summary>
        /// Показывать кнопки дополнительного функционала
        /// </summary>
        public bool IsAdditionalButtonEnabled { get; set; }
    }

    public class WindowHeaderArgs : IWindowHeaderArgs
    {
        public Action Close { get; }
        public Action Maximized { get; }
        public Action Minimized { get; }
        public string TemplateName { get; set; }
        public bool IsAdditionalButtonEnabled { get; set; }

        public WindowHeaderArgs(string templateName, Action close, Action maximized, Action minimazed, bool isAdditionalButtonEnabled = true)
        {
            TemplateName = templateName;
            Close = close;
            Maximized = maximized;
            Minimized = minimazed;
            IsAdditionalButtonEnabled = isAdditionalButtonEnabled;
        }
    }
}
