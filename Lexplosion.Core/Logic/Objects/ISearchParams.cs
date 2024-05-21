using System.Collections.Generic;

namespace Lexplosion.Logic.Objects
{
    public interface ISearchParams
    {
        public string SearchFilter { get; }
        public string GameVersion { get; }
        public IEnumerable<IProjectCategory> Categories { get; }
        public int PageSize { get; }
        public int PageIndex { get; }
    }

    public static class DefaultSearchParams
    {
        public static readonly IEnumerable<IProjectCategory> EmptyCategoriesList = new List<IProjectCategory>();
    }

    public readonly struct ModrinthSearchParams : ISearchParams
    {
        public string SearchFilter { get; }
        public string GameVersion { get; }
        public IEnumerable<IProjectCategory> Categories { get; }
        public int PageSize { get; }
        public int PageIndex { get; }
        public ModrinthSortField SortField { get; }
        public string SortFieldString
        {
            get => SortField.ToString().ToLower();
        }

        public ModrinthSearchParams(string searchFilter, string gameVersion, IEnumerable<IProjectCategory> categories, int pageSize, int pageIndex, ModrinthSortField sortField)
        {
            SearchFilter = searchFilter ?? string.Empty;
            GameVersion = gameVersion ?? string.Empty;
            Categories = categories ?? DefaultSearchParams.EmptyCategoriesList;
            PageSize = pageSize;
            PageIndex = pageIndex;
            SortField = sortField;
        }

        public ModrinthSearchParams()
        {
            SearchFilter = string.Empty;
            GameVersion = string.Empty;
            Categories =  DefaultSearchParams.EmptyCategoriesList;
            PageSize = 1;
            PageIndex = 0;
            SortField = ModrinthSortField.Relevance;
        }
    }

    public readonly struct CurseforgeSearchParams : ISearchParams
    {
        public string SearchFilter { get; }
        public string GameVersion { get; }
        public IEnumerable<IProjectCategory> Categories { get; }
        public int PageSize { get; }
        public int PageIndex { get; }
        public CfSortField SortField { get; }

        public int LastIndexInPage
        {
            get => PageIndex * PageSize;
        }

        public CurseforgeSearchParams(string searchFilter, string gameVersion, IEnumerable<IProjectCategory> categories, int pageSize, int pageIndex, CfSortField sortField)
        {
            SearchFilter = searchFilter ?? string.Empty;
            GameVersion = gameVersion ?? string.Empty;
            Categories = categories ?? DefaultSearchParams.EmptyCategoriesList;
            PageSize = pageSize;
            PageIndex = pageIndex;
            SortField = sortField;
        }

        public CurseforgeSearchParams()
        {
            SearchFilter = string.Empty;
            GameVersion = string.Empty;
            Categories = DefaultSearchParams.EmptyCategoriesList;
            PageSize = 1;
            PageIndex = 0;
            SortField = CfSortField.Popularity;
        }
    }

    public readonly struct NightWorldSearchParams : ISearchParams
    {
        public string SearchFilter { get; }
        public string GameVersion { get; }
        public IEnumerable<IProjectCategory> Categories { get; }
        public int PageSize { get; }
        public int PageIndex { get; }

        public NightWorldSearchParams(int pageSize, int pageIndex)
        {
            SearchFilter = string.Empty;
            GameVersion = string.Empty;
            Categories = DefaultSearchParams.EmptyCategoriesList;
            PageSize = pageSize;
            PageIndex = pageIndex;
        }

        public NightWorldSearchParams()
        {
            SearchFilter = string.Empty;
            GameVersion = string.Empty;
            Categories = DefaultSearchParams.EmptyCategoriesList;
            PageSize = 1;
            PageIndex = 0;
        }
    }
}
