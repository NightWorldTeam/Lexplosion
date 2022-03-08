using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Logic.Network
{
    class DataClient : NetworkClient
    {
        const string clientType = "data-client"; // эта строка нужна при подключении к управляющему серверу
        string UUID = "";

        public DataClient(string uuid, string server) : base(clientType, server)
        {
            UUID = uuid;
        }
    }
}
