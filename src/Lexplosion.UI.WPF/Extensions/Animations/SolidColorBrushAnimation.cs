using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Lexplosion.UI.WPF.Extensions
{
    public class SolidColorBrushAnimation : ColorAnimation
    {
        public SolidColorBrush ToBrush
        {
            get { return To == null ? null : new SolidColorBrush(To.Value); }
            set { To = value?.Color; }
        }
    }
}
