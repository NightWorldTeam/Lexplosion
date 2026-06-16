using System;

namespace Lexplosion.Logic.Management.Localization
{
    /// <summary>
    /// Возвращает ключ как есть для каждого вызова GetString
    /// Используется как значение по умолчанию в AllServicesContainer до тех пор, пока слой UI не внедрит свою реализацию
    /// </summary>
    internal sealed class NullLocalizationService : ILocalizationService
    {
        public static readonly NullLocalizationService Instance = new NullLocalizationService();

        private NullLocalizationService() { }

        public string CurrentLanguageId => string.Empty;

        // Никогда не срабатывает в null реализации
        public event Action LanguageChanged { add { } remove { } }

        /// <summary>Возвращает <paramref name="key"/> без изменений, или <see cref="string.Empty"/> для null.</summary>
        public string GetString(string key) => key ?? string.Empty;
    }
}
