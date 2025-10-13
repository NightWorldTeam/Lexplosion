using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.Objects;
using Lexplosion.UI.WPF.Core.Services;
using System.Collections.Generic;

namespace Lexplosion.UI.WPF.Mvvm.Models.MainContent.Content.GeneralSettings
{
    public sealed class AppearanceSettingsModel : ViewModelBase
    {
        private readonly AppCore _appCore;
        private readonly AppColorThemeService _themeService;


        public Theme SelectedTheme { get => _themeService.SelectedTheme; }
        public ActivityColor SelectedColor { get => _themeService.SelectedActivityColor; }
        public string SelectedAppHeaderTemplateName
        {
            get => _themeService.SelectedAppHeaderTemplateName; set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _themeService.ChangeWindowHeaderTemplate(value);
                    OnPropertyChanged();
                }
            }
        }


        public IEnumerable<ActivityColor> Colors { get => _themeService.Colors; }
        public IEnumerable<Theme> Themes { get => _themeService.Themes; }

        public IEnumerable<string> HeaderTemplates { get => _themeService.HeaderTemplateNames; }


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


        private double _scalingFactor;
        public double ScalingFactor
        {
            get => _scalingFactor; set
            {
                if (_scalingFactor != value)
                {
                    _scalingFactor = value;
                    _appCore.Settings.Core.ZoomLevel = value / 100;
                    _appCore.Settings.SaveCore();
                    _appCore.Resources["ScalingFactorValue"] = value / 100;
                    OnPropertyChanged();
                }
            }
        }


        private bool _isCenterWindowAuto;
        public bool IsCenterWindowAuto
        {
            get => _isCenterWindowAuto; set
            {
                if (_isCenterWindowAuto != value)
                {
                    _isCenterWindowAuto = value;
                    _appCore.Settings.Core.IsCenterWindowAuto = value;
                    _appCore.Settings.SaveCore();
                    OnPropertyChanged();
                }
            }
        }

        private bool _isScalingAnimationEnabled;
        public bool IsScalingAnimationEnabled
        {
            get => _isScalingAnimationEnabled; set
            {
                if (_isScalingAnimationEnabled != value)
                {
                    _isScalingAnimationEnabled = value;
                    _appCore.Settings.Core.IsScalingAnimationEnabled = value;
                    _appCore.Settings.SaveCore();
                    OnPropertyChanged();
                }
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
            _scalingFactor = _appCore.Settings.Core.ZoomLevel * 100;
            IsScalingAnimationEnabled = _appCore.Settings.Core.IsScalingAnimationEnabled;
            OnPropertyChanged(nameof(ScalingFactor));
            IsCenterWindowAuto = _appCore.Settings.Core.IsCenterWindowAuto;
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
