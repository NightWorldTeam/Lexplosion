using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Logic.Network.Services
{
    internal static class NetworkServicesManager
    {
        public static readonly MinecraftInfoService MinecraftInfo = new();
    }
}
