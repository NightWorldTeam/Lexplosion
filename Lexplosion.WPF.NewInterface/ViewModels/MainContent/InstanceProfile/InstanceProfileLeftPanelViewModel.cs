using Lexplosion.WPF.NewInterface.Core.Tools;
using Lexplosion.WPF.NewInterface.Models.InstanceModel;
using Lexplosion.WPF.NewInterface.ViewModels.MainContent.MainMenu;
using System.Windows.Media;

namespace Lexplosion.WPF.NewInterface.ViewModels.MainContent.InstanceProfile
{
    public class InstanceProfileLeftPanelViewModel : LeftPanelViewModel
    {
        #region Properties


        private InstanceModelBase _instanceModel;

        public ImageBrush InstanceImage
        {
            get => new ImageBrush(ImageTools.ToImage(_instanceModel.Logo));
        }

        public string InstanceName { get => _instanceModel.Name; }
        public string InstanceVersion { get => _instanceModel.InstanceData.GameVersion.Id; }
        public string InstanceModloader { get => _instanceModel.InstanceData.Modloader.ToString(); }
        public string PlayerPlayedTime { get => "10ч"; }


        #endregion Properties


        #region Contructors


        public InstanceProfileLeftPanelViewModel(InstanceModelBase instanceModelBase)
        {
            _instanceModel = instanceModelBase;
            _instanceModel.NameChanged += OnNameChanged;
            _instanceModel.GameVersionChanged += OnVersionChanged;
            _instanceModel.ModloaderChanged += OnModloaderChanged;
        }


        #endregion Constructors


        #region Private Methods


        private void OnNameChanged() 
        {
            OnPropertyChanged(nameof(InstanceName));
        }

        private void OnVersionChanged() 
        {
            OnPropertyChanged(nameof(InstanceVersion));
        }

        private void OnModloaderChanged() 
        {
            OnPropertyChanged(nameof(InstanceModloader));
        }


        #endregion Private Methods
    }
}
