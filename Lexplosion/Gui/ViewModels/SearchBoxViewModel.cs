using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects.Curseforge;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.Gui.ViewModels
{
    public sealed class SearchBoxViewModel : VMBase
    {
        private RelayCommand _searchCommand;

        private string _searchTextUncomfirmed = string.Empty;
        private string _searchTextComfirmed = string.Empty;

        public delegate void SearchChangedCallback();
        public event SearchChangedCallback SearchChanged;

        private InstanceSource _selectedInstanceSource = InstanceSource.Curseforge;
        private int _selectedSourceIndex = 1;

        public bool IsMultiSource { get; }

        public InstanceSource SelectedInstanceSource
        {
            get => _selectedInstanceSource; set
            {
                _selectedInstanceSource = value;
                Lexplosion.Run.TaskRun(() => {
                    SearchChanged.Invoke();
                });
                OnPropertyChanged();
            }
        }

        public RelayCommand SearchCommand
        {
            get => _searchCommand ?? (new RelayCommand(obj =>
            {
                if (SearchTextComfirmed != SearchTextUncomfirmed)
                {
                    if (SearchTextUncomfirmed.Length != 0)
                    {
                        SearchTextComfirmed = SearchTextUncomfirmed;
                    }
                    else
                    {
                        SearchTextComfirmed = "";
                    }

                    SearchChanged.Invoke();
                }
            }));
        }

        public string SearchTextUncomfirmed
        {
            get => _searchTextUncomfirmed; set
            {
                _searchTextUncomfirmed = value;
                OnPropertyChanged(nameof(SearchTextUncomfirmed));
            }
        }

        public string SearchTextComfirmed
        {
            get => _searchTextComfirmed; set
            {
                _searchTextComfirmed = value;
                OnPropertyChanged(nameof(SearchTextComfirmed));
            }
        }

        public int SelectedSourceIndex
        {
            get => _selectedSourceIndex; set
            {
                _selectedSourceIndex = value;
                if (value == 0) SelectedInstanceSource = InstanceSource.Nightworld;
                if (value == 1) SelectedInstanceSource = InstanceSource.Curseforge;
                OnPropertyChanged(nameof(SelectedSourceIndex));
            }
        }

        private CurseforgeCategory _selectedCurseforgeCategory;
        public CurseforgeCategory SelectedCurseforgeCategory
        {
            get => _selectedCurseforgeCategory; set 
            {
                _selectedCurseforgeCategory = value;
                OnPropertyChanged();
                Console.WriteLine(value.name);
                SearchChanged?.Invoke();
            }
        }

        public SearchBoxViewModel(bool isMultiSource = false)
        {
            IsMultiSource = isMultiSource;
            Categories = new ObservableCollection<CurseforgeCategory>(
                CurseforgeApi.GetCategories(CfProjectType.Modpacks)
                );

            Categories.Add(new CurseforgeCategory() 
            {
                name = "All"
            });

            Categories.Move(Categories.Count - 1, 0);

            SelectedCurseforgeCategory = Categories[0];
        }

        public ObservableCollection<CurseforgeCategory> Categories { get; }
    }
}