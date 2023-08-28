using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Models.MainContent;

namespace Lexplosion.WPF.NewInterface.ViewModels.MainContent.MainMenu
{
    public sealed class CatalogViewModel : ViewModelBase
    {
        public CatalogModel Model { get; }


        #region Commands



        #endregion Commands


        #region Constructors


        public CatalogViewModel()
        {
            Model = new CatalogModel();
        }


        #endregion Consturctors
    }
}
