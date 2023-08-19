using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace Lexplosion.WPF.NewInterface.Models.MainContent.Content.GeneralSettings
{
    public sealed class LanguageSettingsModel : ViewModelBase
    {
        private ObservableCollection<LanguageModel> _languages = new ObservableCollection<LanguageModel>();
        public IEnumerable<LanguageModel> Languages { get => _languages; }


        public static readonly string[] AvaliableLanguages = new string[2]
        {
            "ru-RU", "en-US"
        };

        public LanguageSettingsModel()
        {
            foreach (var al in AvaliableLanguages) 
            {
                var languageModel = new LanguageModel(al, false);
                languageModel.SelectedEvent += OnLanguageModelChanged;
                _languages.Add(languageModel);
            }
        }

        private void OnLanguageModelChanged(string cultureId)
        {
            ChangeLangauge(cultureId);
            //GlobalData.GeneralSettings.LanguageId = cultureId;
            //DataFilesManager.SaveSettings(GlobalData.GeneralSettings);
        }

        public void ChangeLangauge(string cultureId) 
        {
            App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/Assets/langs/" + cultureId + ".xaml")
            });
        }
    }
}
