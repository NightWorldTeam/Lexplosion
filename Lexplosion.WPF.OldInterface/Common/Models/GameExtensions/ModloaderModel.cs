using System;

namespace Lexplosion.Common.Models.GameExtensions
{
    public sealed class ModloaderModel : ExtensionModel
    {
        public ModloaderModel(GameExtension extension, string gameVersion, Action<bool> onAvailableChanged) : base(extension, gameVersion)
        {
            AvailiableChanged += onAvailableChanged;
        }
    }
}
