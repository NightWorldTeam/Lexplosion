using Lexplosion.Global;
using Lexplosion.Logic.FileSystem;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

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
            App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/Assets/langs/" + cultureId + ".xaml")
            });
        }

        private void OnLanguageModelChanged(LanguageModel langModel, string cultureId)
        {
            _selectedLang.IsSelected = false;
            _selectedLang = langModel;
            ChangeLangauge(cultureId);
            GlobalData.GeneralSettings.LanguageId = cultureId;
			Runtime.ServicesContainer.DataFilesService.SaveSettings(GlobalData.GeneralSettings);
        }
    }
}
