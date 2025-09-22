using Lexplosion.Logic.Management;
using Lexplosion.Logic.Objects;
using Lexplosion.UI.WPF.Core.Objects.TranslatableObjects;
using Lexplosion.UI.WPF.Core.ViewModel;
using Lexplosion.UI.WPF.Mvvm.Models.InstanceControllers;
using Lexplosion.UI.WPF.Mvvm.Models.Mvvm.InstanceModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Lexplosion.UI.WPF.Mvvm.Models.MainContent.MainMenu
{
    public class LibraryFilterPanel : ObservableObject
    {
        public event Action FilterChanged;

        public IList<MinecraftVersion> Versions { get; set; } = new ObservableCollection<MinecraftVersion>();

        /// <summary>
        /// Category - Count of Instance with category.
        /// </summary>
        private readonly Dictionary<string, int> CategoriesCount = new();

        /// <summary>
        /// Category - Count of Instance with category.
        /// </summary>
        private readonly Dictionary<MinecraftVersion, int> VersionsCount = new();


        #region Properties


        public ITranslatableObject<InstanceSource> NoneSource { get; } = new InstanceSourceObject("AllSource", InstanceSource.None);
        public MinecraftVersion AllVersion { get; } = new MinecraftVersion("All", MinecraftVersion.VersionType.Release);


        public IList<CategoryBase> AvailableCategories { get; } = new ObservableCollection<CategoryBase>();
        public ISet<CategoryBase> SelectedCategories { get; } = new HashSet<CategoryBase>();
        public IList<ITranslatableObject<InstanceSource>> Sources { get; } = new ObservableCollection<ITranslatableObject<InstanceSource>>();


        public int SelectedIndex { get; set; }


        private MinecraftVersion _selectedVersion;
        public MinecraftVersion SelectedVersion
        {
            get => _selectedVersion; set
            {
                _selectedVersion = value;
                FilterChanged?.Invoke();
                OnPropertyChanged();
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
                OnPropertyChanged();
            } 
        }


        #endregion Properties


        #region Constructors


        public LibraryFilterPanel(IInstanceController instanceController)
        {
            instanceController.InstanceAdded += InstanceModelBase_GlobalAddedToLibrary;

            Versions.Add(AllVersion);

            foreach (var instance in instanceController.Instances) 
            {
                InstanceModelBase_GlobalAddedToLibrary(instance);
            }

            InstanceModelBase.GlobalAddedToLibrary += InstanceModelBase_GlobalAddedToLibrary;
            InstanceModelBase.GlobalDeletedEvent += InstanceModelBase_GlobalDeletedEvent;

            SelectedSource = NoneSource;
            
            Sources.Add(new InstanceSourceObject("NightWorld", InstanceSource.Nightworld));
            Sources.Add(new InstanceSourceObject("Curseforge", InstanceSource.Curseforge));
            Sources.Add(new InstanceSourceObject("Modrinth", InstanceSource.Modrinth));
            Sources.Add(new InstanceSourceObject("LocalSource", InstanceSource.Local));

            SelectedVersion = Versions[0];
        }


        #endregion Constructors


        #region Public Methods


        public void FilterChangedExecuteEvent() 
        {
            FilterChanged?.Invoke();
        }


        #endregion Public Methods


        #region Private Methods


        private void InstanceModelBase_GlobalAddedToLibrary(InstanceModelBase instanceModel)
        {
            // Version
            if (VersionsCount.ContainsKey(instanceModel.GameVersion))
            {
                VersionsCount[instanceModel.GameVersion]++;
            }
            else 
            {
                VersionsCount.Add(instanceModel.GameVersion, 1);
                Versions.Add(instanceModel.GameVersion);
            }

            if (instanceModel.Tags != null) 
            { 
                // Categories
                foreach (var category in instanceModel.Tags) 
                { 
                    if (CategoriesCount.ContainsKey(category.Name))
                    {
                        CategoriesCount[category.Name]++;
                    }
                    else
                    {
                        CategoriesCount.Add(category.Name, 1);
                        AvailableCategories.Add(category as CategoryBase);
                    }
                }
            }
        }

        private void InstanceModelBase_GlobalDeletedEvent(InstanceModelBase instanceModel)
        {
            // получаем количество клиент с данной версией, если остался один клиент удаляем версию из списка.
            if (VersionsCount.TryGetValue(instanceModel.GameVersion, out var countV))
            {
                if (countV == 1) 
                {
                    Versions.Remove(instanceModel.GameVersion);
                    VersionsCount[instanceModel.GameVersion] = 0;
                    SelectedIndex = 0;
                    OnPropertyChanged(nameof(SelectedIndex));
                }
            }

            // получаем количество клиент с данной категорией, если остался один клиент удаляем категорию из списка.
            // O(n^2), O(n)?
            foreach (var category in instanceModel.Tags.Skip(0)) { 
                if (CategoriesCount.TryGetValue(category.Name, out var countC))
                {
                    if (countC == 1)
                    {
                        // O(n)
                        Versions.Remove(instanceModel.GameVersion);
                        CategoriesCount[category.Name] = 0;
                    }
                }
            }
        }


        #endregion Private Methods
    }
}
