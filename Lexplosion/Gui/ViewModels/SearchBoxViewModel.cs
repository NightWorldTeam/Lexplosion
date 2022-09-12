using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects.Curseforge;
using System;
using System.Collections.ObjectModel;

namespace Lexplosion.Gui.ViewModels
{
    public sealed class SearchBoxViewModel : VMBase
    {

        #region Commands


        private RelayCommand _searchCommand;
        /// <summary>
        /// Команда запускающая поиск по тексту и д.р параметрам.
        /// </summary>
        public RelayCommand SearchCommand
        {
            get => _searchCommand ?? (new RelayCommand(obj =>
            {
                StartSearch();
            }));
        }


        #endregion Commands


        #region Properties


        public delegate void SearchChangedCallback();
        public event SearchChangedCallback SearchChanged;

        public ObservableCollection<CurseforgeCategory> Categories { get; }

        public bool IsMultiSource { get; }
        public bool HasCategories { get; }


        private InstanceSource _selectedInstanceSource = InstanceSource.Curseforge;
        /// <summary>
        /// Ресурс откуда получаем данные.
        /// Curseforge, NightWorld
        /// </summary>
        public InstanceSource SelectedInstanceSource
        {
            get => _selectedInstanceSource; set
            {
                _selectedInstanceSource = value;
                OnPropertyChanged();
                StartSearch();
            }
        }

        private string _searchTextUncomfirmed = string.Empty;
        /// <summary>
        /// Содержит текст, который пользователь ввел, но не запустил поиск.
        /// </summary>
        public string SearchTextUncomfirmed
        {
            get => _searchTextUncomfirmed; set
            {
                _searchTextUncomfirmed = value;
                OnPropertyChanged();
            }
        }


        private string _searchTextComfirmed = string.Empty;
        /// <summary>
        /// Содержит текст, по которому в данный момент выдаются данные.
        /// </summary>
        public string SearchTextComfirmed
        {
            get => _searchTextComfirmed; set
            {
                _searchTextComfirmed = value;
                OnPropertyChanged();
            }
        }

        private byte _selectedSourceIndex = 1;
        /// <summary>
        /// Индекс выбраного источника.
        /// 0 - NightWorld
        /// 1 - Curseforge
        /// </summary>
        public byte SelectedSourceIndex
        {
            get => _selectedSourceIndex; set
            {
                _selectedSourceIndex = value;
                SetSelectedInstanceSourceByIndex(value);
                OnPropertyChanged();
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


        //private CfSortBy _selectedCfSortBy;
        //public CfSortBy SelectedCfSortBy 
        //{
        //    get => _selectedCfSortBy; set 
        //    {
        //        _selectedCfSortBy = value;
        //        OnPropertyChanged();
        //    }
        //}


        #endregion Properties


        #region Constructors


        public SearchBoxViewModel(bool isMultiSource = false, bool hasCategories = false)
        {
            IsMultiSource = isMultiSource;
            HasCategories = hasCategories;
            Categories = PrepareCategories();
        }


        #endregion Constructors


        #region Public & Protected Methods





        #endregion Public & Protected Methods


        #region Private Methods


        private ObservableCollection<CurseforgeCategory> PrepareCategories() 
        {
            var categories = new ObservableCollection<CurseforgeCategory>(
                CurseforgeApi.GetCategories(CfProjectType.Modpacks)
            );

            categories.Add(new CurseforgeCategory() { name = "All" });

            categories.Move(categories.Count - 1, 0);

            SelectedCurseforgeCategory = categories[0];

            return categories;
        }

        private void StartSearch() 
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
        }


        private void SetSelectedInstanceSourceByIndex(byte value) 
        {
            if (value == 0)
                SelectedInstanceSource = InstanceSource.Nightworld;
            else if (value == 1)
                SelectedInstanceSource = InstanceSource.Curseforge;
        }

        #endregion Private Methods
    }
}