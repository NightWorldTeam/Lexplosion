using Lexplosion.Logic.FileSystem.Services;
using Lexplosion.Logic.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Lexplosion.Logic.Management.Instances
{
    public class InstancesGroup : VMBase
    {
        public static readonly Guid AllInctancesGroupId = Guid.Empty;

        private readonly IFileServicesContainer _fileServices;

        /// <summary>
        /// Список сборок
        /// </summary>
        private ObservableCollection<InstanceClient> _clients = new();
        private Dictionary<InstanceClient, int> _clientsKeys = new();

        public byte[] Logo { get; private set; }

        /// <summary>
        /// Состояния IsSelected изменилось.
        /// </summary>
        public event Action<bool> SelectedChanged;
        /// <summary>
        /// Добавленная новая сборка
        /// </summary>
        public event Action<InstanceClient> NewInstanceAdded;

        /// <summary>
        /// Id группы
        /// </summary>
        public Guid Id { get; private set; }
        /// <summary>
        /// Название группы
        /// </summary>
        private string _name;
        public string Name
        {
            get => _name; set
            {
                _name = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Краткое описание группы
        /// </summary>
        private string _summary;
        public string Summary
        {
            get => _summary; set
            {
                _summary = value;
                HasSummary = !string.IsNullOrEmpty(_summary);
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Является ли группа, группой по умолчанию (All)
        /// </summary>
        public bool IsDefaultGroup { get => Id == AllInctancesGroupId; }
        /// <summary>
        /// Группа выбрана
        /// </summary>
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected; set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                SelectedChanged?.Invoke(_isSelected);
            }
        }

        private bool _hasSummary;
        public bool HasSummary
        {
            get => _hasSummary; set
            {
                _hasSummary = value;
                OnPropertyChanged();
            }
        }

        public IReadOnlyCollection<InstanceClient> Clients
        {
            get => _clients;
        }

        /// <summary>
        /// Конструктор создающий существующую группу
        /// </summary>
        /// <param name="group">Информация о группе</param>
        /// <param name="clients">Список со всеми установленными сборками. Ключ - внутренний id, значение - сам инстанс клиент</param>
        /// <param name="fileServices">сервисы</param>
        internal InstancesGroup(InstalledInstancesGroup group, IDictionary<string, InstanceClient> clients, IFileServicesContainer fileServices)
        {
            Id = group.Id;
            Name = group.Name;
            Summary = group.Summary;
            HasSummary = !string.IsNullOrEmpty(group.Summary);
            _fileServices = fileServices;

            foreach (var clientId in group.InstancesIds)
            {
                if (clients.TryGetValue(clientId, out InstanceClient client))
                    AddClient(client);
            }
        }

        /// <summary>
        /// Конструктор создающий пустую группу
        /// </summary>
        /// <param name="name">Имя группы</param>
        /// <param name="fileServices">Сервисы</param>
        internal InstancesGroup(string name, string summary, IFileServicesContainer fileServices)
        {
            Id = Guid.NewGuid();
            Name = name;
            Summary = summary;
            _fileServices = fileServices;
        }

        /// <summary>
        /// Контруктор, создающий группу All
        /// </summary>
        /// <param name="clients">Список всех установленных сборок</param>
        /// <param name="fileServices">сервисы</param>
        internal InstancesGroup(IEnumerable<InstanceClient> clients, IFileServicesContainer fileServices)
        {
            Id = AllInctancesGroupId;
            Name = "All";
            Summary = "AllSummary";
            _fileServices = fileServices;
            _clients = new(clients);

            int i = 0;
            foreach (var item in _clients)
            {
                _clientsKeys[item] = i;
                i++;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void AddClient(InstanceClient client)
        {
            if (_clientsKeys.ContainsKey(client)) return;

            _clients.Add(client);
            _clientsKeys[client] = _clients.Count - 1;
            NewInstanceAdded?.Invoke(client);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ChangeInstancePosition(InstanceClient client, int newIndex)
        {
            if (!_clientsKeys.TryGetValue(client, out int currentIndex)) return;

            if (currentIndex < newIndex)
            {
                if (newIndex >= _clientsKeys.Count) newIndex = _clientsKeys.Count - 1;
                for (int i = currentIndex; i < newIndex; i++)
                {
                    _clients[i] = _clients[i + 1];
                    _clientsKeys[_clients[i]] = i;
                }
            }
            else
            {
                for (int i = newIndex; i < currentIndex; i++)
                {
                    int ii = i + 1;
                    _clients[ii] = _clients[i];
                    _clientsKeys[_clients[ii]] = ii;
                }
            }

            _clients[newIndex] = client;
            _clientsKeys[client] = newIndex;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddInstance(InstanceClient client) => AddClient(client);

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void RemoveInstance(InstanceClient client)
        {
            if (!_clientsKeys.TryGetValue(client, out int currentIndex)) return;

            int endIndex = _clientsKeys.Count - 1;
            for (int i = currentIndex; i < endIndex; i++)
            {
                _clients[i] = _clients[i + 1];
                _clientsKeys[_clients[i]] = i;
            }

            _clients.RemoveAt(endIndex);
            _clientsKeys.Remove(client);
        }

        public void SaveGroupInfo()
        {
            _fileServices.DataFilesService.SaveGroupInfo(BuildInstalledInstanceGroup());
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddIfNotExists(IEnumerable<InstanceClient> clients)
        {
            foreach (InstanceClient client in clients)
            {
                if (_clientsKeys.ContainsKey(client)) continue;
                AddClient(client);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddIfNotExists(InstanceClient client)
        {
            if (_clientsKeys.ContainsKey(client)) return;
            AddClient(client);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool InstanceContains(InstanceClient client) => _clients.Contains(client);

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void UpdateInstances(IEnumerable<InstanceClient> clients)
        {
            if (_clients.SequenceEqual(clients))
            {
                return;
            }

            var removedItems = _clients.Except(clients).ToList();

            foreach (var removedItem in removedItems)
            {
                RemoveInstance(removedItem);
            }

            var newItems = clients.Except(_clients);

            foreach (var newItem in newItems)
            {
                AddClient(newItem);
            }
        }

        public void UpdateLogo(string imagePath)
        {

        }

        internal InstalledInstancesGroup BuildInstalledInstanceGroup()
        {
            return new InstalledInstancesGroup()
            {
                Name = Name,
                Summary = Summary,
                Id = Id,
                InstancesIds = _clients.Select(x => x.LocalId).ToList(),
            };
        }
    }
}
