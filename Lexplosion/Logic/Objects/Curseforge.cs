using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Logic.Objects.Curseforge
{
    /// <summary>
    /// Описывает проект курсфорджа. Дочерние классы используются при декодировании Json
    /// </summary>
    public abstract class CurseforgeProjectInfo
    {
        public class GameVersion
        {
            public string gameVersion;
            public int projectFileId;
        }

        public class LatestFile
        {
            public long id;
            public List<string> gameVersion;
        }

        public class Author
        {
            public string name;
            public string url;
        }

        public class Attachment
        {
            public int id;
            public bool isDefault;
            public string thumbnailUrl;
            public string url;
        }

        public int id;
        public string name;
        public List<LatestFile> latestFiles;
        public string summary;
        public float downloadCount { get; set; }
        public string dateModified;
        public string websiteUrl;
        public List<Attachment> attachments;
        public List<Category> categories;
        public List<Author> authors;
    }

    /// <summary>
    /// Описывает модпак с курсфорджа. Используются при декодировании Json
    /// </summary>
    public class CurseforgeInstanceInfo : CurseforgeProjectInfo
    {
        public List<GameVersion> gameVersionLatestFiles;
        public ModloaderType Modloader;
    }

    /// <summary>
    /// Описывает мод с курсфорджа. Используются при декодировании Json
    /// </summary>
    public class CurseforgeModInfo : CurseforgeProjectInfo
    {
        public class GameVersionMod : GameVersion
        {
            public int modLoader;
        }

        public List<GameVersionMod> gameVersionLatestFiles;
        public ModloaderType Modloader;
    }

    class CurseforgeFileInfo
    {
        public int id;
        public string downloadUrl;
        public string fileName;
    }
}
