using Newtonsoft.Json;
using System.Collections.Generic;
using System.Globalization;

namespace Lexplosion.Logic.Objects.Modrinth
{
    public enum ModrinthProjectType
    {
        Unknown,
        Mod,
        Modpack,
        Resourcepack,
        Shader
    }

    public abstract class ModrinthProject
    {
        [JsonProperty("title")]
        public string Title;

        [JsonProperty("loaders")]
        public List<string> Loaders;

        [JsonProperty("project_type")]
        public string Type;//mod, modpack, resourcepack, shader
    }

    public class ModrinthCtalogUnit : ModrinthProject
    {
        [JsonProperty("project_id")]
        public string ProjectId;

        [JsonProperty("slug")]
        public string Slug;

        [JsonProperty("author")]
        public string Author;

        [JsonProperty("categories")]
        public List<string> Categories;

        [JsonProperty("updated")]
        public string Updated;

        [JsonProperty("versions")]
        public List<string> GameVersions;

        [JsonProperty("description")]
        public string Summary;

        [JsonProperty("downloads")]
        public int Downloads;

        [JsonProperty("icon_url")]
        public string LogoUrl;
    }

    public class ModrinthProjectInfo : ModrinthProject
    {
        [JsonProperty("id")]
        public string ProjectId;

        [JsonProperty("description")]
        public string Summary;

        [JsonProperty("downloads")]
        public int Downloads;

        [JsonProperty("categories")]
        public List<string> Categories;

        [JsonProperty("game_versions")]
        public List<string> GameVersions;

        [JsonProperty("slug")]
        public string Slug;

        [JsonProperty("versions")]
        public List<string> Versions;

        [JsonProperty("icon_url")]
        public string LogoUrl;

        [JsonProperty("updated")]
        public string Updated;

        [JsonProperty("gallery")]
        public List<Dictionary<string, string>> Images;

        public string WebsiteUrl
        {
            get => "https://modrinth.com/modpack/" + Slug;
        }
    }

    public class ModrinthProjectFile
    {
        public class File
        {
            [JsonProperty("url")]
            public string Url;

            [JsonProperty("filename")]
            public string Filename;

            [JsonProperty("size")]
            public int Size;
        }

        public class Dependencie
        {
            [JsonProperty("version_id")]
            public string VersionId;

            [JsonProperty("project_id")]
            public string ProjectId;

            [JsonProperty("dependency_type")]
            public string DependencyType;
        }

        [JsonProperty("project_id")]
        public string ProjectId;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("id")]
        public string FileId;

        [JsonProperty("files")]
        public List<File> Files;

        [JsonProperty("dependencies")]
        public List<Dependencie> Dependencies;

        [JsonProperty("date_published")]
        public string Date;

        [JsonProperty("version_type")]
        public string Status;
    }

    public class InstanceManifest
    {
        public class FileData
        {
            public string path;
            public int fileSize;
            public List<string> downloads;
            public Dictionary<string, string> hashes;
        }

        public string name;
        public string versionId;
        public List<FileData> files;
        public Dictionary<string, string> dependencies;
    }

    public class ModrinthCategory : CategoryBase
    {
        [JsonProperty("name")]
        public override string Id { get; set; }

        [JsonIgnore]
        public override string Name
        {
            get
            {
                return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Id.Replace("-", " "));
            }
            set { }
        }

        [JsonProperty("project_type")]
        public override string ClassId { get; set; }


        [JsonProperty("header")]
        public override string ParentCategoryId { get; set; }
    }
}
