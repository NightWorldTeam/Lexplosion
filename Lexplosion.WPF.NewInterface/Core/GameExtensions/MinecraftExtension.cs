using Lexplosion.Logic.Management;
using System.Collections.ObjectModel;
using System.Linq;

namespace Lexplosion.WPF.NewInterface.Core.GameExtensions
{
    /// <summary>
    /// Расшерение для майнкрафта. 
    /// Содержит версию игры, список версий расширения.
    /// </summary>
    public sealed class MinecraftExtension
    {
        #region Properties


        /// <summary>
        /// Версии расширения
        /// </summary>
        public ReadOnlyCollection<string> Versions { get; }
        /// <summary>
        /// Версия игры.
        /// </summary>
        public MinecraftVersion Version { get; }
        /// <summary>
        /// Тип расшерения.
        /// </summary>
        public GameExtension Type { get; }
        /// <summary>
        /// Наличие хотя-бы одной версии расширения.
        /// </summary>
        public bool IsAvaliable { get => Versions?.Count > 0; }


        #endregion Properties


        #region Constructors


        public MinecraftExtension(ReadOnlyCollection<string> versions, GameExtension gameExtension)
        {
            Versions = versions;
            Type = gameExtension;
        }


        #endregion Constructors


        #region Public Methods


        /// <summary>
        /// Поверяет существуют ли версии модлоадера на данную версию игры.<br/>
        /// Например минимальная версия игры с которой существует Quilt -> 1.14.4, раньше версий на него не сущевуствует.
        /// </summary>
        /// <param name="minecraftVersion">Версия майнкрафта</param>
        /// <param name="extension">Тип расшерения</param>
        /// <returns></returns>
        public static bool CheckExistsOnVersion(MinecraftVersion minecraftVersion, GameExtension extension)
        {
            if (minecraftVersion?.Id == null)
            {
                return false;
            }

            if (minecraftVersion?.Type == MinecraftVersion.VersionType.Snapshot)
                return true;

            ushort[] version = minecraftVersion.Id.Split('.').Select(ushort.Parse).ToArray<ushort>();

            switch (extension)
            {
                case GameExtension.Forge: return version[0] >= 1 && version[1] >= 1;
                case GameExtension.Fabric: return version[0] >= 1 && version[1] >= 13;
                case GameExtension.Quilt: return version[0] >= 1 && version[1] > 14 || (version[1] == 14 && version[2] >= 4);
                case GameExtension.Optifine: return version[0] >= 1 && version[1] > 7 || (version[1] == 7 && version[2] >= 2);
                default: return false;
            }
        }


        #endregion Public Methods
    }
}