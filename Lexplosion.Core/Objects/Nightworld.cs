using System.Collections.Generic;
using Lexplosion.Logic.Objects.CommonClientData;
using Newtonsoft.Json;

namespace Lexplosion.Logic.Objects.Nightworld
{
    class LocalNightWorldManifest
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

    class NightWorldManifest : LocalNightWorldManifest
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

    class PlayerData
    {
        public string Nickname;
        public string AvatarUrl;
        public string ProfileUrl;
    }

    class NightWorldCategory : CategoryBase
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
}
