using Lexplosion.Common.ModalWindow;
using Lexplosion.Common.Models;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Lexplosion.Common.ViewModels.ModalVMs
{
    public sealed class InstanceForServer 
    {
        public InstanceFormViewModel InstanceFormViewModel { get; }
        public bool IsSelected { get; set; }
        public bool IsAutoConnect { get; set; }

        public InstanceForServer(InstanceFormViewModel instanceFormViewModel)
        {
            InstanceFormViewModel = instanceFormViewModel;
            IsAutoConnect = !string.IsNullOrEmpty(instanceFormViewModel.Client.GetSettings().AutoLoginServer);
        }
    }

    public sealed class SelectMenuInstanceForServerModel
    {
        private readonly MinecraftServerInstance _minecraftServerInstance;

        private ObservableCollection<InstanceForServer> _availableInstances;
        public IEnumerable<InstanceForServer> AvailableInstances { get => _availableInstances; }

        public SelectMenuInstanceForServerModel(MinecraftServerInstance minecraftServerInstance)
        {

            _minecraftServerInstance = minecraftServerInstance;

            _availableInstances = new ObservableCollection<InstanceForServer>(
                MainModel.Instance.LibraryController
                    .GetInstances(ic => ic.GameVersion.ToString() == _minecraftServerInstance.GameVersion)
                    .Select(i => new InstanceForServer(i))
                );
        }

        public void AddNewInstance() 
        {
            var ic = InstanceClient.CreateClient(_minecraftServerInstance, false);
            var ifvm = MainModel.Instance.CreateInstanceForm(ic);
            MainModel.Instance.AddInstanceForm(ifvm);

            var ifs = new InstanceForServer(ifvm);
            ifs.IsAutoConnect = true;
            ifs.IsSelected = true;
            _availableInstances.Insert(0, ifs);
        }

        public void Apply() 
        {
            foreach (var instance in AvailableInstances) 
            {
                if (!instance.IsSelected) 
                {
                    continue;
                }

                instance.InstanceFormViewModel.Client.AddGameServer(_minecraftServerInstance, instance.IsAutoConnect);
            }
        }
    }

    public sealed class SelectMenuInstanceForServerViewModel : ModalVMBase
    {
        public SelectMenuInstanceForServerModel Model { get; }

        public double Width => 620;
        public double Height => 400;

        private RelayCommand _addNewInstanceCommand;
        public ICommand AddNewInstanceCommand 
        {
            get => _addNewInstanceCommand ?? (_addNewInstanceCommand = new RelayCommand(obj => 
            {
                Model.AddNewInstance();
            }));
        }

        private RelayCommand _applyInstancesCommand;
        public override RelayCommand ActionCommand
        {
            get => _applyInstancesCommand ?? (_applyInstancesCommand = new RelayCommand(obj => 
            {
                Model.Apply();
                CloseModalWindowCommand?.Execute(null);
            }));
        }

        private RelayCommand _closeModalWindowCommand;
        public override RelayCommand CloseModalWindowCommand
        {
            get => _closeModalWindowCommand ?? (_closeModalWindowCommand = new RelayCommand(obj =>
            {
                ModalWindowViewModelSingleton.Instance.Close();
            }));
        }

        public SelectMenuInstanceForServerViewModel(MinecraftServerInstance minecraftServerInstance)
        {
            Model = new SelectMenuInstanceForServerModel(minecraftServerInstance);
        }
    }
}
