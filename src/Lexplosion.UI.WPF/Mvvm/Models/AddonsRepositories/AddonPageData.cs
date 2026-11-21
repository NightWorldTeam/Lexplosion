using Lexplosion.Logic.Management.Addons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Lexplosion.UI.WPF.Mvvm.Models.AddonsRepositories
{
    public sealed class AddonPageData
    {
        public string Description { get; }
        public IReadOnlyList<string> Images { get; }
        public IReadOnlyList<string> MinecraftVersions { get; }
        public IReadOnlyList<string> Modloaders { get; }
        public IReadOnlyList<string> Categories { get; }

        public IReadOnlyList<string> VisibleMinecraftVersions { get; }
        public IReadOnlyList<string> MoreMinecraftVersions { get; }
        public IReadOnlyList<string> VisibleModloaders { get; }
        public IReadOnlyList<string> MoreModloaders { get; }
        public IReadOnlyList<string> VisibleCategories { get; }
        public IReadOnlyList<string> MoreCategories { get; }

        public AddonPageData(string description, IEnumerable<string> images, IEnumerable<string> minecraftVersions, IEnumerable<string> modloaders, IEnumerable<string> categories)
        {
            Description = description ?? string.Empty;
            Images = Array.AsReadOnly((images ?? Enumerable.Empty<string>()).ToArray());

            var stableVersions = (minecraftVersions ?? Enumerable.Empty<string>()).Where(version => Regex.IsMatch(version ?? string.Empty, @"^\d+\.\d+(?:\.\d+)?$")).ToArray();
            MinecraftVersions = Labels(Enumerable.Reverse(stableVersions.Length > 0 ? stableVersions : minecraftVersions ?? Enumerable.Empty<string>()));
            Modloaders = Labels(modloaders);
            Categories = Labels(categories);

            VisibleMinecraftVersions = Slice(MinecraftVersions, 0, 7);
            MoreMinecraftVersions = Slice(MinecraftVersions, 7);
            VisibleModloaders = Slice(Modloaders, 0, 6);
            MoreModloaders = Slice(Modloaders, 6);
            VisibleCategories = Slice(Categories, 0, 10);
            MoreCategories = Slice(Categories, 10);
        }

        public static AddonPageData FromAddon(InstanceAddon addon)
        {
            if (addon == null)
            {
                return new AddonPageData(string.Empty, null, null, null, null);
            }

            return new AddonPageData(addon.Description, null,
                string.IsNullOrWhiteSpace(addon.GameVersion) ? null : new[] { addon.GameVersion },
                addon.DisplayModloaders, addon.Categories?.Select(category => category.Name));
        }

        private static IReadOnlyList<string> Labels(IEnumerable<string> values)
        {
            var labels = (values ?? Enumerable.Empty<string>()).Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            return Array.AsReadOnly(labels.Length > 0 ? labels : new[] { "—" });
        }

        private static IReadOnlyList<string> Slice(IReadOnlyList<string> values, int start, int count = int.MaxValue)
        {
            return Array.AsReadOnly(values.Skip(start).Take(count).ToArray());
        }
    }
}
