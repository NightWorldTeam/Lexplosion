using Lexplosion;
using Lexplosion.Logic.Management.Addons;
using Lexplosion.Logic.Objects.Modrinth;
using Lexplosion.UI.WPF.Mvvm.Models.AddonsRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Lexplosion.UI.WPF.Services.Addons
{
    internal static class AddonDetailsDataLoader
    {
        internal static AddonPageData FetchPage(InstanceAddon addon)
        {
            var summary = AddonPageData.FromAddon(addon);
            string description = summary.Description;
            IEnumerable<string> images = Array.Empty<string>();
            IEnumerable<string> minecraftVersions = summary.MinecraftVersions;
            IEnumerable<string> modloaders = summary.Modloaders;

            if (addon.Source == ProjectSource.Modrinth)
            {
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
                        try
                        {
                            project = Runtime.ServicesContainer.MdApi.GetProject(slug);
                        }
                        catch (Exception) { }

                        if (project != null)
                        {
                            if (!string.IsNullOrWhiteSpace(project.FullDescription))
                            {
                                description = project.FullDescription;
                            }

                            if (project.GameVersions?.Count > 0)
                            {
                                minecraftVersions = project.GameVersions;
                            }

                            if (project.Loaders?.Count > 0)
                            {
                                modloaders = project.Loaders;
                            }

                            images = project.Images == null ? Array.Empty<string>() :
                                project.Images.Select(item => item != null && item.TryGetValue("url", out var image) ? image : null)
                                    .Where(image => Uri.TryCreate(image, UriKind.Absolute, out var parsed) && parsed.Scheme == Uri.UriSchemeHttps)
                                    .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
                        }
                    }
                }
            }
            else if (addon.Source == ProjectSource.Curseforge)
            {
                try
                {
                    string fullDescription = addon.GetFullDescription();
                    if (!string.IsNullOrWhiteSpace(fullDescription))
                    {
                        description = fullDescription;
                    }
                }
                catch (Exception) { }
            }

            return new AddonPageData(description, images, minecraftVersions, modloaders, summary.Categories);
        }
    }
}
