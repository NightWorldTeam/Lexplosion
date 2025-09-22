using System.Collections.Generic;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Logic.Objects.Nightworld;
using Newtonsoft.Json;

namespace Lexplosion.Logic.Network
{

	/// <summary>
	/// Этот класс просто расшряет LibInfo чтобы параметр OS проверить еще при получении данных и потом его отсевить
	/// </summary>
	class DataLibInfo : LibInfo
	{
		public List<string> os;

		[JsonIgnore]
		public LibInfo GetLibInfo
		{
			get
			{
				return new LibInfo
				{
					notArchived = this.notArchived,
					url = this.url,
					obtainingMethod = this.obtainingMethod,
					isNative = this.isNative,
					activationConditions = this.activationConditions,
					notLaunch = this.notLaunch
				};
			}
		}
	}

	public interface ProtectedManifest
	{
		public string code { get; set; }
		public string str { get; set; }
	}

	/// <summary>
	/// Этот класс нужен для декодирования json в методе GetVersionManifest в классах ToServer и NightWorldApi
	/// </summary>
	class ProtectedVersionManifest : VersionManifest, ProtectedManifest
	{
		public new Dictionary<string, DataLibInfo> libraries;
		public string code { get; set; }
		public string str { get; set; }
	}

	class ProtectedInstallerManifest : AdditionalInstallerManifest, ProtectedManifest
	{
		public new Dictionary<string, DataLibInfo> libraries;
		public string code { get; set; }
		public string str { get; set; }
	}

	/// <summary>
	/// Результат инцеста(зачеркнуто) авторизации.
	/// </summary>
	public class AuthResult
	{
		public AuthCode Status;
		public string Login;
		public string UUID;
		public string AccesToken;
	}

	public class NwAuthResult : AuthResult
	{
		public int BaseStatus;
		public string SessionToken;
		public string AccessID;
		public long LastNewsId;
		public NwUserBanner Banner;
    }

	public class MojangAuthResult : AuthResult
	{
		public string ClientToken;
	}
}
