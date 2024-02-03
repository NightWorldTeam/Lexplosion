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
            public string Name;
            [JsonProperty("id")]
            public string Id;
        }

        [JsonProperty("address")]
        public string Address;
        [JsonProperty("name")]
        public string Name;
        [JsonProperty("description")]
        public string Description;
        [JsonProperty("id")]
        public string Id;
        [JsonProperty("tags")]
        public List<Tag> Tags;
        [JsonProperty("gameVersion")]
        public string GameVersion;
        [JsonProperty("bgUrl")]
        public string BgUrl;
        [JsonProperty("iconUrl")]
        public string IconUrl;
        [JsonProperty("imagesUrls")]
        public List<string> ImagesUrls;

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Name)
                && !string.IsNullOrWhiteSpace(Id)
                && !string.IsNullOrWhiteSpace(Address)
                && !Address.Contains(" ")
                && MinecraftVersion.IsValidRelease(GameVersion);
        }
    }
}
