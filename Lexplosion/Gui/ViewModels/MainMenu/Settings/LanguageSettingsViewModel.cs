using System;
using System.Globalization;

namespace Lexplosion.Gui.ViewModels.MainMenu.Settings
{
    public class LanguageModel
    {
        private readonly CultureInfo _cultureInfo;
        private readonly MainViewModel _mainViewModel;

        public string Id { get => _cultureInfo.Name; }
        public string ImagePath { get; }
        public string NativeName { get; }
        public string CurrentLangName { get; }

        public LanguageModel(String cultureId, MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            _cultureInfo = new CultureInfo(cultureId);
            NativeName = (char.ToUpper(_cultureInfo.NativeName[0]) + _cultureInfo.NativeName.Substring(1)).Split(' ')[0];
            CurrentLangName = (char.ToUpper(_cultureInfo.DisplayName[0]) + _cultureInfo.DisplayName.Substring(1)).Split(' ')[0];
            ImagePath = "pack://application:,,,/Assets/images/icons/countries/" + _cultureInfo.Name + ".png";
        }

        private RelayCommand _languageSelectorCommand;
        public RelayCommand LanguageSelectorCommand
        {
            get => _languageSelectorCommand ?? (_languageSelectorCommand = new RelayCommand(obj =>
            {
                var lang = (LanguageModel)obj;
                Runtime.ChangeCurrentLanguage(lang.Id);
                _mainViewModel.UpdateLang();
            }));
        }
    }

    public class LanguageSettingsViewModel : VMBase
    {
        public LanguageModel[] Languages { get; }

        public LanguageSettingsViewModel(MainViewModel mainViewModel)
        {
            Languages = new LanguageModel[Runtime.Languages.Length];

            for (var i = 0; i < Runtime.Languages.Length; i++) 
            {
                Languages[i] = new LanguageModel(Runtime.Languages[i], mainViewModel);
            }
        }
    }
}
