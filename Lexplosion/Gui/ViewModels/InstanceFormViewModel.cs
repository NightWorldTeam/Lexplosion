using Lexplosion.Gui.Models;
using Lexplosion.Gui.Models.InstanceForm;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Management.Instances;
using System;

namespace Lexplosion.Gui.ViewModels
{
    public class InstanceFormViewModel : VMBase
    {
        private RelayCommand _upperBtnCommand;
        private RelayCommand _lowerBtnCommand;

        private InstanceClient _instanceClient;

        public InstanceFormModel Model { get; }

        public RelayCommand UpperBtnCommand
        {
            get => _upperBtnCommand ?? (_upperBtnCommand = new RelayCommand(obj =>
            {
                switch ((UpperButtonFunc)obj) 
                {
                    case UpperButtonFunc.Download:
                        if (!MainModel.LibraryInstances.ContainsKey(_instanceClient))
                            MainModel.LibraryInstances.Add(_instanceClient, this);
                        Model.DownloadModel.DonwloadPrepare();
                        break;
                    case UpperButtonFunc.ProgressBar:
                        // TODO: может сделать, что-то типо меню скачивания??
                        break;
                    case UpperButtonFunc.Play:
                        Model.LaunchModel.LaunchInstance();
                        break;
                    case UpperButtonFunc.Close:
                        LaunchGame.GameStop();
                        Model.UpperButton.ChangeFuncPlay();
                        break;
                }
            }));
        }
        public RelayCommand LowerBtnCommand
        {
            get => _lowerBtnCommand ?? (_lowerBtnCommand = new RelayCommand(obj =>
            {
                Console.WriteLine(((LowerButtonFunc)obj).ToString());
                switch ((LowerButtonFunc)obj)
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
                    case LowerButtonFunc.Update:
                        Model.DownloadModel.DonwloadPrepare();
                        break;
                    case LowerButtonFunc.OpenWebsite:
                            try
                            {
                                System.Diagnostics.Process.Start(_instanceClient.WebsiteUrl);
                            }
                            catch
                            {
                                // message box here.
                            }
                        break;
                    case LowerButtonFunc.RemoveInstance:
                        break;
                    case LowerButtonFunc.Export:
                        break;
                }
            }));
        }

        public InstanceFormViewModel(InstanceClient instanceClient)
        {
            Model = new InstanceFormModel(instanceClient);
            _instanceClient = instanceClient;
        }
    }
}
