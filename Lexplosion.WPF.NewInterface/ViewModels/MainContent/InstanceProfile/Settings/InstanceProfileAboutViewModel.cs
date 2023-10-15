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

        private BaseInstanceData _instanceData;
        private BaseInstanceData _oldInstanceData;


        #region Properties


        public bool HasChanges 
        {
            get => OnHasChanges();
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
            get => _instanceData.Name; set 
            {
                _instanceData.Name = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasChanges));
            }
        }

        public string Summary 
        {
            get => _instanceData.Summary; set 
            {
                _instanceData.Summary = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasChanges));
            }
        }

        public string Description 
        {
            get => _instanceData.Description; set 
            {
                _instanceData.Description = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasChanges));
            }
        }


        #endregion Properties


        #region Contructors


        public InstanceProfileAboutModel(InstanceModelBase instanceModel)
        {
            _instanceModel = instanceModel;
            _instanceData = instanceModel.InstanceData;
            _oldInstanceData = instanceModel.InstanceData;
            LogoBytes = ImageTools.ToImage(instanceModel.Logo);
            Name = _instanceData.Name;
            Summary = _instanceData.Summary;
            Description = _instanceData.Description;
        }


        #endregion Constructors


        #region Public Methods


        public void SetLogoPath(string path) 
        {
            _logoPath = path;
        }

        /// <summary>
        /// Сохраняет промежуточные изменения.
        /// </summary>
        public void SaveData()
        {
            _instanceModel.ChangeOverviewParameters(_instanceData, _logoPath);
            _oldInstanceData = _instanceModel.InstanceData;
            OnPropertyChanged(nameof(HasChanges));
        }

        /// <summary>
        /// Отменяет промежуточные изменения изменения.
        /// </summary>
        public void ResetChanges()
        {
            _instanceData = _instanceModel.InstanceData;
            Name = _instanceData.Name;
            Summary = _instanceData.Summary;
            Description = _instanceData.Description;

            _oldInstanceData = _instanceModel.InstanceData;
            OnPropertyChanged(nameof(HasChanges));
        }


        #endregion Public Methods


        #region Private Methods


        public bool OnHasChanges() 
        {
            if (!_oldInstanceData.Name.Equals(Name))
                return true;
            if (!_oldInstanceData.Summary.Equals(Summary))
                return true;
            if (!_oldInstanceData.Description.Equals(Description))
                return true;
            return false;
        }


        #endregion Private Methods
    }

    public sealed class InstanceProfileAboutViewModel : ViewModelBase
    {
        public InstanceProfileAboutModel Model { get; }


        #region Commands


        private RelayCommand _rebootChangesCommand;
        public ICommand RebootChangesCommand 
        {
            get => RelayCommand.GetCommand(ref _rebootChangesCommand, (obj) => { Model.ResetChanges(); });
        }

        private RelayCommand _saveChangesCommand;
        public ICommand SaveChangesCommand
        {
            get => RelayCommand.GetCommand(ref _saveChangesCommand, (obj) => { Model.SaveData(); });
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
