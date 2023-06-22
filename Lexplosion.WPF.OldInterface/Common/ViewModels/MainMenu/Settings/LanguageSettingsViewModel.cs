using Lexplosion.Common.ViewModels.ModalVMs;
using Lexplosion.Global;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Tools;
using System;
using System.Globalization;

namespace Lexplosion.Common.ViewModels.MainMenu.Settings
{
    public class LanguageModel : VMBase
    {
        private readonly CultureInfo _cultureInfo;

        //private Dictionary<string, string> 

        public string Id { get => _cultureInfo.Name; }
        public string ImagePath { get; }
        public string NativeName { get; }
        public string CurrentLangName { get; }
        public static LanguageModel SelectedLanguage { get; private set; }
        public static LanguageModel PrevSelectedLanguage { get; private set; }

        public LanguageModel(String cultureId)
        {
            _cultureInfo = new CultureInfo(cultureId);
            NativeName = (char.ToUpper(_cultureInfo.NativeName[0]) + _cultureInfo.NativeName.Substring(1)).Split(' ')[0];
            CurrentLangName = ResourceGetter.GetString(_cultureInfo.Name);
            ImagePath = "pack://application:,,,/Assets/images/icons/countries/" + _cultureInfo.Name + ".png";
            IsSelectedLanguage = GlobalData.GeneralSettings.LanguageId == _cultureInfo.Name;
        }

        private RelayCommand _languageSelectorCommand;
        public RelayCommand LanguageSelectorCommand
        {
            get => _languageSelectorCommand ?? (_languageSelectorCommand = new RelayCommand(obj =>
            {
                var lang = (LanguageModel)obj;
                GlobalData.GeneralSettings.LanguageId = _cultureInfo.Name;
                DataFilesManager.SaveSettings(GlobalData.GeneralSettings);

                var dialog = new DialogViewModel();
                var message = ResourceGetter.GetString("changeLanguageWariningMessage");

                dialog.ShowDialog(ResourceGetter.GetString("langChange"), message, () => RuntimeApp.ChangeCurrentLanguage(lang.Id, true));
                RuntimeApp.ChangeCurrentLanguage(lang.Id, false);
            }));
        }

        private bool _isSelectedLanguage;
        public bool IsSelectedLanguage
        {
            get => _isSelectedLanguage; set
            {
                _isSelectedLanguage = value;
                OnPropertyChanged();
            }
        }
    }

    public class LanguageSettingsViewModel : VMBase
    {
        public LanguageModel[] Languages { get; }

        public LanguageSettingsViewModel()
        {
            Languages = new LanguageModel[RuntimeApp.AvaliableLanguages.Length];

            for (var i = 0; i < RuntimeApp.AvaliableLanguages.Length; i++)
            {
                Languages[i] = new LanguageModel(RuntimeApp.AvaliableLanguages[i]);
            }
        }
    }
}
