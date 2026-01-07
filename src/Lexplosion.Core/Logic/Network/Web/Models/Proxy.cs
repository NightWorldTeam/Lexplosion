using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Logic.Network.Web.Models
{
    public readonly struct Proxy
    {
        public string IP { get; }
        public int Port { get; }
        public string Url { get => $"http://{IP}:{Port}"; }
    }
}
