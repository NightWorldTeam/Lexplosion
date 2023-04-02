using System.Collections;
using System.Collections.Generic;

namespace Lexplosion.Gui.Models.GameExtensions
{
    interface IExtensionModel
    {
        public string GameVersion { get; }
        public GameExtension GameExtension { get; }
        public bool IsAvaliable { get; }
        public string Version { get; set; }
        public IEnumerable<string> Versions { get; }
    }
}
