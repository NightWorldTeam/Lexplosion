using Lexplosion.Global;
using Lexplosion.Gui.ViewModels.ModalVMs;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Tools;
using System;
using System.Globalization;

namespace Lexplosion.Gui.ViewModels.MainMenu.Settings
{
    public class LanguageModel : VMBase
    {
        private readonly CultureInfo _cultureInfo;
        private readonly MainViewModel _mainViewModel;

        //private Dictionary<string, string> 

        public string Id { get => _cultureInfo.Name; }
        public string ImagePath { get; }
        public string NativeName { get; }
        public string CurrentLangName { get; }
        public static LanguageModel SelectedLanguage { get; private set; }
        public static LanguageModel PrevSelectedLanguage { get; private set; }

        public LanguageModel(String cultureId, MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
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

                var dialog = new DialogViewModel(_mainViewModel);
                var message = ResourceGetter.GetString("changeLanguageWariningMessage");

                dialog.ShowDialog(ResourceGetter.GetString("langChange"), message, () => Runtime.ChangeCurrentLanguage(lang.Id, true));
                Runtime.ChangeCurrentLanguage(lang.Id, false);
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
