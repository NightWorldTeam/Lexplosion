using Lexplosion.Logic.Objects;
using System.Collections.Generic;

namespace Lexplosion.Logic.Network
{
    /// <summary>
    /// Этот класс нужен для декодирования json в методе GetVersionManifest в классах ToServer и NightWorldApi
    /// </summary>
    public class DataVersionManifest : VersionManifest
    {
        public string code;
        public string str;
        public new Dictionary<string, DataLibInfo> libraries;
    }
}
