using Lexplosion.UI.WPF.Core.ViewModel;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Lexplosion.UI.WPF.Mvvm.Models.Modal
{
    public sealed class JvmArgEntry : ObservableObject
    {
        private string _key;
        public string Key
        {
            get => _key;
            set
            {
                _key = value;
                OnPropertyChanged();
            }
        }

        private string _value;
        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }

        public JvmArgEntry()
        {
        }

        public JvmArgEntry(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }

    public sealed class JvmArgsEditorModel
    {
        private static readonly Regex SplitArgsRegex = new(
            @"[\""'].+?[\""']|[^\s]+", RegexOptions.Compiled);

        private static readonly Regex XFlagRegex = new(
            @"^(-X\w+?)(\d+[gGmMkKbB]?)$", RegexOptions.Compiled);

        private static readonly Regex XEqRegex = new(
            @"^(-X\w+)=(.+)$", RegexOptions.Compiled);

        private static readonly Regex GenericEqRegex = new(
            @"^(-[\w:]+)=(.+)$", RegexOptions.Compiled);

        public List<JvmArgEntry> Parse(string args)
        {
            if (string.IsNullOrWhiteSpace(args))
                return new List<JvmArgEntry>();

            var tokens = SplitArgs(args);
            var entries = new List<JvmArgEntry>();

            foreach (var token in tokens)
            {
                var entry = ParseToken(token);
                if (entry != null)
                    entries.Add(entry);
            }

            return entries;
        }

        private List<string> SplitArgs(string args)
        {
            var result = new List<string>();
            var matches = SplitArgsRegex.Matches(args);

            foreach (Match match in matches)
                result.Add(match.Value);

            return result;
        }

        private JvmArgEntry ParseToken(string token)
        {
            token = token.Trim().Trim('"', '\'');

            if (string.IsNullOrEmpty(token))
                return null;

            if (token.StartsWith("-D"))
            {
                if (token.Contains('='))
                {
                    var eqIndex = token.IndexOf('=');
                    var key = token.Substring(0, eqIndex);
                    var value = token.Substring(eqIndex + 1);
                    return new JvmArgEntry(key, value);
                }
                return new JvmArgEntry(token, "");
            }

            if (token.StartsWith("-XX:"))
            {
                var eqIndex = token.IndexOf('=');
                if (eqIndex > 0)
                {
                    var key = token.Substring(0, eqIndex);
                    var value = token.Substring(eqIndex + 1);
                    return new JvmArgEntry(key, value);
                }
                return new JvmArgEntry(token, "");
            }

            if (token.StartsWith("-X"))
            {
                var match = XFlagRegex.Match(token);
                if (match.Success)
                    return new JvmArgEntry(match.Groups[1].Value, match.Groups[2].Value);

                var eqMatch = XEqRegex.Match(token);
                if (eqMatch.Success)
                    return new JvmArgEntry(eqMatch.Groups[1].Value, eqMatch.Groups[2].Value);
            }

            if (token.StartsWith("-"))
            {
                var eqMatch = GenericEqRegex.Match(token);
                if (eqMatch.Success)
                    return new JvmArgEntry(eqMatch.Groups[1].Value, eqMatch.Groups[2].Value);

                return new JvmArgEntry(token, "");
            }

            return new JvmArgEntry(token, "");
        }

        public string Rebuild(List<JvmArgEntry> entries)
        {
            if (entries == null || entries.Count == 0)
                return string.Empty;

            var parts = entries.Select(e =>
            {
                if (string.IsNullOrEmpty(e.Value))
                    return e.Key;

                if (e.Key.StartsWith("-X") && !string.IsNullOrEmpty(e.Value) && char.IsDigit(e.Value[0]))
                    return $"{e.Key}{e.Value}";

                return $"{e.Key}={e.Value}";
            });

            return string.Join(" ", parts);
        }

        public List<JvmArgEntry> MergeAndDeduplicate(List<JvmArgEntry> existing, string bulkText)
        {
            var dict = new Dictionary<string, JvmArgEntry>();

            foreach (var entry in existing)
            {
                var normalizedKey = NormalizeKey(entry.Key);
                if (!dict.ContainsKey(normalizedKey))
                    dict[normalizedKey] = entry;
            }

            var newEntries = Parse(bulkText);

            foreach (var entry in newEntries)
            {
                var normalizedKey = NormalizeKey(entry.Key);
                if (!dict.ContainsKey(normalizedKey))
                    dict[normalizedKey] = entry;
            }

            return dict.Values.ToList();
        }

        private string NormalizeKey(string key)
        {
            return key?.Trim().ToLowerInvariant() ?? string.Empty;
        }
    }
}
