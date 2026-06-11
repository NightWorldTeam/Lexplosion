using Lexplosion.Logic.Management;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Lexplosion.UI.WPF.Core.GameExtensions
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
            if (string.IsNullOrEmpty(minecraftVersion?.Id)) return false;

            // Снапшоты обычно обрабатываются отдельно
            if (minecraftVersion.Type == MinecraftVersion.VersionType.Snapshot)
                return true;

            string current = minecraftVersion.Id;

            return extension switch
            {
                GameExtension.Forge => IsAtLeast(current, "1.1"),
                GameExtension.Fabric => IsAtLeast(current, "1.13"),
                GameExtension.Quilt => IsAtLeast(current, "1.14.4"),
                GameExtension.Optifine => IsAtLeast(current, "1.7.2"),
                GameExtension.Neoforge => IsAtLeast(current, "1.20.2"),
                _ => false
            };
        }

        /// <summary>
        /// Безопасно проверяет, что версия current >= target.
        /// Справляется с форматами "1.21", "1.7.10", "26.1.1" и т.д.
        /// </summary>
        private static bool IsAtLeast(string current, string target)
        {
            var v1 = current.Split('.');
            var v2 = target.Split('.');

            int maxLength = Math.Max(v1.Length, v2.Length);

            for (int i = 0; i < maxLength; i++)
            {
                // Если сегмент отсутствует, считаем его за 0 (например, "1.21" станет "1.21.0")
                int v1Part = i < v1.Length && int.TryParse(v1[i], out int res1) ? res1 : 0;
                int v2Part = i < v2.Length && int.TryParse(v2[i], out int res2) ? res2 : 0;

                if (v1Part > v2Part) return true;
                if (v1Part < v2Part) return false;
            }

            return true; // Версии полностью идентичны
        }


        #endregion Public Methods
    }
}