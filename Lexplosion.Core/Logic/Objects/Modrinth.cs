﻿using Newtonsoft.Json;
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
        public string Type; //mod, modpack, resourcepack, shader

        [JsonProperty("updated")]
        public string Updated;

        [JsonProperty("icon_url")]
        public string LogoUrl;
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

        [JsonProperty("versions")]
        public List<string> GameVersions;

        [JsonProperty("description")]
        public string Summary;

        [JsonProperty("downloads")]
        public int Downloads;

        [JsonProperty("date_modified")]
        public new string Updated;        
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

        [JsonProperty("gallery")]
        public List<Dictionary<string, string>> Images;

        [JsonProperty("team")]
        public string Team;

        [JsonIgnore]
        public string Author = null;

        public string WebsiteUrl
        {
            get => "https://modrinth.com/modpack/" + Slug;
        }

        public ModrinthProjectInfo() { }
        public ModrinthProjectInfo(ModrinthCtalogUnit catalogUnit)
        {
            ProjectId = catalogUnit.ProjectId;
            Summary = catalogUnit.Summary;
            Downloads = catalogUnit.Downloads;
            GameVersions = catalogUnit.GameVersions;
            Slug = catalogUnit.Slug;
            Versions = null;
            LogoUrl = catalogUnit.LogoUrl;
            Updated = catalogUnit.Updated;
            Author = catalogUnit.Author;
            Team = null;
            Images = null;
            Title = catalogUnit.Title;
            Loaders = catalogUnit.Loaders;
            Type = catalogUnit.Type;
        }
    }

    public class ProjectFileHashes
    {
        [JsonProperty("sha512")]
        public string Sha512;

        [JsonProperty("sha1")]
        public string Sha1;
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

            [JsonProperty("hashes")]
            public ProjectFileHashes Hashes;
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

    public class ModrinthUser
    {
        [JsonProperty("username")]
        public string Username;
        [JsonProperty("name")]
        public string Name;
        [JsonProperty("avatar_url")]
        public string Avatar_url;
    }

    public class ModrinthTeam
    {
        [JsonProperty("team_id")]
        public string TeamId;

        [JsonProperty("user")]
        public ModrinthUser User;
    }
}
