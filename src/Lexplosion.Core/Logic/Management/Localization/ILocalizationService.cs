using System;

namespace Lexplosion.Logic.Management.Localization
{
    /// <summary>
    /// Интерфейс для предоставления локализованных строк
    /// </summary>
    public interface ILocalizationService
    {
        /// <summary>
        /// Возвращает локализованную строку для заданного ключа
        /// </summary>
        string GetString(string key);

        /// <summary>
        /// Идентификатор локали текущего активного языка
        /// </summary>
        string CurrentLanguageId { get; }

        /// <summary>
        /// Происходит после успешного переключения языка, когда новые данные перевода
        /// полностью загружены. Компоненты UI подписываются для повторной загрузки строк
        /// </summary>
        event Action LanguageChanged;
    }
}
