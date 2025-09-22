using System.Windows;

namespace Lexplosion.UI.WPF.Core.Resources
{
    public interface IResourceLoader
    {
        internal ResourceDictionary ToResourceDictionary();
    }
}
