using System.Windows;
using System.Windows.Media;

namespace Lexplosion.Tools
{
    internal static class ResourceGetter
    {
        public static Geometry GetIcon(string key) 
        {
            return Geometry.Parse((string)App.Current.Resources[key]);
        }

        public static Color GetColor(string key) 
        {
            return (Color)Application.Current.Resources[key];
        }

        public static string GetString(string key) 
        {
            return (string)Application.Current.Resources[key] ?? "Не удалось найти значение";
        }
    }
}
