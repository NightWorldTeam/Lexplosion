using Lexplosion.Logic.Management;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Lexplosion.Logic.Objects
{
    public class MinecraftServerInstance
    {
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


        #endregion Properties


        [JsonConstructor]
        public MinecraftServerInstance(string address, string name, string description, string id, List<Tag> tags, string gameVersion, string bgUrl, string iconUrl, List<string> imagesUrls)
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
        }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Name)
                && !string.IsNullOrWhiteSpace(Id)
                && !string.IsNullOrWhiteSpace(Address)
                && !Address.Contains(" ")
                && MinecraftVersion.IsValidRelease(GameVersion);
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
