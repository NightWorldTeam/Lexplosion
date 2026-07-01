using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Lexplosion.Core.Tools
{
    public static class JvmArgsParser
    {
        private static readonly Regex HeapFlagPattern = new(
            @"\-Xm[xs]\d+[kKmMgGtT]?",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex GcFlagPattern = new(
            @"\-XX:\+?(Use\w+GC|ZGCGenerational|UnlockExperimentalVMOptions)",
            RegexOptions.Compiled);

        private static readonly Dictionary<string, int> GcFlagMinVersion = new(StringComparer.OrdinalIgnoreCase)
        {
            ["UseZGC"] = 15,
            ["ZGCGenerational"] = 21,
            ["UseShenandoahGC"] = 15,
            ["UseConcMarkSweepGC"] = 5,
            ["UseG1GC"] = 7,
            ["UseParallelGC"] = 5,
            ["UseSerialGC"] = 5,
            ["UnlockExperimentalVMOptions"] = 7,
        };

        private static readonly Dictionary<string, int> JavaNameMajorVersion = new(StringComparer.OrdinalIgnoreCase)
        {
            ["jre-legacy"] = 8,
            ["jre8"] = 8,
            ["jre11"] = 11,
            ["jre17"] = 17,
            ["jre21"] = 21,
        };

        public static string StripHeapFlags(string args)
        {
            if (string.IsNullOrWhiteSpace(args))
                return args ?? string.Empty;

            return HeapFlagPattern.Replace(args, "").Trim();
        }

        public static List<string> DetectGcFlags(string args)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(args))
                return result;

            foreach (Match match in GcFlagPattern.Matches(args))
            {
                result.Add(match.Value);
            }

            return result;
        }

        public static bool IsGcFlagCompatible(string flag, int javaMajorVersion)
        {
            if (javaMajorVersion <= 0)
                return true;

            foreach (var kvp in GcFlagMinVersion)
            {
                if (flag.IndexOf(kvp.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return javaMajorVersion >= kvp.Value;
                }
            }

            return true;
        }

        public static int ParseJavaMajorVersion(string javaVersionName)
        {
            if (string.IsNullOrWhiteSpace(javaVersionName))
                return 0;

            if (JavaNameMajorVersion.TryGetValue(javaVersionName.Trim().ToLowerInvariant(), out int version))
                return version;

            if (javaVersionName.StartsWith("jre", StringComparison.OrdinalIgnoreCase))
            {
                string numPart = javaVersionName.Substring(3);
                if (int.TryParse(numPart, out int parsed))
                    return parsed;
            }

            return 0;
        }
    }
}
