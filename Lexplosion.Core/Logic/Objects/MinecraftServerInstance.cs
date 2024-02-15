using Lexplosion.Logic.Management;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Lexplosion.Logic.Objects
{
    public class MinecraftServerInstance : VMBase
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public class Tag
        {
            [JsonProperty("name")]
            public string Name { get; }
            [JsonProperty("id")]
            public string Id { get; }

            [JsonConstructor]
            public Tag(string name, string id)
            {
                Name = name;
                Id = id;
            }
        }

        public class ModpackData
        {
            /// <summary>
            /// Нужен только если только сборка из кастомного источника. Указывет id источника. 
            /// Если курсфордж или модринф, то тут будет null
            /// </summary>
            [JsonProperty("sourceId")]
            public string SourceId;

            /// <summary>
            /// id модпака
            /// </summary>
            [JsonProperty("modpackId")]
            public string ModpackId;

            /// <summary>
            /// версия модпака
            /// </summary>
            [JsonProperty("version")]
            public string Version;

            public bool IsValid()
            {
                return !string.IsNullOrWhiteSpace(SourceId) && !string.IsNullOrWhiteSpace(ModpackId);
            }
        }


        #region Properties


        [JsonProperty("address")]
        public string Address { get; }
        [JsonProperty("name")]
        public string Name { get; }
        [JsonProperty("description")]
        public string Description { get; }
        [JsonProperty("id")]
        public string Id { get; }
        [JsonProperty("tags")]
        public List<Tag> Tags { get; }
        [JsonProperty("gameVersion")]
        public string GameVersion { get; }
        [JsonProperty("bgUrl")]
        public string BgUrl { get; }
        [JsonProperty("iconUrl")]
        public string IconUrl { get; }
        [JsonProperty("imagesUrls")]
        public List<string> ImagesUrls { get; }

        [JsonProperty("instanceId")]
        public string InstanceId { get; }
        [JsonProperty("instanceName")]
        public string InstanceName { get; }

        private InstanceSource _instanceSource;

        [JsonProperty("instanceSource")]
        public InstanceSource InstanceSource
        {
            get => _instanceSource;
            set
            {
                if (value == InstanceSource.None)
                {
                    _instanceSource = InstanceSource.Local;
                    return;
                }

                _instanceSource = Enum.IsDefined(typeof(InstanceSource), (int)value) ? value : InstanceSource.Local;
            }
        }

        /// <summary>
        /// Если сервер ванильный, тут будет null
        /// </summary>
        [JsonProperty("modpackInfo")]
        public ModpackData ModpackInfo { get; set; }

        // not loaded = -2
        private int _onlineCount = -2;
        [JsonIgnore]
        public int OnlineCount
        {
            get => _onlineCount; set
            {
                _onlineCount = value;
                IsOnline = _onlineCount > -1;
                OnPropertyChanged();
            }
        }

        private bool _isOnline;
        [JsonIgnore]
        public bool IsOnline
        {
            get => _isOnline; set
            {
                _isOnline = value;
                OnPropertyChanged();
            }
        }


        #endregion Properties


        [JsonConstructor]
        public MinecraftServerInstance(string address, string name, string description, string id, List<Tag> tags, string gameVersion, string bgUrl, string iconUrl, List<string> imagesUrls,
            string instanceId, string instanceName, InstanceSource instanceSource)
        {
            Address = address;
            Name = name;
            Description = description;
            Id = id;
            Tags = tags;
            GameVersion = gameVersion;
            BgUrl = bgUrl;
            IconUrl = iconUrl;
            ImagesUrls = imagesUrls;
            InstanceId = instanceId;
            InstanceName = instanceName;
            InstanceSource = instanceSource;
        }


        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Name)
                && !string.IsNullOrWhiteSpace(Id)
                && !string.IsNullOrWhiteSpace(Address)
                && !Address.Contains(" ")
                && MinecraftVersion.IsValidRelease(GameVersion)
                && (InstanceSource == InstanceSource.Local || (ModpackInfo != null && ModpackInfo.IsValid()));
        }
    }

    public class McServerOnlineData
    {
        public class PalyersCount
        {
            [JsonProperty("online")]
            public int Online;
            [JsonProperty("max")]
            public int Max;
        }

        [JsonProperty("players")]
        public PalyersCount Players;
    }
}
