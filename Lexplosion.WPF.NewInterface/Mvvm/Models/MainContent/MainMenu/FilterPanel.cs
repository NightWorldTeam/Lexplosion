using Lexplosion.Logic.Management;
using Lexplosion.Logic.Objects;
using Lexplosion.WPF.NewInterface.Core.Objects.TranslatableObjects;
using Lexplosion.WPF.NewInterface.Core.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.MainMenu
{
    public class FilterPanel : ObservableObject
    {
        public event Action FilterChanged;

        public List<MinecraftVersion> Versions { get; set; } = new List<MinecraftVersion>();


        #region Properties


        public ITranslatableObject<InstanceSource> NoneSource { get; } = new InstanceSourceObject("AllSource", InstanceSource.None);


        private MinecraftVersion _selectedVersion;
        public MinecraftVersion SelectedVersion
        {
            get => _selectedVersion; set
            {
                _selectedVersion = value;
                FilterChanged?.Invoke();
            }
        }


        private ITranslatableObject<InstanceSource> _selectedSource;
        public ITranslatableObject<InstanceSource> SelectedSource
        {
            get => _selectedSource; set
            {
                _selectedSource = value;
                FilterChanged?.Invoke();
                OnPropertyChanged();
            }
        }

        private bool _isOperatorAnd;
        public bool IsOperatorAnd
        {
            get => _isOperatorAnd; set
            {
                _isOperatorAnd = value;
                FilterChanged?.Invoke();
            }
        }

        public IList<CategoryBase> AvailableCategories { get; } = new ObservableCollection<CategoryBase>();
        public IList<CategoryBase> SelectedCategories { get; } = new ObservableCollection<CategoryBase>();

        public IList<ITranslatableObject<InstanceSource>> Sources { get; } = new ObservableCollection<ITranslatableObject<InstanceSource>>();


        #endregion Properties


        public CollectionViewSource AvailableCategoriesViewSource { get; } = new();


        #region Constructors


        public FilterPanel()
        {
            SelectedSource = NoneSource;

            AvailableCategoriesViewSource.Source = AvailableCategories;
            AvailableCategoriesViewSource.View.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            
            Sources.Add(new InstanceSourceObject("NightWorld", InstanceSource.Nightworld));
            Sources.Add(new InstanceSourceObject("Curseforge", InstanceSource.Curseforge));
            Sources.Add(new InstanceSourceObject("Modrinth", InstanceSource.Modrinth));
            Sources.Add(new InstanceSourceObject("LocalSource", InstanceSource.Local));

            Versions.Add(new MinecraftVersion("All", MinecraftVersion.VersionType.Release));
            SelectedVersion = Versions[0];
        }


        #endregion Constructors


        #region Public Methods


        public void FilterChangedExecuteEvent() 
        {
            FilterChanged?.Invoke();
        }


        #endregion Public Methods
    }
}
