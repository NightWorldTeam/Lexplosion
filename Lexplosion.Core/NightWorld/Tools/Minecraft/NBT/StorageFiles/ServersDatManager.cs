using System.Collections.Generic;
using System.IO;

namespace NightWorld.Tools.Minecraft.NBT.StorageFiles
{
    public class ServersDatManager
    {
        public struct ServerData
        {
            public string Name;
            public string Ip;
            public string ImageBase64;
            public bool? AcceptTextures;

            public ServerData(string name, string ip)
            {
                Name = name;
                Ip = ip;
                ImageBase64 = null;
                AcceptTextures = null;
            }

            public ServerData(string name, string ip, string imageBase64, bool? acceptTextures)
            {
                Name = name;
                Ip = ip;
                ImageBase64 = imageBase64;
                AcceptTextures = acceptTextures;
            }
        }

        private NbtCompound _data = null;
        private readonly List<ServerData> _servers = new List<ServerData>();
        private string _filePath = null;

        public string FilePath { set => _filePath = value; }

        public IEnumerable<ServerData> Servers { get { return _servers; } }

        public bool ContsainsServer(string name, string ip)
        {
            foreach (var server in _servers)
            {
                if (server.Name == name && server.Ip == ip)
                {
                    return true;
                }
            }

            return false;
        }

        public ServersDatManager()
        {
            _data = DefaultStruct();
        }

        public ServersDatManager(string filePath)
        {
            _filePath = filePath;
            try
            {
                if (!File.Exists(_filePath))
                {
                    _data = DefaultStruct();
                    return;
                }

                byte[] fileBytes = File.ReadAllBytes(_filePath);
                loadData(fileBytes);
            }
            catch
            {
                _data = DefaultStruct();
            }
        }

        public ServersDatManager(byte[] fileBytes)
        {
            loadData(fileBytes);
        }

        private void loadData(byte[] fileBytes)
        {
            NbtDocoder decoder = new NbtDocoder();
            INbtNode data0;
            try
            {
                data0 = decoder.Load(fileBytes);
            }
            catch
            {
                data0 = DefaultStruct();
            }

            if (!(data0 is NbtCompound)) data0 = DefaultStruct();

            NbtCompound data = (NbtCompound)data0;
            if (!data.Content.ContainsKey("servers") && !(data.Content["servers"] is NbtList)) data = DefaultStruct();

            NbtList list = (NbtList)data.Content["servers"];
            if (list.ListContentType != NbtTagType.Compound)
            {
                data = DefaultStruct();
                list = (NbtList)data.Content["servers"];
            }

            _data = data;

            foreach (INbtNode item0 in list)
            {
                if (!(item0 is NbtCompound)) continue;

                NbtCompound item = (NbtCompound)item0;
                if (!item.ContainsKey("ip") || !item.ContainsKey("name")) continue;
                if (!(item["ip"] is NbtString) || !(item["name"] is NbtString)) continue;

                string name = ((NbtString)item["name"]).Content;
                string ip = ((NbtString)item["ip"]).Content;
                string icon = null;
                bool? acceptTextures = null;

                if (item.ContainsKey("icon") && item["icon"] is NbtString iconBase64)
                {
                    icon = iconBase64.Content;
                }

                if (item.ContainsKey("acceptTextures") && item["acceptTextures"] is NbtByte flag)
                {
                    acceptTextures = flag.Content == 1;
                }

                ServerData serverData = new ServerData(name, ip, icon, acceptTextures);
                _servers.Add(serverData);
            }
        }

        private NbtCompound DefaultStruct()
        {
            return new NbtCompound()
            {
                new NbtList("servers", NbtTagType.Compound)
            };
        }

        public void AddServer(ServerData serverData)
        {
            _servers.Add(serverData);
            var serversList = (NbtList)_data["servers"];

            var server = new NbtCompound
            {
                new NbtString("ip", serverData.Ip),
                new NbtString("name", serverData.Name)
            };

            if (serverData.ImageBase64 != null)
            {
                server.Add(new NbtString("icon", serverData.ImageBase64));
            }

            if (serverData.AcceptTextures != null)
            {
                byte value = (byte)((serverData.AcceptTextures == true) ? 1 : 0);
                server.Add(new NbtByte("acceptTextures", value));
            }

            serversList.Add(server);
        }

        public byte[] CompileData()
        {
            NbtEncoder encoder = new NbtEncoder();
            return encoder.Encode(_data ?? DefaultStruct());
        }

        public bool SaveFile()
        {
            try
            {
                string dir = Path.GetDirectoryName(_filePath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.WriteAllBytes(_filePath, CompileData());
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
