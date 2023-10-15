using Lexplosion.Logic.Management;
using System.Windows.Media.Animation;

namespace Lexplosion.WPF.NewInterface.Core.GameExtensions
{
    public sealed class OptimizationModManager : ExtensionManagerBase
    {
        private readonly MinecraftVersion _minecraftVersion;
        private readonly GameExtension _gameExtension;

        public bool IsOptifineAvaliable { get => MinecraftExtension.CheckExistsOnVersion(_minecraftVersion, GameExtension.Optifine) && IsExtensionLoaded(GameExtension.Optifine, _minecraftVersion); }
        // public bool IsSodium { get => CheckGameExtensionAvaliable(GameExtension.Sodium); }
        // public bool IsOptiFabric { get => CheckGameExtensionAvaliable(GameExtension.IsOptiFabric); }
        public bool IsAvaliable { get => MinecraftExtension.CheckExistsOnVersion(_minecraftVersion, _gameExtension) && IsExtensionLoaded(_gameExtension, _minecraftVersion); }

        public OptimizationModManager(MinecraftVersion gameVersion, GameExtension extension = GameExtension.Optifine, bool isEnable = true) : base(extension, gameVersion, isEnable)
        {
            _minecraftVersion = gameVersion;
            _gameExtension = extension;
        }

        protected override void OnPropertiesChanged()
        {
            OnPropertyChanged(nameof(IsOptifineAvaliable));
            OnPropertyChanged(nameof(IsAvaliable));
        }
    }
}
