using System;
using System.Threading;
using System.Windows;
using Lexplosion.Global;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Management.Localization;

namespace Lexplosion.UI.WPF.Core
{
    /// <summary>
    /// WPF-реализация <see cref="ILocalizationService"/>.
    /// Оборачивает <see cref="App.Current.Resources"/> для поиска строк по ключу.
    /// Владеет логикой переключения языка (SetLanguage), включая замену ResourceDictionary
    /// и сохранение выбранного языка в настройках.
    /// </summary>
    public sealed class WpfLocalizationService : ILocalizationService
    {
        private readonly DataFilesManager _dataFilesManager;
        private ResourceDictionary _currentLangDict;

        /// <inheritdoc/>
        public string CurrentLanguageId { get; private set; }

        /// <inheritdoc/>
        public event Action LanguageChanged;

        /// <summary>
        /// Создаёт сервис и загружает язык из <see cref="GlobalData.GeneralSettings.LanguageId"/>.
        /// Если LanguageId пустой, определяет язык по культуре ОС и сохраняет его в настройках.
        /// </summary>
        public WpfLocalizationService(DataFilesManager dataFilesManager)
        {
            _dataFilesManager = dataFilesManager;

            string langId = GlobalData.GeneralSettings.LanguageId;

            if (string.IsNullOrWhiteSpace(langId))
            {
                langId = ResolveDefaultLanguage();
                GlobalData.GeneralSettings.LanguageId = langId;
                _dataFilesManager.SaveSettings(GlobalData.GeneralSettings);
            }

            LoadDictionary(langId);
            CurrentLanguageId = langId;
        }

        /// <summary>
        /// Возвращает локализованную строку для заданного ключа из активного ResourceDictionary.
        /// Если ключ не найден — возвращает сам ключ (никогда не возвращает null).
        /// </summary>
        public string GetString(string key)
        {
            if (key == null) return string.Empty;
            return App.Current.Resources[key] as string ?? key;
        }

        /// <summary>
        /// Переключает активный язык. Является no-op, если <paramref name="languageId"/> совпадает
        /// с текущим. После успешной смены обновляет настройки, сохраняет их и вызывает
        /// <see cref="LanguageChanged"/>.
        /// </summary>
        public void SetLanguage(string languageId)
        {
            if (string.IsNullOrWhiteSpace(languageId)) return;
            if (languageId == CurrentLanguageId) return;

            LoadDictionary(languageId);
            CurrentLanguageId = languageId;

            GlobalData.GeneralSettings.LanguageId = languageId;
            _dataFilesManager.SaveSettings(GlobalData.GeneralSettings);

            LanguageChanged?.Invoke();
        }

        // ── private helpers ──────────────────────────────────────────────────────

        private void LoadDictionary(string languageId)
        {
            var dict = new ResourceDictionary
            {
                Source = new Uri(RuntimeApp.LangPath + languageId + ".xaml")
            };

            // Убираем старый словарь языка (если он есть), чтобы не копились дубли
            if (_currentLangDict != null)
                App.Current.Resources.MergedDictionaries.Remove(_currentLangDict);

            App.Current.Resources.MergedDictionaries.Add(dict);
            _currentLangDict = dict;
        }

        private static string ResolveDefaultLanguage()
        {
            try
            {
                string culture = Thread.CurrentThread.CurrentCulture.ToString();
                // Проверяем, существует ли XAML для данной культуры
                _ = new ResourceDictionary
                {
                    Source = new Uri(RuntimeApp.LangPath + culture + ".xaml")
                };
                return culture;
            }
            catch
            {
                return "ru-RU";
            }
        }
    }
}
