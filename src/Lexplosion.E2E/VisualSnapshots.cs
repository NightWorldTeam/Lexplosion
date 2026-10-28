using System;
using System.Drawing;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Lexplosion.E2E
{
    /// <summary>
    /// Optional pixel baseline for deterministic QA fixtures. Navigation checks
    /// run without baselines; no screenshot is silently accepted as a baseline.
    /// </summary>
    internal static class VisualSnapshots
    {
        public static void Verify(string screenshot, string route, string artifacts)
        {
            string baselineDir = Environment.GetEnvironmentVariable("LEXPLOSION_E2E_BASELINES");
            if (string.IsNullOrWhiteSpace(baselineDir))
            {
                File.AppendAllText(Path.Combine(artifacts, "visual-results.tsv"),
                    route + "\tUNVERIFIED\tno-baseline-directory" + Environment.NewLine);
                return;
            }

            Directory.CreateDirectory(baselineDir);
            string filename = new string(route.Take(90).Select(c =>
                char.IsLetterOrDigit(c) || c == '.' || c == '-' || c == '_' ? c : '_').ToArray()) + ".png";
            string baseline = Path.Combine(baselineDir, filename);
            if (Environment.GetEnvironmentVariable("LEXPLOSION_E2E_RECORD_BASELINES") == "1")
            {
                File.Copy(screenshot, baseline, overwrite: true);
                File.AppendAllText(Path.Combine(artifacts, "visual-results.tsv"),
                    route + "\tRECORDED\tnew-reference" + Environment.NewLine);
                return;
            }

            if (!File.Exists(baseline))
            {
                File.AppendAllText(Path.Combine(artifacts, "visual-results.tsv"),
                    route + "\tUNVERIFIED\tmissing-reference" + Environment.NewLine);
                if (Environment.GetEnvironmentVariable("LEXPLOSION_E2E_STRICT_VISUAL") == "1")
                    Assert.Fail("Missing visual baseline for " + route);
                return;
            }

            using (var expected = new Bitmap(baseline))
            using (var actual = new Bitmap(screenshot))
            {
                if (actual.Width != expected.Width || actual.Height != expected.Height)
                {
                    File.AppendAllText(Path.Combine(artifacts, "visual-results.tsv"),
                        route + "\tFAIL\tsize-mismatch" + Environment.NewLine);
                    Assert.Fail("Visual resolution mismatch at " + route +
                        ": expected " + expected.Size + ", actual " + actual.Size);
                }

                // Sampling avoids an expensive per-pixel full-window comparison.
                // Only use on a controlled fixture with fixed DPI, theme and content.
                long differences = 0;
                long sampled = 0;
                for (int y = 0; y < actual.Height; y += 4)
                for (int x = 0; x < actual.Width; x += 4)
                {
                    var a = actual.GetPixel(x, y);
                    var b = expected.GetPixel(x, y);
                    if (Math.Abs(a.R - b.R) + Math.Abs(a.G - b.G) +
                        Math.Abs(a.B - b.B) > 75)
                        differences++;
                    sampled++;
                }

                double ratio = sampled == 0 ? 1 : (double)differences / sampled;
                double max = 0.05;
                var tolerance = Environment.GetEnvironmentVariable("LEXPLOSION_E2E_VISUAL_TOLERANCE");
                if (double.TryParse(tolerance, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var configured))
                    max = Math.Max(0, Math.Min(1, configured));

                File.AppendAllText(Path.Combine(artifacts, "visual-results.tsv"),
                    route + "\t" + (ratio <= max ? "PASS" : "FAIL") +
                    "\tdiff=" + ratio.ToString("P2", System.Globalization.CultureInfo.InvariantCulture) +
                    Environment.NewLine);
                Assert.That(ratio, Is.LessThanOrEqualTo(max),
                    "Visual regression at " + route + "; inspect screenshot and reference.");
            }
        }
    }
}
