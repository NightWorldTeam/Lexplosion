using Lexplosion;
using Lexplosion.Logic.Management.Addons;
using Lexplosion.Logic.Objects.Modrinth;
using System;
using System.Linq;
using System.Threading;

namespace Lexplosion.UI.WPF.Services.Addons
{
    // Source-specific data is loaded off the WPF UI thread.
    internal sealed class AddonPageData
    {
        public string Description { get; set; }
        public string[] Images { get; set; } = Array.Empty<string>();
        public string[] MinecraftVersions { get; set; } = Array.Empty<string>();
        public string[] Modloaders { get; set; } = Array.Empty<string>();
    }

    internal static class AddonDetailsDataLoader
    {
        internal static AddonPageData FetchPage(InstanceAddon addon)
        {
            var data = new AddonPageData
            {
                Description = addon.Description ?? string.Empty,
                MinecraftVersions = string.IsNullOrWhiteSpace(addon.GameVersion)
                    ? Array.Empty<string>() : new[] { addon.GameVersion },
                Modloaders = addon.DisplayModloaders?.ToArray() ?? Array.Empty<string>()
            };
            if (addon.Source == ProjectSource.Modrinth)
            {
                // InstanceAddon already loads WebsiteUrl on its worker thread.
                // A catalog card may be opened before that background work completes.
                string url = addon.WebsiteUrl;
                for (int attempt = 0; attempt < 30 && string.IsNullOrWhiteSpace(url); attempt++)
                {
                    Thread.Sleep(150);
                    url = addon.WebsiteUrl;
                }
                if (Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                    (uri.Host.Equals("modrinth.com", StringComparison.OrdinalIgnoreCase) ||
                     uri.Host.Equals("www.modrinth.com", StringComparison.OrdinalIgnoreCase)))
                {
                    string slug = uri.AbsolutePath.Trim('/').Split('/').LastOrDefault();
                    if (!string.IsNullOrWhiteSpace(slug))
                    {
                        ModrinthProjectInfo project = null;
                        try { project = Runtime.ServicesContainer.MdApi.GetProject(slug); }
                        catch (Exception) { /* Show the catalog summary if the API is offline. */ }
                        if (project != null)
                        {
                            data.Description = string.IsNullOrWhiteSpace(project.FullDescription)
                                ? data.Description : project.FullDescription;
                            if (project.GameVersions?.Count > 0)
                                data.MinecraftVersions = project.GameVersions.ToArray();
                            if (project.Loaders?.Count > 0)
                                data.Modloaders = project.Loaders.ToArray();
                            data.Images = project.Images == null ? Array.Empty<string>() :
                                project.Images.Select(item => item != null && item.TryGetValue("url", out var image)
                                    ? image : null)
                                .Where(image => Uri.TryCreate(image, UriKind.Absolute, out var parsed) &&
                                    parsed.Scheme == Uri.UriSchemeHttps)
                                .Distinct(StringComparer.OrdinalIgnoreCase)
                                .ToArray();
                        }
                    }
                }
            }
            else if (addon.Source == ProjectSource.Curseforge)
            {
                try
                {
                    string description = addon.GetFullDescription();
                    if (!string.IsNullOrWhiteSpace(description)) data.Description = description;
                }
                catch (Exception) { /* Fall back to the short catalog description. */ }
            }
            return data;
        }

    }
}
