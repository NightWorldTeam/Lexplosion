using Lexplosion.Global;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Lexplosion.UI.WPF.Core.Objects
{
    public struct LanguageModel : INotifyPropertyChanged
    {
        public readonly CultureInfo _cultureInfo = null;

        public event Action<LanguageModel, string> SelectedEvent = null;
        public event PropertyChangedEventHandler PropertyChanged = null;


        #region Properties


        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected; set
            {
                _isSelected = value;
                if (_isSelected)
                {
                    SelectedEvent?.Invoke(this, _cultureInfo.Name);
                }
                OnPropertyChanged();
            }
        }

        public string CurrentLangNameKey { get; }
        public string NativeName { get; }
        public string LangLogoPath { get; }


        #endregion Properties


        #region Constructors


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


        #endregion Constructors


        private void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
