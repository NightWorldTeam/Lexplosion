using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Logic.Objects
{
    public abstract class ModrinthProject
    {
        [JsonProperty("project_id")]
        public string ProjectId;

        [JsonProperty("loaders")]
        public List<string> Loaders;

        [JsonProperty("project_type")]
        public string ProjectType;
    }

    public class ModrinthCtalogUnit : ModrinthProject
    {
        [JsonProperty("slug")]
        public string Slug;

        [JsonProperty("author")]
        public string Author;

        [JsonProperty("categories")]
        public List<string> Categories;

        [JsonProperty("title")]
        public string Title;

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
    }

    public class ModrinthAddonInfo : ModrinthProject
    {
        [JsonProperty("game_versions")]
        public List<string> GameVersions;

        [JsonProperty("versions")]
        public List<string> Versions;
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
    }
}
