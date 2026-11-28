using Lexplosion;
using Lexplosion.Logic.Management.Addons;
using Lexplosion.UI.WPF.Mvvm.Models.AddonsRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lexplosion.UI.WPF.Services.Addons
{
    internal static class AddonDetailsDataLoader
    {
        internal static async Task<AddonPageData> FetchPageAsync(InstanceAddon addon)
        {
            var summary = AddonPageData.FromAddon(addon);
            string description = summary.Description;
            IEnumerable<string> images = Array.Empty<string>();
            IEnumerable<string> minecraftVersions = string.IsNullOrWhiteSpace(addon.GameVersion) ? Array.Empty<string>() : new[] { addon.GameVersion };
            IEnumerable<string> modloaders = addon.DisplayModloaders;
            IEnumerable<string> categories = addon.Categories?.Select(category => category.Name);

            if (addon.Source == ProjectSource.Modrinth)
            {
                var project = await addon.GetModrinthProjectInfoAsync().ConfigureAwait(false);
                if (project == null || string.IsNullOrWhiteSpace(project.ProjectId))
                {
                    throw new InvalidOperationException("Modrinth project data is unavailable.");
                }

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

                images = project.Images?.Select(item => item != null && item.TryGetValue("url", out var image) ? image : null)
                    .Where(image => Uri.TryCreate(image, UriKind.Absolute, out var parsed) && parsed.Scheme == Uri.UriSchemeHttps)
                    .Distinct(StringComparer.OrdinalIgnoreCase) ?? Enumerable.Empty<string>();
            }
            else if (addon.Source == ProjectSource.Curseforge)
            {
                string fullDescription = await Task.Run(addon.GetFullDescription).ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(fullDescription))
                {
                    description = fullDescription;
                }
            }

            return new AddonPageData(description, images, minecraftVersions, modloaders, categories);
        }
    }
}
