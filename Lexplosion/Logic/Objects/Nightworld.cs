using System.Collections.Generic;
using Lexplosion.Logic.Objects.CommonClientData;

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
            public ModloaderType modloaderType;
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
}
