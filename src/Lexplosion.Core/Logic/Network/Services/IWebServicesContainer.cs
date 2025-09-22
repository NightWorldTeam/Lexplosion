using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lexplosion.Logic.Network.Services;
using Lexplosion.Logic.Network;

namespace Lexplosion.Logic.Network.Services
{
	public interface IWebServicesContainer
	{
		public ToServer WebService { get; }
		public MinecraftInfoService MinecraftService { get; }
	}
}
