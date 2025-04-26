using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.Curseforge;
using Lexplosion.Tools;

namespace Lexplosion.Logic.Network.Web
{
	public class CurseforgeApi
	{
		private const string Token = "$2a$10$Ky9zG9R9.ha.kf5BRrvwU..OGSvC0I2Wp56hgXI/4aRtGbizrm3we";
		private readonly ToServer _toServer;

		private class DataContainer<T>
		{
			[JsonProperty("data")]
			public T Data;
			[JsonProperty("pagination")]
			public Pagination Paginator { get; set; }
		}

		private class Pagination
		{
			[JsonProperty("index")]
			public int Index { get; set; }
			[JsonProperty("pageSize")]
			public int PageSize { get; set; }
			[JsonProperty("resultCount")]
			public int ResultCount { get; set; }
			[JsonProperty("totalCount")]
			public int TotalCount { get; set; }
		}

		public class FingerprintSearchAnswer
		{
			public class SearchedFiles
			{
				public int id;
				public CurseforgeFileInfo file;
			}

			public List<SearchedFiles> exactMatches;
		}

		public CurseforgeApi(ToServer toServer)
		{
			_toServer = toServer;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private T GetApiData<T>(string url, out Pagination pagination) where T : new()
		{
			pagination = null;

			try
			{
				var headers = new Dictionary<string, string>()
				{
					["x-api-key"] = Token
				};

				string answer = _toServer.HttpGet(url, headers);
				if (answer != null)
				{
					var data = JsonConvert.DeserializeObject<DataContainer<T>>(answer);
					if (data == null) return new T();

					pagination = data.Paginator;
					return data.Data ?? new T();
				}

				return new T();
			}
			catch
			{
				return new T();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private T GetApiData<T>(string url, string jsonInputData, out Pagination pagination) where T : new()
		{
			pagination = null;

			try
			{
				var headers = new Dictionary<string, string>()
				{
					["x-api-key"] = Token
				};

				string answer = _toServer.HttpPostJson(url, jsonInputData, out _, headers);
				if (answer != null)
				{
					var data = JsonConvert.DeserializeObject<DataContainer<T>>(answer);
					if (data == null) return new T();

					pagination = data.Paginator;
					return data.Data ?? new T();
				}

				return new T();
			}
			catch
			{
				return new T();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private T GetApiData<T>(string url, string jsonInputData) where T : new() => GetApiData<T>(url, jsonInputData, out _);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private T GetApiData<T>(string url) where T : new() => GetApiData<T>(url, out _);

		public CatalogResult<CurseforgeInstanceInfo> GetInstances(CurseforgeSearchParams searchParams)
		{
			var queryBuilder = new QueryApiBuilder("https://api.curseforge.com/v1/mods/search");

			queryBuilder.Add("gameId", "432");
			queryBuilder.Add("classId", "4471");
			queryBuilder.Add("sortOrder", "desc");
			queryBuilder.Add("pageSize", searchParams.PageSize);
			queryBuilder.Add("index", searchParams.PageSize * searchParams.PageIndex);

			if (!string.IsNullOrWhiteSpace(searchParams.SearchFilter))
				queryBuilder.Add("searchFilter", WebUtility.UrlEncode(searchParams.SearchFilter));

			if (!string.IsNullOrWhiteSpace(searchParams.GameVersion))
				queryBuilder.Add("gameVersion", searchParams.GameVersion);

			queryBuilder.Add("categoryIds", BuildCategoriesToQuery(searchParams.Categories));
			queryBuilder.Add("sortField", (int)searchParams.SortField);

			var url = queryBuilder.Build();

			Runtime.DebugWrite(url, color: ConsoleColor.Cyan);

			var result = GetApiData<List<CurseforgeInstanceInfo>>(url, out Pagination paginator);
			return new(result, paginator?.TotalCount ?? 1);
		}

		private string BuildCategoriesToQuery(IEnumerable<IProjectCategory> categories)
		{
			string ctrs = string.Join(",", categories.Select(x => x.Id != "-1"));
			return "[" + ctrs + "]";
		}

		public CatalogResult<CurseforgeAddonInfo> GetAddonsList(AddonType type, CurseforgeSearchParams searchParams)
		{
			/*
             https://api.curseforge.com/v1/mods/search?gameId=432&classId=12&sortOrder=desc&pageSize=10&index=0&gameVersion=1.20.1&categoryIds=%5B%5D&sortField=0&searchFilter=
             https://api.curseforge.com/v1/mods/search?gameId=432&classId=12&sortOrder=desc&pageSize=10&index=0&gameVersion=1.20.1&categoryIds=&sortField=0
             */

			var queryBuilder = new QueryApiBuilder("https://api.curseforge.com/v1/mods/search");

			queryBuilder.Add("gameId", "432");
			queryBuilder.Add("classId", (int)type);
			queryBuilder.Add("sortOrder", "desc");
			queryBuilder.Add("pageSize", searchParams.PageSize);
			queryBuilder.Add("index", searchParams.PageIndex * searchParams.PageSize);

			if (!string.IsNullOrWhiteSpace(searchParams.SearchFilter))
				queryBuilder.Add("searchFilter", WebUtility.UrlEncode(searchParams.SearchFilter));

			if (!string.IsNullOrWhiteSpace(searchParams.GameVersion))
				queryBuilder.Add("gameVersion", searchParams.GameVersion);

			if (searchParams.Categories.Count() > 0)
				queryBuilder.Add("categoryIds", BuildCategoriesToQuery(searchParams.Categories));

			queryBuilder.Add("sortField", (int)searchParams.SortField);

			if (type == AddonType.Mods)
			{
				queryBuilder.Add("modLoaderTypes", WebUtility.UrlEncode("[" + string.Join(",", searchParams.Modloaders) + "]"));
			}

			var url = queryBuilder.Build();

			Runtime.DebugWrite(url);

			var result = GetApiData<List<CurseforgeAddonInfo>>(url, out Pagination paginator);
			return new(result, paginator?.TotalCount ?? 1);
		}

		/// <summary>
		/// Возвращает файлы проекта
		/// </summary>
		/// <param name="projectId">id проекта</param>
		/// <param name="gameVersion">Версия игры</param>
		/// <param name="modloader">Модлоадер. Если его не нужно учитывать, то null</param>
		/// <returns></returns>
		public List<CurseforgeFileInfo> GetProjectFiles(string projectId, string gameVersion, Modloader? modloader)
		{
			string modloaderStr = "";
			if (modloader != null)
			{
				modloaderStr = "&modLoaderType=" + ((int)modloader);
			}

			// TODO: у курсфорджа ограничения на 50 файлов, поэтому нужный нам файл иногда может просто не найтись
			return GetApiData<List<CurseforgeFileInfo>>("https://api.curseforge.com/v1/mods/" + projectId + "/files?gameVersion=" + gameVersion + modloaderStr);
		}

		public List<CurseforgeFileInfo> GetProjectFiles(string projectId, string gameVersion, IEnumerable<Modloader> modloaders)
		{
			string modloaderStr = "";
			if (modloaders != null)
			{
				modloaderStr = "&modLoaderTypes=" + WebUtility.UrlEncode($"[\"{string.Join(",", modloaders.Select(x => (int)x))}\"]");
			}

			// TODO: у курсфорджа ограничения на 50 файлов, поэтому нужный нам файл иногда может просто не найтись
			return GetApiData<List<CurseforgeFileInfo>>("https://api.curseforge.com/v1/mods/" + projectId + "/files?gameVersion=" + gameVersion + modloaderStr);
		}

		public List<CurseforgeFileInfo> GetFilesFromFingerprints(List<string> fingerprint)
		{
			var jsonContent = "{\"fingerprints\": [" + string.Join(",", fingerprint) + "]}";

			var data = GetApiData<FingerprintSearchAnswer>("https://api.curseforge.com/v1/fingerprints/432", jsonContent);
			var result = new List<CurseforgeFileInfo>();
			if (data?.exactMatches != null)
			{
				foreach (var item in data?.exactMatches)
				{
					result.Add(item.file);
				}
			}

			return result;
		}

		public List<CurseforgeFileInfo> GetProjectFiles(string projectId)
		{
			return GetApiData<List<CurseforgeFileInfo>>("https://api.curseforge.com/v1/mods/" + projectId + "/files");
		}

		public CurseforgeFileInfo GetProjectFile(string projecrId, string fileId)
		{
			return GetApiData<CurseforgeFileInfo>("https://api.curseforge.com/v1/mods/" + projecrId + "/files/" + fileId);
		}

		public CurseforgeAddonInfo GetAddonInfo(string id)
		{
			return GetApiData<CurseforgeAddonInfo>("https://api.curseforge.com/v1/mods/" + id + "/");
		}

		public List<CurseforgeAddonInfo> GetAddonsInfo(string[] ids)
		{
			string jsonContent = "{\"modIds\": [" + string.Join(",", ids) + "]}";

			var data = GetApiData<List<CurseforgeAddonInfo>>("https://api.curseforge.com/v1/mods", jsonContent);
			return data ?? new List<CurseforgeAddonInfo>();
		}

		public CurseforgeInstanceInfo GetInstance(string id)
		{
			try
			{
				return GetApiData<CurseforgeInstanceInfo>("https://api.curseforge.com/v1/mods/" + id + "/");
			}
			catch
			{
				return new CurseforgeInstanceInfo();
			}
		}

		public string GetProjectChangelog(string projectID, string fileID)
		{
			//return ToServer.HttpGet("https://api.curseforge.com/v1/mods/" + projectID + "/files/" + fileID + "/changelog");
			// TODO: придумать как эту хуйню красиво сделать
			return "";
		}

		public string GetProjectDescription(string projectId)
		{
			try
			{
				string result = _toServer.HttpGet($"https://api.curseforge.com/v1/mods/{projectId}/description", new Dictionary<string, string>()
				{
					["x-api-key"] = Token
				});

				if (result == null) return string.Empty;

				var data = JsonConvert.DeserializeObject<DataContainer<string>>(result);
				return data?.Data ?? string.Empty;
			}
			catch
			{
				return string.Empty;
			}
		}

		public List<CurseforgeCategory> GetCategories(CfProjectType type)
		{
			List<CurseforgeCategory> categories = GetApiData<List<CurseforgeCategory>>("https://api.curseforge.com/v1/categories?gameId=432&classId=" + (int)type);
			categories.Insert(0, new CurseforgeCategory
			{
				Id = "-1",
				Name = "All",
				ClassId = ((int)type).ToString(),
				ParentCategoryId = ((int)type).ToString()
			});

			return categories;
		}
	}
}
