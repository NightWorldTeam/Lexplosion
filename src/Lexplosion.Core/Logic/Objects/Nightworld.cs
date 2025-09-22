using System;
using System.Collections.Generic;
using Lexplosion.Global;
using Lexplosion.Logic.Objects.CommonClientData;
using Newtonsoft.Json;

namespace Lexplosion.Logic.Objects.Nightworld
{
    public class LocalNightWorldManifest
    {
        public class WithFolder
        {
            public Dictionary<string, FileInfo> objects;
            public List<string> oldFiles;
            public bool security;
            public long folderVersion;
        }

        public Dictionary<string, WithFolder> data = new Dictionary<string, WithFolder>();
        public bool CustomVersion;
    }

    public class NightWorldManifest : LocalNightWorldManifest
    {
        public class Version
        {
            public string modloaderVersion;
            public ClientType modloaderType;
            public string gameVersion;
            public bool security;
        }

        public Version version;
    }

    public class PlayerData
    {
        public string Nickname;
        public string AvatarUrl;
        public string ProfileUrl;
    }

    public class NightWorldCategory : CategoryBase
    {
        [JsonProperty("categoryId")]
        public override string Id { get; set; }
        [JsonProperty("name")]
        public override string Name { get; set; }
        /// <summary>
        /// Id типа аддона.
        /// </summary>
        public override string ClassId { get; set; }//TODO: на нулл проверку намутить
        /// <summary>
        /// Id родительской категории, 
        /// Если не содержит родительскую категорию, содержит classId
        /// </summary>
        public override string ParentCategoryId { get; set; } //TODO: на нулл проверку намутить
    }

    public class NwUser
    {
        [JsonProperty("status")]
        public ActivityStatus ActivityStatus;
        /// <summary>
        /// Имя клиента, в который сейчас играет пользователь. Может быть null.
        /// </summary>
        /// 
        [JsonProperty("gameClientName")]
        public string GameClientName;

        [JsonProperty("login")]
        public string Login;

        [JsonIgnore]
        public string AvatarUrl
        {
            get => LaunсherSettings.URL.Base + "requestProcessing/getUserImage.php?user_login=" + Login;
        }

        [JsonProperty("banner")]
        public NwUserBanner Banner { get; set; }
    }

	public sealed class NwUserBannerColors
	{
		[JsonProperty("primaryForeColor")]
		public uint? PrimaryForeColor { get; set; } = null;

		[JsonProperty("secondaryForeColor")]
		public uint? SecondaryForeColor { get; set; } = null;

		[JsonProperty("primaryColor")]
		public uint? PrimaryColor { get; set; } = null;

		[JsonProperty("secondaryColor")]
		public uint? SecondaryColor { get; set; } = null;

		// хз зачем возможно не понадобиться
		[JsonProperty("activityColor")]
		public uint? ActivityColor { get; set; } = null;

		[JsonProperty("separateColor")]
		public uint? SeparateColor { get; set; } = null;
	}


	public sealed class NwUserBanner
    {
        [JsonProperty("url")]
        public string Url { get; set; }

		[JsonProperty("colors")]
		public NwUserBannerColors Colors { get; set; }


		[JsonProperty("primaryForeColor")]
		public uint? NameColor { get; set; } = null;


		[JsonProperty("secondaryForeColor")]
        public uint? AtivityColor { get; set; } = null;

		[JsonProperty("primaryColor")]
        public uint? MoreButtonColor { get; set; } = null;

		[JsonProperty("secondaryColor")]
        public uint? StatusIndecatorBorderColor { get; set; } = null;

		// хз зачем возможно не понадобиться
		[JsonProperty("activityColor")]
		public uint? ActivityColor { get; set; } = null;

		[JsonProperty("separateColor")]
        public uint? MoreButtonIconColor { get; set; } = null;
    }

    public struct UsersCatalogPage
    {
        /// <summary>
        /// Список найденных пользователей
        /// </summary>
        [JsonProperty("data")]
        public List<NwUser> Data;
        /// <summary>
        /// true - если существует следующая страница, false - если нет
        /// </summary>
        [JsonProperty("nextPage")]
        public bool NextPage { get; set; }

        [JsonProperty("pagesCount")]
        public int PagesCount { get; set; }
    }

    public struct FriendRequests
    {
        /// <summary>
        /// Входящией заявки в друзья
        /// </summary>
        [JsonProperty("incoming")]
        public List<NwUser> Incoming;
        /// <summary>
        /// Исходящие заявки в друзья
        /// </summary>
        [JsonProperty("outgoing")]
        public List<NwUser> Outgoing;
    }

    public class NewsModel
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Content { get; set; }

        [JsonIgnore]
        public DateTime CreationDate { get; private set; } = DateTime.Today;

		private long _dateUnix;
		[JsonProperty("Date")]
		public long DateUnix
        {
            get => _dateUnix; 
			set
            {
				_dateUnix = value;
				DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                CreationDate = dateTime.AddSeconds(value).ToLocalTime();
            }
        }

	}
}
