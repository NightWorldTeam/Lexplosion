using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core.Modal;
using Lexplosion.WPF.NewInterface.Core.ViewModel;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Args;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal
{
    public sealed class InstanceForServer 
    {
        public InstanceModelBase InstanceModel { get; }
        public bool IsSelected { get; set; }
        public bool IsAutoConnect { get; set; }

        public InstanceForServer(InstanceModelBase instanceModel)
        {
            InstanceModel = instanceModel;
            IsAutoConnect = !string.IsNullOrEmpty(instanceModel.Settings.AutoLoginServer);
        }

        public void AddServer(MinecraftServerInstance server, bool isAutoLogin) 
        {
            InstanceModel.AddServer(server, isAutoLogin);
        }
    }

    public class SelectInstanceForServerModel : ObservableObject 
    {
        private readonly MinecraftServerInstance _server;
        private readonly Func<InstanceClient, InstanceModelBase> _prepareLibraryAndGetInstanceModelBase;
		private readonly ClientsManager _clientsManager = Runtime.ClientsManager;


		private ObservableCollection<InstanceForServer> _availableInstances;
        public IEnumerable<InstanceForServer> AvailableInstances { get => _availableInstances; }

        public SelectInstanceForServerModel(MinecraftServerInstance server, SelectInstanceForServerArgs selectInstanceForServerArgs)
        {
            _server = server;
            _prepareLibraryAndGetInstanceModelBase = selectInstanceForServerArgs.PrepareLibraryAndGetInstanceModelBase;

            _availableInstances = new(
                selectInstanceForServerArgs.GetLibraryInstances()
                    .Where(i => i.GameVersion.ToString() == server.GameVersion)
                    .Select(i => new InstanceForServer(i))
                );
        }

        /// <summary>
        /// Создать новую сборку для сервера.
        /// </summary>
        public void AddNewInstance() 
        {
            var newInstance = _clientsManager.CreateClient(_server, false);
            var instanceModelBase = _prepareLibraryAndGetInstanceModelBase(newInstance);

            var instanceForServer = new InstanceForServer(instanceModelBase);
            instanceForServer.IsAutoConnect = true;
            instanceForServer.IsSelected = true;
            _availableInstances.Insert(0, instanceForServer);
        }

        /// <summary>
        /// Подтвердить выбор
        /// </summary>
        public void Apply() 
        {
            foreach (var instance in AvailableInstances)
            {
                if (!instance.IsSelected)
                {
                    continue;
                }

                instance.AddServer(_server, instance.IsAutoConnect);
            }
        }
    }

    public sealed class SelectInstanceForServerViewModel : ModalViewModelBase
    {
        public SelectInstanceForServerModel Model { get; }


        #region Commands


        private RelayCommand _addNewInstanceCommand;
        public ICommand AddNewInstanceCommand
        {
            get => RelayCommand.GetCommand(ref _addNewInstanceCommand, () => { Model.AddNewInstance(); CloseCommand.Execute(null); });
        }

        private RelayCommand _applyCommand;
        public ICommand ApplyCommand
        {
            get => RelayCommand.GetCommand(ref _applyCommand, () => { Model.Apply(); CloseCommand.Execute(null); }, (obj) => Model.AvailableInstances.FirstOrDefault(i => i.IsSelected) != null);
        }


        #endregion Commands


        public SelectInstanceForServerViewModel(MinecraftServerInstance server, SelectInstanceForServerArgs selectInstanceForServerArgs)
        {
            Model = new(server, selectInstanceForServerArgs);
        }
    }
}
