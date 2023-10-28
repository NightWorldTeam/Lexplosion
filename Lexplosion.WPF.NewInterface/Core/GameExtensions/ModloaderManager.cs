using Lexplosion.Logic.Management;

namespace Lexplosion.WPF.NewInterface.Core.GameExtensions
{
    public sealed class ModloaderManager : ExtensionManagerBase
    {
        private readonly MinecraftVersion _minecraftVersion;
        private readonly GameExtension _gameExtension;


        #region Properties


        public bool IsForgeAvaliable => MinecraftExtension.CheckExistsOnVersion(_minecraftVersion, GameExtension.Forge) && IsExtensionLoaded(GameExtension.Forge, _minecraftVersion);

        public bool IsFabricAvaliable => MinecraftExtension.CheckExistsOnVersion(_minecraftVersion, GameExtension.Fabric) && IsExtensionLoaded(GameExtension.Fabric, _minecraftVersion);

        public bool IsQuiltAvaliable => MinecraftExtension.CheckExistsOnVersion(_minecraftVersion, GameExtension.Quilt) && IsExtensionLoaded(GameExtension.Quilt, _minecraftVersion);

        public bool IsCurrentAvaliable { get => MinecraftExtension.CheckExistsOnVersion(_minecraftVersion, _gameExtension) && IsExtensionLoaded(_gameExtension, _minecraftVersion); }


        #endregion Properties


        #region Constructors


        public ModloaderManager(GameExtension extension, MinecraftVersion gameVersion) : base(extension, gameVersion)
        {
            _minecraftVersion = gameVersion;
            _gameExtension = extension;
        }


        #endregion Constructors


        #region Public & Protected Methods


        protected override void OnPropertiesChanged()
        {
            OnPropertyChanged(nameof(IsForgeAvaliable));
            OnPropertyChanged(nameof(IsFabricAvaliable));
            OnPropertyChanged(nameof(IsQuiltAvaliable));
            OnPropertyChanged(nameof(IsCurrentAvaliable));
        }

        public void UpdateAllProperties()
        {
            OnPropertiesChanged();
        }

        #endregion Public & Protected Methods
    }
}
