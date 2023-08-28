using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Models.MainContent;

namespace Lexplosion.WPF.NewInterface.ViewModels.MainContent.MainMenu
{
    public sealed class LibraryViewModel : ViewModelBase
    {
        public LibraryModel Model { get; }

        public LibraryViewModel()
        {
            Model = new LibraryModel();
        }
    }
}
