using System.Windows.Media;

namespace Lexplosion.Common.ViewModels.MainMenu.Settings
{
    public class AccentColorModel : VMBase
    {
        public Brush AccentColorBrush { get; }
        public Color AccentColor { get; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected; set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        private RelayCommand _changeColorCommand;
        public RelayCommand ChangeColorCommand
        {
            get => _changeColorCommand ?? (_changeColorCommand = new RelayCommand(obj =>
            {
                RuntimeApp.ChangeColorToColor(AccentColor);
            }));
        }

        public AccentColorModel(Color color)
        {
            AccentColor = color;
            AccentColorBrush = new SolidColorBrush(AccentColor);
            IsSelected = AccentColor == RuntimeApp.CurrentAccentColor;
        }
    }

    public class ApperanceSettingsViewModel : VMBase
    {

        public AccentColorModel[] AccentColors { get; }
        public ApperanceSettingsViewModel()
        {
            AccentColors = new AccentColorModel[RuntimeApp.AccentColors.Length];
            for (var i = 0; i < AccentColors.Length; i++)
            {
                AccentColors[i] = new AccentColorModel(RuntimeApp.AccentColors[i]);
            }
        }
    }
}
