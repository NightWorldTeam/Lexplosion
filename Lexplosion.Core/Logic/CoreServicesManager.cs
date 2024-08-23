using Lexplosion.Logic.Network.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Logic
{
    public class CoreServicesManager
    {
        public static MinecraftInfoService MinecraftInfo { get => NetworkServicesManager.MinecraftInfo; }
    }
}
