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
			["launchermeta.mojang.com"] = [("https://bmclapi2.bangbang93.com", "https://launchermeta.mojang.com")],
			["launcher.mojang.com"] = [("https://bmclapi2.bangbang93.com", "https://launcher.mojang.com")],
			["piston-data.mojang.com"] = [("https://bmclapi2.bangbang93.com", "https://piston-data.mojang.com")],
			["resources.download.minecraft.net"] = [("https://bmclapi2.bangbang93.com/assets", "https://resources.download.minecraft.net")],
			["libraries.minecraft.net"] = [("https://bmclapi2.bangbang93.com/maven", "https://libraries.minecraft.net")],
			["files.minecraftforge.net/maven"] = [("https://bmclapi2.bangbang93.com/maven", "https://files.minecraftforge.net/maven")],
            ["maven.minecraftforge.net/maven"] = [("https://bmclapi2.bangbang93.com/maven", "https://maven.minecraftforge.net")],
            ["meta.fabricmc.net"] = [("https://bmclapi2.bangbang93.com/fabric-meta", "https://meta.fabricmc.net")],
			["maven.fabricmc.net"] = [("https://bmclapi2.bangbang93.com/maven", "https://maven.fabricmc.net")],
			["maven.neoforged.net"] =
			[
                ("https://bmclapi2.bangbang93.com/maven/net/neoforged/forge", "https://maven.neoforged.net/releases/net/neoforged/forge"),
				("https://bmclapi2.bangbang93.com/maven/net/neoforged/neoforge", "https://maven.neoforged.net/releases/net/neoforged/neoforge"),
                ("https://bmclapi2.bangbang93.com/maven", "https://maven.neoforged.net")
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

				if (shiftNumber == 2) // используем bmclApi
				{
					// в bmclApi ссылки нет, возвращаем оригинальный url
					if (!_bmclApiUrls.TryGetValue(domain, out var replaceData)) 
					{
                        //addr = addr.ReplaceFirst("https://", "").ReplaceFirst("http://", "");
                        //return _libraiesMirrorUrl + addr;
                        return addr;
                    }

					foreach (var item in replaceData)
					{
						if (addr.Contains(item.Item2)) return addr.Replace(item.Item2, item.Item1);
					}
				}
				else if (shiftNumber == 1) // используем наше зеркало
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
