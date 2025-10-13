using System.Windows.Media;

namespace Lexplosion.UI.WPF.Core.Resources.Theme
{
    class ThemeItem
    {
        #region Properties


        /// <summary>
        /// Название
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Прозрачность цвета
        /// </summary>
        public double Opacity { get; private set; }
        /// <summary>
        /// Цвет
        /// </summary>
        public Color Color { get; }
        /// <summary>
        /// Кисть (SolidColorBrush)
        /// </summary>
        public Brush Brush { get; }


        #endregion Properties 


        public ThemeItem(string name, string color, double opacity = 1.0)
        {
            Name = name;
            Color = (Color)ColorConverter.ConvertFromString(color);
            Brush = new SolidColorBrush(Color)
            {
                Opacity = opacity
            };
        }
    }
}
