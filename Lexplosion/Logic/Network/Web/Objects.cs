using System.Collections.Generic;
using Lexplosion.Logic.Objects.CommonClientData;

namespace Lexplosion.Logic.Network
{
    /// <summary>
    /// Этот класс нужен для декодирования json в методе GetVersionManifest в классах ToServer и NightWorldApi
    /// </summary>
    class DataVersionManifest : VersionManifest
    {
        public class DataLibInfo : LibInfo
        {
            public List<string> os;
        }

        public string code;
        public string str;
        public new Dictionary<string, DataLibInfo> libraries;
    }
}
