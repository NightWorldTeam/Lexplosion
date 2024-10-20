using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Core.ViewModel;
using Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceTransfer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceControllers
{
    public sealed class InstanceSharesController : ObservableObject
    {
        public event Action<string> ShareStopped;
        public event Action ActiveSharesListChanged;


        private readonly ObservableCollection<DistributedInstance> _activeShares = [];
        private readonly ObservableCollection<InstanceDistribution> _availableInstanceDistribution = new();

        public IReadOnlyCollection<DistributedInstance> ActiveShares { get => _activeShares; }
        public IReadOnlyCollection<InstanceDistribution> AvailableInstanceDistribution { get => _availableInstanceDistribution; }



        public InstanceSharesController()
        {

        }


        public void AddActiveShare(DistributedInstance distributedInstance)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                _activeShares.Add(distributedInstance);
                ActiveSharesListChanged?.Invoke();
            });
        }

        public void AddFileReceiver(InstanceDistribution instanceDistribution) 
        {
            _availableInstanceDistribution.Add(instanceDistribution);
        }

        public void RemoveFileReceiver(InstanceDistribution instanceDistribution)
        {
            _availableInstanceDistribution.Remove(instanceDistribution);
        }

        public bool Contains(string id) 
        {
            return _availableInstanceDistribution.FirstOrDefault(i => i.Id == id) != null;
        }

        public void RemoveAllShares() 
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                _activeShares.Clear();
                ActiveSharesListChanged?.Invoke();
            });
        }

        public void RemoveActiveShare(DistributedInstance distributedInstance)
        {
            App.Current.Dispatcher.Invoke(() => 
            {
                distributedInstance.Stop();
                _activeShares.Remove(distributedInstance);
                ShareStopped?.Invoke(distributedInstance.Id);
                ActiveSharesListChanged?.Invoke();
            });
        }
    }
}
