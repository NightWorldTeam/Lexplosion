using System;
using System.Collections.Generic;
using System.Linq;
using Lexplosion.Logic.FileSystem.Services;
using Lexplosion.Logic.Objects;

namespace Lexplosion.Logic.Management.Instances
{
	public class InstancesGroup
	{
		private List<InstanceClient> _clients = new();
		private readonly IFileServicesContainer _fileServices;
		public Guid Id { get; private set; }
		public string Name { get; private set; }

		public static readonly Guid AllInctancesGroupId = Guid.Empty;

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
		internal InstancesGroup(string name, IFileServicesContainer fileServices)
		{
			Id = Guid.NewGuid();
			Name = name;
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
			_fileServices = fileServices;
			_clients = new List<InstanceClient>(clients);
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
