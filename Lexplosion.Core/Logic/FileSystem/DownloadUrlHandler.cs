using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using Lexplosion.Global;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Tools;

namespace Lexplosion.Logic.FileSystem
{
	internal class DownloadUrlHandler
	{
		private string _libraiesMirrorUrl = LaunсherSettings.URL.MirrorUrl;

		private bool _sourceChanged = false;
		private object _locker = new object();

		private Dictionary<string, (string, string)[]> _bmclApiUrls = new()
		{
			["launchermeta.mojang.com"] = [("://bmclapi2.bangbang93.com", "://launchermeta.mojang.com")],
			["launcher.mojang.com"] = [("://bmclapi2.bangbang93.com", "://launcher.mojang.com")],
			["piston-data.mojang.com"] = [("://bmclapi2.bangbang93.com", "://piston-data.mojang.com")],
			["resources.download.minecraft.net"] = [("://bmclapi2.bangbang93.com/assets", "://resources.download.minecraft.net")],
			["libraries.minecraft.net"] = [("://bmclapi2.bangbang93.com/maven", "://libraries.minecraft.net")],
			["files.minecraftforge.net/maven"] = [("://bmclapi2.bangbang93.com/maven", "://files.minecraftforge.net/maven")],
			["meta.fabricmc.net"] = [("://bmclapi2.bangbang93.com/fabric-meta", "://meta.fabricmc.net")],
			["maven.fabricmc.net"] = [("://bmclapi2.bangbang93.com/maven", "://maven.fabricmc.net")],
			["maven.neoforged.net"] =
			[
				("://bmclapi2.bangbang93.com/maven/net/neoforged/forge", "://maven.neoforged.net/releases/net/neoforged/forge"),
				("://bmclapi2.bangbang93.com/maven/net/neoforged/neoforge", "://maven.neoforged.net/releases/net/neoforged/neoforge")
			]
		};

		/// <summary>
		/// Ключ - домен для замены, значение - номер смены.
		/// Если в коллекции нет ключа или ключ есть и значение 0, значит возвращаем стандартный url.
		/// Если Ключ в коллекции есть и значение 1, то вместо этого домена скачивать нужно с bmclApi
		/// Если ключ в коллекции есть и значение 2, то вместо этого домена нужно качать с нашего зеркала
		/// Если ключ в коллекции есть и значение больше 2, то возращаемся к стандартному url, ибо со весми другими источниками тоже возникли проблемы
		/// </summary>
		private Dictionary<string, int> _domainsToReplace = new();

		public string GenerateFileUrl(string baseUrl, out int shiftNumber)
		{
			string addr = baseUrl;

			shiftNumber = 0;
			lock (_locker)
			{
				if (!_sourceChanged) return addr;

				string domain = (new Uri(addr).Host);
				if (!_domainsToReplace.TryGetValue(domain, out shiftNumber)) return addr;

				if (shiftNumber == 1) // используем bmclApi
				{
					if (!_bmclApiUrls.TryGetValue(domain, out var replaceData)) return addr;

					foreach (var item in replaceData)
					{
						if (addr.Contains(item.Item2)) return addr.Replace(item.Item2, item.Item1);
					}
				}
				else if (shiftNumber == 2) // используем наше зеркало
				{
					addr = addr.ReplaceFirst("https://", "").ReplaceFirst("http://", "");
					return _libraiesMirrorUrl + addr;
				}

				return addr;
			}
		}

		public void ErrorOccured(string fileUrl, int currentShiftNumber)
		{
			lock (_locker)
			{
				_sourceChanged = true;

				var url = new Uri(fileUrl);
				if (_domainsToReplace.TryGetValue(url.Host, out int shiftNumber))
				{
					if (shiftNumber > currentShiftNumber) return;
					_domainsToReplace[url.Host]++;

					Runtime.DebugWrite($"Shift for {fileUrl}, last shift: {shiftNumber}");
					return;
				}

				Runtime.DebugWrite($"First for {fileUrl}");
				_domainsToReplace[url.Host] = 1;
			}
		}
	}
}
