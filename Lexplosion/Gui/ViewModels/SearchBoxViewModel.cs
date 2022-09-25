using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.Curseforge;
using Lexplosion.Tools;
using LumiSoft.Net.Mime.vCard;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static Lexplosion.Logic.Objects.Curseforge.CurseforgeProjectInfo;

namespace Lexplosion.Gui.ViewModels
{
    public sealed class SearchBoxViewModel : VMBase
    {

        #region Properties

        public event Action<string> SearchChanged;

        public ObservableCollection<CurseforgeCategory> Categories { get; private set; }

        public static List<string> CfSortToString { get; } = new List<string>()
        {
            ResourceGetter.GetString("featuredSortBy"),
            ResourceGetter.GetString("popularitySortBy"),
            ResourceGetter.GetString("lastUpdatedSortBy"),
            ResourceGetter.GetString("nameSortBy"),
            ResourceGetter.GetString("authorSortBy"),
            ResourceGetter.GetString("totalDownloadsFlSortBy"),
            ResourceGetter.GetString("categorySortBy"),
            ResourceGetter.GetString("gameVersionSortBy"),
        };

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

        private CurseforgeCategory _selectedCurseforgeCategory = new CurseforgeCategory() { name = "All", id = -1 };
        public CurseforgeCategory SelectedCurseforgeCategory
        {
            get => _selectedCurseforgeCategory; set 
            {
                _selectedCurseforgeCategory = value;
                OnPropertyChanged();
                SearchChanged?.Invoke("");
            }
        }


        public CfSortField SelectedCfSortBy = CfSortField.Popularity;
        
        private string _selectedCfSortByString = CfSortToString[(int)CfSortBy.Popularity];
        public string SelectedCfSortByString
        {
            get => _selectedCfSortByString; set
            {
                _selectedCfSortByString = value;
                OnPropertyChanged();
                SelectedCfSortBy = (CfSortField)CfSortToString.IndexOf(value) - 1;
                SearchChanged?.Invoke("");
            }
        }


        private string _selectedVersion;
        public string SelectedVersion 
        {
            get => _selectedVersion; set 
            {
                _selectedVersion = value;
                OnPropertyChanged();
                StartSearch();
            }
        }


        #endregion Properties


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


        #region Constructors


        public SearchBoxViewModel(bool isMultiSource = false, bool hasCategories = false)
        {
            IsMultiSource = isMultiSource;
            HasCategories = hasCategories;
            Lexplosion.Run.TaskRun(() => { 
                Categories = PrepareCategories();
            });
            SelectedVersion = ResourceGetter.GetString("allVersions");
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

            categories.Add(SelectedCurseforgeCategory);

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
            }

            SearchChanged?.Invoke("");
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