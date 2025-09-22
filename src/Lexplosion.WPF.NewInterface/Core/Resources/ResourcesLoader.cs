using Lexplosion.WPF.NewInterface.Core.Resources.Theme;
using System.Windows;

namespace Lexplosion.Core.Resources
{
    public class ResourcesLoader 
    {
        public (string, ResourceDictionary) LoadThemeFromPath(string path)
        {
            var theme = new ThemeLoader(path);
            return ($"{theme.Name}ColorTheme", theme.ToResourceDictionary());
        }
    }
}
