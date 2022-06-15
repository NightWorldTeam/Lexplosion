using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Gui.ViewModels.CurseforgeMarket
{
    public class CurseforgeMarketViewModel : VMBase
    {
        private RelayCommand _closePage;

        public PaginatorViewModel PaginatorVM { get; } = new PaginatorViewModel();
        public SearchBoxViewModel SearchBoxVM { get; } = new SearchBoxViewModel();

        public RelayCommand ClosePageCommand 
        {
            get => _closePage ?? (new RelayCommand(obj => 
            {
                MainViewModel.NavigationStore.CurrentViewModel = MainViewModel.NavigationStore.PrevViewModel;
            }));
        }

        public CurseforgeMarketViewModel()
        {

        }
    }
}
