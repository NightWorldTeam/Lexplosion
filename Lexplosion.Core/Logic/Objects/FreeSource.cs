using Lexplosion.Logic.Objects.CommonClientData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Lexplosion.Logic.Objects.FreeSource
{
	public class InstanceManifest : ArchivedClientData
	{
		public InstalledAddonsFormat Addons;
	}

	public class FreeSourcePlatformData : InstancePlatformData
	{
		[JsonIgnore]
		public bool SourceUrlIsExists { get; private set; } = false;
		[JsonIgnore]
		private string _sourceUrl;

		public string sourceUrl
		{
			get => _sourceUrl;
			set
			{
				_sourceUrl = value;
				SourceUrlIsExists = !string.IsNullOrWhiteSpace(value);
			}
		}
		public string sourceId;

		public bool IsValid()
		{
			return !string.IsNullOrWhiteSpace(id) && (SourceUrlIsExists || !string.IsNullOrWhiteSpace(sourceId));
		}
	}

	public class FileDesc
	{
		[JsonProperty("sha512")]
		public string Sha512;

		[JsonProperty("size")]
		public long Size;

		public FileDesc(string sha512, long size)
		{
			Sha512 = sha512;
			Size = size;
		}

		public override int GetHashCode()
		{
			if (Sha512 == null) return (int)Size;
			return Sha512.GetHashCode() ^ RotateLeft((int)Size);
		}

		private int RotateLeft(int value)
		{
			return (value << 16) | (value >> 16);
		}

		public override bool Equals(object obj)
		{
			if (obj is FileDesc value)
			{
				return Sha512 == value.Sha512 && Size == value.Size;
			}

			return false;
		}

		public override string ToString()
		{
			return "(sha512: " + Sha512 + ", size: " + Size + ")";
		}
	}

	public class SourceMap
	{
		private string _baseUrl;
		[JsonIgnore]
		public string BaseUrl
		{
			get => _baseUrl;
			set
			{
				_baseUrl = value.TrimEnd('/');

				ModpacksListUrl = ValidateUrl(ModpacksListUrl);
				ModpackManifestUrl = ValidateUrl(ModpackManifestUrl);
				ModpackVersionsListUrl = ValidateUrl(ModpackVersionsListUrl);
				ModpackVersionManifestUrl = ValidateUrl(ModpackVersionManifestUrl);
			}
		}

		[JsonProperty("modpacksListUrl")]
		public string ModpacksListUrl { get; set; }

		[JsonProperty("modpackManifestUrl")]
		public string ModpackManifestUrl { get; set; }

		[JsonProperty("modpackVersionsListUrl")]
		public string ModpackVersionsListUrl { get; set; }

		[JsonProperty("modpackVersionManifestUrl")]
		public string ModpackVersionManifestUrl { get; set; }

		public string GetModpackVersionsListUrl(string modpackId)
		{
			return ModpackVersionsListUrl?.Replace("${modpackId}", modpackId);
		}

		public string GetModpackManifestUrl(string modpackId)
		{
			return ModpackManifestUrl?.Replace("${modpackId}", modpackId);
		}

		public string GetModpackVersionManifestUrl(string modpackId, string modpackVersion)
		{
			if (ModpackVersionManifestUrl == null) return null;
			return ModpackVersionManifestUrl.Replace("${modpackId}", modpackId).Replace("${modpackVersion}", modpackVersion);
		}

		private string ValidateUrl(string url)
		{
			if (string.IsNullOrWhiteSpace(url)) return null;
			if (url.StartsWith("https://") || url.StartsWith("http://")) return url;

			string _url = _baseUrl;
			if (!url.StartsWith("/")) _url += "/";

			return _url + url;
		}
	}

	public class SourceManifest
	{
		public SourceMap sourceMap;
	}

	public class ModpackVersion
	{
		[JsonProperty("downloadUrl")]
		public string DownloadUrl;

		[JsonProperty("loadDate")]
		public long LoadDate;

		[JsonProperty("modpackId")]
		public string ModpackId;

		[JsonProperty("version")]
		public string Version;
	}

	public class MidpackVersionsList
	{
		[JsonProperty("latestVersion")]
		public string LatestVersion;

		[JsonProperty("allVersions")]
		public Dictionary<string, ModpackVersion> AllVersions;
	}

	public class ModpackManifest
	{
		[JsonProperty("name")]
		public string Name;

		[JsonProperty("updateDate")]
		public long UpdateDate;

		[JsonProperty("description")]
		public string Description;

		[JsonProperty("Summary")]
		public string Summary;

		[JsonProperty("gameVersion")]
		public string GameVersion;

		private ClientType _core;

		[JsonProperty("core")]
		public ClientType Core
		{
			get => _core;
			set
			{
				if (!Enum.IsDefined(typeof(ClientType), (int)value))
				{
					_core = ClientType.Vanilla;
				}
				else
				{
					_core = value;
				}
			}
		}

		[JsonProperty("logoUrl")]
		public string LogoUrl;

		[JsonProperty("images")]
		public List<string> Images;
	}

}
