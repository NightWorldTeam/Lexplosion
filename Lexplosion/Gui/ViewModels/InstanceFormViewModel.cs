using Lexplosion.Gui.Models;
using Lexplosion.Gui.Models.InstanceForm;
using Lexplosion.Logic.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Lexplosion.Gui.ViewModels
{
    public class InstanceFormViewModel : VMBase
    {
        private RelayCommand _upperBtnCommand;
        private RelayCommand _lowerBtnCommand;

        public InstanceFormModel Model { get; }

        public RelayCommand UpperBtnCommand
        {
            get => _upperBtnCommand ?? (_upperBtnCommand = new RelayCommand(obj =>
            {
                switch ((UpperButtonFunc)obj) 
                {
                    case UpperButtonFunc.Download:
                        if (!MainModel.AddedInstanceForms.Contains(MainModel.GetSpecificVM(Model.Instance.OutsideId)))
                            MainModel.AddedInstanceForms.Add(this);
                        Model.DownloadModel.DonwloadPrepare();
                        break;
                    case UpperButtonFunc.ProgressBar:
                        break;
                    case UpperButtonFunc.Update:
                        Model.DownloadModel.DonwloadPrepare();
                        break;
                    case UpperButtonFunc.Play:
                        Model.LaunchModel.LaunchInstance();
                        break;
                    case UpperButtonFunc.Close:
                        break;
                }
            }));
        }
        public RelayCommand LowerBtnCommand
        {
            get => _lowerBtnCommand ?? (_lowerBtnCommand = new RelayCommand(obj =>
            {
                switch((LowerButtonFunc)obj) 
                {
                    case LowerButtonFunc.AddToLibrary:
                        break;
                    case LowerButtonFunc.DeleteFromLibrary:
                        break;
                    case LowerButtonFunc.OpenFolder:
                        Model.OpenInstanceFolder();
                        break;
                    case LowerButtonFunc.CancelDownload:
                        break;
                }
            }));
        }

        public InstanceFormViewModel(InstanceFormModel model)
        {
            Model = model;
            //NavigationShowCaseCommand = new NavigationCommands<ShowCaseViewModel>(
            //    MainViewModel.NavigationStore, () => new ShowCaseViewModel());
        }
    }
}
