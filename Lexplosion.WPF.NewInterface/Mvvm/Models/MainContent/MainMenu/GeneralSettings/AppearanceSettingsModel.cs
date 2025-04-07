using Lexplosion.Global;
using Lexplosion.Logic.FileSystem;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Core.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.Content.GeneralSettings
{
    public sealed class AppearanceSettingsModel : ViewModelBase
    {
        private readonly AppCore _appCore;
        private readonly AppColorThemeService _themeService;
        

        public Theme SelectedTheme { get => _themeService.SelectedTheme; }
        public ActivityColor SelectedColor { get => _themeService.SelectedActivityColor; }


        public IEnumerable<ActivityColor> Colors { get => _themeService.Colors; }
        public IEnumerable<Theme> Themes { get => _themeService.Themes; }


        private string _newHexActivityColor;
        public string NewHexActivityColor
        {
            get => _newHexActivityColor; set
            {
                _newHexActivityColor = value;

                if (ActivityColor.TryCreateColor(value, out var color))
                {
                    _themeService.SelectedColorChanged(color, true);
                }
                else
                {
                    _themeService.SelectedColorChanged(SelectedColor, true);
                }

                OnPropertyChanged();
            }
        }


        #region Tooltip


        private bool _isToolTipsEnabled;
        public bool IsToolTipsEnabled
        {
            get => _isToolTipsEnabled; set
            {
                _isToolTipsEnabled = value;
                OnPropertyChanged();
                OnToolTipStateChanged();
            }
        }


        private int _initialShowDelay;
        public int InitialShowDelay
        {
            get => _initialShowDelay; set
            {
                _initialShowDelay = value;
                OnPropertyChanged();
                OnInitialShowDelayChanged();
            }
        }

        private int _betweenShowDelay;
        public int BetweenShowDelay
        {
            get => _betweenShowDelay; set
            {
                _betweenShowDelay = value;
                OnPropertyChanged();
                OnBetweenShowDelayChanged();
            }
        }


        #endregion Tooltip


        public AppearanceSettingsModel(AppCore appCore)
        {
            _appCore = appCore;
            _themeService = _appCore.Settings.ThemeService;
        }


        #region Private Methods


        /// <summary>
        /// Изменяет состояние всплывающих подсказок
        /// </summary>
        private void OnToolTipStateChanged()
        {
            RuntimeApp.ChangeToolTipState(IsToolTipsEnabled);
        }

        private void OnInitialShowDelayChanged()
        {
            RuntimeApp.ChangeSettingInitialShowDelay(InitialShowDelay);
        }

        private void OnBetweenShowDelayChanged()
        {
            RuntimeApp.ChangeSettingBetweenShowDelay(BetweenShowDelay);
        }


        #endregion Private Methods
    }
}
