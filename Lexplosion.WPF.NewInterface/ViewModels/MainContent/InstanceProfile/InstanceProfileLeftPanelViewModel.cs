using Lexplosion.WPF.NewInterface.ViewModels.MainContent.MainMenu;
using System.Windows.Media;

namespace Lexplosion.WPF.NewInterface.ViewModels.MainContent.InstanceProfile
{
    public class InstanceProfileLeftPanelViewModel : LeftPanelViewModel
    {
        private ImageBrush _instanceImage;
        public ImageBrush InstanceImage 
        {
            get => _instanceImage; 
            set => RaiseAndSetIfChanged(ref _instanceImage, value); 
        }

        public string InstanceVersion { get; set; }
        public string InstanceName { get; set; }
        public string InstanceModloader { get; set; }
    }
}
