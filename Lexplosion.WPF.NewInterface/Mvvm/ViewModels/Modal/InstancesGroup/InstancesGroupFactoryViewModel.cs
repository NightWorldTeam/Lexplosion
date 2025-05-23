using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Modal;
using Lexplosion.WPF.NewInterface.Core.ViewModel;
using System;
using System.Collections.Generic;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal
{
    public sealed class InstancesGroupFactoryModel : ObservableObject 
    {
        public event Action<InstancesGroup> GroupCreated;

        private readonly ClientsManager _clientsManager;

        public FiltableObservableCollection AllInstancesViewSource { get; } = new();


        private string _name;
        public string Name
        {
            get => _name; set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

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

        public InstancesGroupFactoryModel(ClientsManager clientsManager, IReadOnlyCollection<InstanceClient> allInstances)
        {
            AllInstancesViewSource.Source = allInstances;
            _clientsManager = clientsManager;
        }

        public void CreateGroup() 
        {
            var newGroup = _clientsManager.CreateGroup(Name, Summary);
            GroupCreated?.Invoke(newGroup);
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

    public sealed class InstancesGroupFactoryViewModel : ActionModalViewModelBase
    {
        public InstancesGroupFactoryModel Model { get; }

        public InstancesGroupFactoryViewModel(ClientsManager clientManager, IReadOnlyCollection<InstanceClient> allInstances)
        {
            Model = new InstancesGroupFactoryModel(clientManager, allInstances);
            ActionCommandExecutedEvent += (obj) => Model.CreateGroup();
            IsCloseAfterCommandExecuted = true;
        }
    }
}
