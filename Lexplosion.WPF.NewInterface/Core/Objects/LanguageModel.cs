using Lexplosion.Global;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Lexplosion.WPF.NewInterface.Core.Objects
{
    public struct LanguageModel : INotifyPropertyChanged
    {
        public readonly CultureInfo _cultureInfo;

        public event Action<string> SelectedEvent;
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected; set
            {
                _isSelected = value;
                if (_isSelected) 
                { 
                    SelectedEvent?.Invoke(_cultureInfo.Name);
                }
                OnPropertyChanged();
            }
        }

        public string CurrentLangNameKey { get; }
        public string NativeName { get; }
        public string LangLogoPath { get; }

        public LanguageModel(string cultureId, bool isSelected = false)
        {
            Runtime.DebugWrite(cultureId);
            _cultureInfo = new CultureInfo(cultureId);
            NativeName = (char.ToUpper(_cultureInfo.NativeName[0]) + _cultureInfo.NativeName.Substring(1)).Split(' ')[0];
            CurrentLangNameKey = _cultureInfo.Name;
            LangLogoPath = "pack://application:,,,/Assets/images/icons/countries/" + _cultureInfo.Name + ".png";
            IsSelected = isSelected;
        }
        
        public LanguageModel(string cultureId)
        {
            _cultureInfo = new CultureInfo(cultureId);
            NativeName = (char.ToUpper(_cultureInfo.NativeName[0]) + _cultureInfo.NativeName.Substring(1)).Split(' ')[0];
            CurrentLangNameKey = _cultureInfo.Name;
            LangLogoPath = "pack://application:,,,/Assets/images/icons/countries/" + _cultureInfo.Name + ".png";
            IsSelected = GlobalData.GeneralSettings.LanguageId == _cultureInfo.Name;
        }

        private void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
