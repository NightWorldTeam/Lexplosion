using Lexplosion.Logic.Management.Instances;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.Modal;
using Lexplosion.UI.WPF.Core.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.Modal
{
    public sealed class InstancesGroupEditModel : ObservableObject
    {
        public event Action<InstancesGroup> GroupSaved;

        private readonly InstancesGroup _groupInstance;
        private readonly ClientsManager _clientsManager;

        public FiltableObservableCollection AllInstancesViewSource { get; } = new();

        public string Name { get; set; }
        public string Summary { get; set; }
        public List<InstanceClient> SelectedInstances { get; } = [];

        private string _searchText;
        /// <summary>
        /// Текст поиска для сборки
        /// </summary>
        public string SearchText
        {
            get => _searchText; set
            {
                _searchText = value;
                OnFilterChanged();
            }
        }

        public InstancesGroupEditModel(InstancesGroup group, ClientsManager clientsManager)
        {
            _groupInstance = group;
            AllInstancesViewSource.Source = clientsManager.GetExistsGroups().First().Clients;

            Name = group.Name;
            Summary = group.Summary;
            SelectedInstances = new(group.Clients);

            _clientsManager = clientsManager;
        }

        public void SaveGroupChanges()
        {
            _groupInstance.Name = Name;
            _groupInstance.Summary = Summary;
            _groupInstance.SaveGroupInfo();

            _groupInstance.UpdateInstances(SelectedInstances);

            GroupSaved?.Invoke(_groupInstance);
        }

        /// <summary>
        /// Вызывает фильтрацию списка сброк при вызове метода.
        /// </summary>
        private void OnFilterChanged()
        {
            AllInstancesViewSource.Filter = (ic =>
            {
                return string.IsNullOrEmpty(SearchText)
                    ? true
                    : (ic as InstanceClient).Name.IndexOf(SearchText, System.StringComparison.InvariantCultureIgnoreCase) > -1;
            });
        }
    }

    public sealed class InstancesGroupEditViewModel : ActionModalViewModelBase
    {
        public InstancesGroupEditModel Model { get; }

        public InstancesGroupEditViewModel(InstancesGroup group, ClientsManager clientManager)
        {
            Model = new(group, clientManager);
            ActionCommandExecutedEvent += (obj) => Model.SaveGroupChanges();
            IsCloseAfterCommandExecuted = true;
        }
    }
}
