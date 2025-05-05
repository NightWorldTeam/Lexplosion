using Lexplosion.Logic.FileSystem.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lexplosion.Logic.Management.Instances
{
	public class InstanceGroup
	{
		private List<InstanceClient> _clients;
		private readonly IFileServicesContainer _fileServices;
		private readonly Guid _id;
		private readonly string _name;

		public IReadOnlyCollection<InstanceClient> Clients
		{
			get => _clients;
		}

		internal InstanceGroup(Guid id, string name, List<InstanceClient> clients, IFileServicesContainer fileServices)
		{
			_id = id;
			_name = name;
			_clients = clients;
			_fileServices = fileServices;
		}

		public void ChangePosition(InstanceClient client, int newIndex)
		{
			int index = _clients.FindIndex((InstanceClient clnt) => clnt == client);
			if (index < 0) return;

			_clients.RemoveAt(index);
			_clients.Insert(index, client);
		}

		public void SaveGroupInfo()
		{
			_fileServices.DataFilesService.SaveGroupInfo(new Objects.InstalledInstanceGroup()
			{
				Name = _name,
				Id = _id,
				InstancesIds = _clients.Select(x => x.LocalId).ToList(),
			});
		}



	}
}
