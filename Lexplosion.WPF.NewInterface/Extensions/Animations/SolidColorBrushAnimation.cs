using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Lexplosion.WPF.NewInterface.Extensions
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
