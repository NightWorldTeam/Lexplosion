using Lexplosion.Global;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.UI.WPF.Mvvm.Models.MainContent.Content.GeneralSettings
{
    public sealed class LanguageSettingsModel : ViewModelBase
    {
        public static readonly string[] AvailableLanguages = new string[]
        {
            "ru-RU", "en-US", //"uk-UA", "zh-CN", "de-DE"
        };

        private LanguageModel _selectedLang;
        private LanguageModel selectedLang;


        private ObservableCollection<LanguageModel> _languages = new ObservableCollection<LanguageModel>();
        public IEnumerable<LanguageModel> Languages { get => _languages; }


        #region Constructors


        public LanguageSettingsModel()
        {
            foreach (var al in AvailableLanguages)
            {
                var languageModel = new LanguageModel(al, al == GlobalData.GeneralSettings.LanguageId);

                if (languageModel.IsSelected)
                    _selectedLang = languageModel;

                languageModel.SelectedEvent += OnLanguageModelChanged;
                _languages.Add(languageModel);
            }
        }


        #endregion Constructors


        public void ChangeLangauge(string cultureId)
        {
            RuntimeApp._wpfLocalizationService.SetLanguage(cultureId);
        }

        private void OnLanguageModelChanged(LanguageModel langModel, string cultureId)
        {
            if (_selectedLang == langModel) return;
            _selectedLang.IsSelected = false;
            _selectedLang = langModel;
            ChangeLangauge(cultureId);
        }
    }
}
