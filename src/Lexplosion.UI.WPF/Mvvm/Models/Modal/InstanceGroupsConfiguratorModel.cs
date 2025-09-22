using Lexplosion.Logic.Management.Instances;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.ViewModel;
using Lexplosion.UI.WPF.Mvvm.Models.Mvvm.InstanceModel;
using System.Collections.Generic;
using System.Linq;

namespace Lexplosion.UI.WPF.Mvvm.Models.Modal
{
    public class InstanceGroupsConfiguratorModel : ObservableObject
    {
        private readonly IReadOnlyCollection<InstancesGroup> _allInstancesGroups;
        private readonly IEnumerable<InstancesGroup> _beforeChangesGroups;

        public InstanceModelBase InstanceModel { get; }

        public FiltableObservableCollection AllGroupsViewSource { get; } = new();

        public List<InstancesGroup> SelectedGroups { get; set; } = [];

        private string _searchText;
        public string SearchText
        {
            get => _searchText; set
            {
                _searchText = value;
                OnFilterChanged();
            }
        }

        private void OnFilterChanged()
        {
            AllGroupsViewSource.Filter = (group =>
            {
                return string.IsNullOrEmpty(SearchText)
                    ? true
                    : (group as InstancesGroup).Name.IndexOf(SearchText, System.StringComparison.InvariantCultureIgnoreCase) > -1;
            });
        }

        public InstanceGroupsConfiguratorModel(InstanceModelBase instanceModel, ClientsManager clientsManager)
        {
            InstanceModel = instanceModel;
            AllGroupsViewSource.Source = clientsManager.GetExistsGroups().Skip(1).ToList();
            _allInstancesGroups = clientsManager.GetExistsGroups();
            _beforeChangesGroups = _allInstancesGroups
                .Skip(1)
                .Where(group => group.Clients.Contains(instanceModel.InstanceClient));
            SelectedGroups = new(_beforeChangesGroups);
        }

        public void SaveChanges()
        {
            if (_beforeChangesGroups.SequenceEqual(SelectedGroups))
            {
                return;
            }

            var removedItems = _beforeChangesGroups.Except(SelectedGroups).ToList();

            foreach (var removedItem in removedItems)
            {
                removedItem.RemoveInstance(InstanceModel.InstanceClient);
                removedItem.SaveGroupInfo();
            }

            foreach (var group in SelectedGroups)
            {
                group.AddIfNotExists(InstanceModel.InstanceClient);
                group.SaveGroupInfo();
            }
        }
    }
}
