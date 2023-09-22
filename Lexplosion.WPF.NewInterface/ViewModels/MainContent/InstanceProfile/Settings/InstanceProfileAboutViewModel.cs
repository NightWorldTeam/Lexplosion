using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Tools;
using Lexplosion.WPF.NewInterface.Models.InstanceModel;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Lexplosion.WPF.NewInterface.ViewModels.MainContent.InstanceProfile
{
    public class InstanceProfileAboutModel : ViewModelBase 
    {
        private readonly InstanceModelBase _instanceModel;

        private string _logoPath;

        private BaseInstanceData InstanceData { get; }
        private BaseInstanceData OldInstanceData { get; set; }


        #region Properties


        public bool HasChanges 
        {
            get => OldInstanceData.Name != Name || OldInstanceData.Summary != Summary || OldInstanceData.Description != Description;
        }


        private BitmapImage _logoBytes;
        public BitmapImage LogoBytes 
        {
            get => _logoBytes; set 
            {
                _logoBytes = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasChanges));
            }
        }

        public string Name 
        {
            get => InstanceData.Name; set 
            {
                InstanceData.Name = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasChanges));
            }
        }

        public string Summary 
        {
            get => InstanceData.Summary; set 
            {
                InstanceData.Summary = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasChanges));
            }
        }

        public string Description 
        {
            get => InstanceData.Description; set 
            {
                InstanceData.Description = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasChanges));
            }
        }


        #endregion Properties


        #region Contructors


        public InstanceProfileAboutModel(InstanceModelBase instanceModel)
        {
            _instanceModel = instanceModel;
            InstanceData = instanceModel.InstanceData;
            OldInstanceData = instanceModel.InstanceData;
            LogoBytes = ImageTools.ToImage(instanceModel.Logo);
            Name = InstanceData.Name;
            Summary = InstanceData.Summary;
            Description = InstanceData.Description;
        }


        #endregion Constructors


        #region Public Methods

        public void SetLogoPath(string path) 
        {
            _logoPath = path;
        }

        public void SaveChanges() 
        {
            _instanceModel.ChangeOverviewParameters(InstanceData, _logoPath);
            OldInstanceData = _instanceModel.InstanceData;
            OnPropertyChanged(nameof(HasChanges));
        }


        #endregion Public Methods
    }

    public sealed class InstanceProfileAboutViewModel : ViewModelBase
    {
        public InstanceProfileAboutModel Model { get; }


        #region Commands


        private RelayCommand _rebootChangesCommand;
        public ICommand RebootChangesCommand 
        {
            get => RelayCommand.GetCommand(ref _rebootChangesCommand, (obj) => { });
        }

        private RelayCommand _saveChangesCommand;
        public ICommand SaveChangesCommand
        {
            get => RelayCommand.GetCommand(ref _saveChangesCommand, (obj) => { Model.SaveChanges(); });
        }

        private RelayCommand _setLogoPathCommand;
        public ICommand SetLogoPathCommand 
        {
            get => RelayCommand.GetCommand<string>(ref _setLogoPathCommand, Model.SetLogoPath);
        }


        #endregion Commands


        public InstanceProfileAboutViewModel(InstanceModelBase instanceModel)
        {
            Model = new InstanceProfileAboutModel(instanceModel);
        }
    }
}
