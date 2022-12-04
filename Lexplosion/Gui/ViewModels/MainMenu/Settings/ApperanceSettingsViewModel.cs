using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lexplosion.Global;
using Lexplosion.Logic.FileSystem;
using System.Globalization;

namespace Lexplosion.Gui.ViewModels.MainMenu.Settings
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
                Runtime.ChangeColorToColor(AccentColor);
            }));
        }

        public AccentColorModel(Color color)
        {
            AccentColor = color;
            AccentColorBrush = new SolidColorBrush(AccentColor);
            IsSelected = AccentColor == Runtime.CurrentAccentColor;
        }
    }

    public class ApperanceSettingsViewModel : VMBase
    {

        public AccentColorModel[] AccentColors { get; }
        public ApperanceSettingsViewModel()
        {
            AccentColors = new AccentColorModel[Runtime.AccentColors.Length];
            for (var i = 0; i < AccentColors.Length; i++) 
            {
                AccentColors[i] = new AccentColorModel(Runtime.AccentColors[i]);
            }
        }
    }
}
