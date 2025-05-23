using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Lexplosion.Core.Extensions;
using Lexplosion.Logic.FileSystem.Services;
using Lexplosion.Logic.Objects;

namespace Lexplosion.Logic.Management.Instances
{
    public class InstancesGroup : VMBase
    {
        public static readonly Guid AllInctancesGroupId = Guid.Empty;


        /// <summary>
        /// Состояния IsSelected изменилось.
        /// </summary>
        public event Action<bool> SelectedChanged; 


        private readonly IFileServicesContainer _fileServices;

        /// <summary>
        /// Id группы
        /// </summary>
        public Guid Id { get; private set; }
        /// <summary>
        /// Название группы
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Краткое описание группы
        /// </summary>
        public string Summary { get; private set; }
        /// <summary>
        /// Является ли группа, группой по умолчанию (All)
        /// </summary>
        public bool IsDefaultGroup { get; }
        /// <summary>
        /// Группа выбрана
        /// </summary>
        private bool _isSelected;
        public bool IsSelected 
        {
            get => _isSelected; set 
            {
                _isSelected = value;

                if (_isSelected != value) 
                {
                    SelectedChanged?.Invoke(_isSelected);
                }
            }
        }
        /// <summary>
        /// Список сборок
        /// </summary>
        private ObservableCollection<InstanceClient> _clients = new();

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
            _fileServices = fileServices;

            foreach (var clientId in group.InstancesIds)
            {
                if (clients.TryGetValue(clientId, out InstanceClient client))
                    _clients.Add(client);
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
            IsDefaultGroup = true;
            _fileServices = fileServices;
            _clients = new(clients);
        }

        public void ChangeInstancePosition(InstanceClient client, int newIndex)
        {
            int index = _clients.FindIndex((InstanceClient clnt) => clnt == client);
            if (index < 0) return;

            _clients.RemoveAt(index);
            _clients.Insert(index, client);
        }

        public void AddInstance(InstanceClient client, int index)
        {
            if (index > _clients.Count) index = _clients.Count - 1;
            _clients.Insert(index, client);
        }

        public void AddInstance(InstanceClient client) => _clients.Add(client);

        public void RemoveInstance(InstanceClient client)
        {
            int index = _clients.FindIndex((InstanceClient clnt) => clnt == client);
            if (index < 0) return;
            _clients.RemoveAt(index);
        }

        public void SaveGroupInfo()
        {
            _fileServices.DataFilesService.SaveGroupInfo(BuildInstalledInstanceGroup());
        }

        public void AddIfNotExists(IEnumerable<InstanceClient> clients)
        {
            var existsClients = _clients.ToHashSet();
            foreach (InstanceClient client in clients)
            {
                if (existsClients.Contains(client)) continue;
                _clients.Add(client);
            }
        }

        internal InstalledInstancesGroup BuildInstalledInstanceGroup()
        {
            return new InstalledInstancesGroup()
            {
                Name = Name,
                Id = Id,
                InstancesIds = _clients.Select(x => x.LocalId).ToList(),
            };
        }
    }
}
