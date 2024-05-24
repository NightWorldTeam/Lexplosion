using System.Collections.Generic;
using System.Linq;

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
        public static readonly IEnumerable<string> EmptyModloadersList = new List<string>();
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
        public IEnumerable<string> Modloaders { get; }

        private ModrinthSearchParams(string searchFilter, string gameVersion, IEnumerable<IProjectCategory> categories, int pageSize, int pageIndex, ModrinthSortField sortField, IEnumerable<string> modloaders)
        {
            SearchFilter = searchFilter ?? string.Empty;
            GameVersion = gameVersion ?? string.Empty;
            Categories = categories ?? DefaultSearchParams.EmptyCategoriesList;
            PageSize = pageSize;
            PageIndex = pageIndex;
            SortField = sortField;
            Modloaders = modloaders;
        }

        public ModrinthSearchParams(string searchFilter, string gameVersion, IEnumerable<IProjectCategory> categories, int pageSize, int pageIndex, ModrinthSortField sortField, IEnumerable<Modloader> modloaders)
            : this(searchFilter, gameVersion, categories, pageSize, pageIndex, sortField,
                  modloaders?.Select(x => x.ToString()) ?? DefaultSearchParams.EmptyModloadersList)
        { }

        public ModrinthSearchParams(string searchFilter, string gameVersion, IEnumerable<IProjectCategory> categories, int pageSize, int pageIndex, ModrinthSortField sortField, IEnumerable<ClientType> clientTypes)
            : this(searchFilter, gameVersion, categories, pageSize, pageIndex, sortField,
                   clientTypes?.Where(x => x != ClientType.Vanilla)?.Select(x => x.ToString()) ?? DefaultSearchParams.EmptyModloadersList)
        { }

        public ModrinthSearchParams(string searchFilter, string gameVersion, IEnumerable<IProjectCategory> categories, int pageSize, int pageIndex, ModrinthSortField sortField)
            : this(searchFilter, gameVersion, categories, pageSize, pageIndex, sortField, DefaultSearchParams.EmptyModloadersList) { }

        public ModrinthSearchParams()
        {
            SearchFilter = string.Empty;
            GameVersion = string.Empty;
            Categories = DefaultSearchParams.EmptyCategoriesList;
            PageSize = 1;
            PageIndex = 0;
            SortField = ModrinthSortField.Relevance;
            Modloaders = DefaultSearchParams.EmptyModloadersList;
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
        public IEnumerable<string> Modloaders { get; }

        public int LastIndexInPage
        {
            get => PageIndex * PageSize;
        }

        private CurseforgeSearchParams(string searchFilter, string gameVersion, IEnumerable<IProjectCategory> categories, int pageSize, int pageIndex, CfSortField sortField, IEnumerable<string> modloaders)
        {
            SearchFilter = searchFilter ?? string.Empty;
            GameVersion = gameVersion ?? string.Empty;
            Categories = categories ?? DefaultSearchParams.EmptyCategoriesList;
            PageSize = pageSize;
            PageIndex = pageIndex;
            SortField = sortField;
            Modloaders = modloaders?.Select(x => x.ToString()) ?? DefaultSearchParams.EmptyModloadersList;
        }

        public CurseforgeSearchParams(string searchFilter, string gameVersion, IEnumerable<IProjectCategory> categories, int pageSize, int pageIndex, CfSortField sortField, IEnumerable<Modloader> modloaders)
        : this(searchFilter, gameVersion, categories, pageSize, pageIndex, sortField, modloaders?.Select(x => x.ToString()) ?? DefaultSearchParams.EmptyModloadersList) { }

        public CurseforgeSearchParams(string searchFilter, string gameVersion, IEnumerable<IProjectCategory> categories, int pageSize, int pageIndex, CfSortField sortField, IEnumerable<ClientType> clientTypes)
        : this(searchFilter, gameVersion, categories, pageSize, pageIndex, sortField, clientTypes?.Where(x => x != ClientType.Vanilla)?.Select(x => x.ToString()) ?? DefaultSearchParams.EmptyModloadersList) { }

        public CurseforgeSearchParams(string searchFilter, string gameVersion, IEnumerable<IProjectCategory> categories, int pageSize, int pageIndex, CfSortField sortField)
            : this(searchFilter, gameVersion, categories, pageSize, pageIndex, sortField, DefaultSearchParams.EmptyModloadersList) { }

        public CurseforgeSearchParams()
        {
            SearchFilter = string.Empty;
            GameVersion = string.Empty;
            Categories = DefaultSearchParams.EmptyCategoriesList;
            PageSize = 1;
            PageIndex = 0;
            SortField = CfSortField.Popularity;
            Modloaders = DefaultSearchParams.EmptyModloadersList;
        }
    }

    public readonly struct NightWorldSearchParams : ISearchParams
    {
        public string SearchFilter { get; }
        public string GameVersion { get; }
        public IEnumerable<IProjectCategory> Categories { get; }
        public int PageSize { get; }
        public int PageIndex { get; }
        public IEnumerable<string> Modloaders { get; }

        public NightWorldSearchParams(int pageSize, int pageIndex)
        {
            SearchFilter = string.Empty;
            GameVersion = null;
            Categories = DefaultSearchParams.EmptyCategoriesList;
            PageSize = pageSize;
            PageIndex = pageIndex;
            Modloaders = DefaultSearchParams.EmptyModloadersList;
        }

        public NightWorldSearchParams()
        {
            SearchFilter = string.Empty;
            GameVersion = string.Empty;
            Categories = DefaultSearchParams.EmptyCategoriesList;
            PageSize = 1;
            PageIndex = 0;
            Modloaders = DefaultSearchParams.EmptyModloadersList;
        }
    }
}
