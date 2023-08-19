using Lexplosion.Logic.Objects;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.ViewModels.AddonsRepositories.Modrinth
{
    public sealed class ModrinthModsModel : ViewModelBase 
    {
        private ObservableCollection<IProjectCategory> _categories = new ObservableCollection<IProjectCategory>();
        public IEnumerable<IProjectCategory> Categories { get => _categories; }


        #region Public Methods


        public void Search(string value) { }

        public void ClearFilters() { }


        #endregion Public Methods
    }

    public class ModrinthModsViewModel : ViewModelBase
    {
        public ModrinthModsModel Model { get; }


        #region Commands


        private RelayCommand _clearFiltersCommand;
        public ICommand ClearFiltersCommand 
        {
            get => _clearFiltersCommand ?? (_clearFiltersCommand = new RelayCommand(obj => 
            {
                Model.ClearFilters();
            }));
        }

        private RelayCommand _searchCommand;
        public RelayCommand SearchCommand 
        {
            get => _searchCommand ?? (_searchCommand = new RelayCommand(obj => 
            {
                Model.Search((string)obj);
            }));
        }

        #endregion Commands
    }
}
