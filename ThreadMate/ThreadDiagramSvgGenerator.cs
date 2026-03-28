using System.Globalization;
using System.Text.RegularExpressions;

namespace ThreadMate
{
    public static class ThreadDiagramSvgGenerator
    {
        private static string? _internalTemplate;
        private static string? _externalTemplate;

        public static string GenerateInternalThreadDiagram(double major, double pitchDiameter, double minor)
        {
            var template = EnsureTemplateLoaded(ref _internalTemplate, "thread_internal.svg");
            if (string.IsNullOrWhiteSpace(template))
            {
                return "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 10 10\"></svg>";
            }

            return ReplaceValuesAndApplyTheme(template, major, pitchDiameter, minor);
        }

        public static string GenerateExternalThreadDiagram(double major, double pitchDiameter, double minor)
        {
            var template = EnsureTemplateLoaded(ref _externalTemplate, "thread_external.svg");
            if (string.IsNullOrWhiteSpace(template))
            {
                return "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 10 10\"></svg>";
            }

            return ReplaceValuesAndApplyTheme(template, major, pitchDiameter, minor);
        }

        private static string? EnsureTemplateLoaded(ref string? template, string fileName)
        {
            if (!string.IsNullOrWhiteSpace(template))
            {
                return template;
            }

            try
            {
                using var stream = FileSystem.OpenAppPackageFileAsync(fileName).GetAwaiter().GetResult();
                using var reader = new StreamReader(stream);
                template = reader.ReadToEnd();
            }
            catch
            {
                template = null;
            }

            return template;
        }

        private static string ReplaceValuesAndApplyTheme(string svg, double major, double pitchDiameter, double minor)
        {
            svg = ReplaceTextById(svg, "major", FormatValue(major));
            svg = ReplaceTextById(svg, "pitch_diameter", FormatValue(pitchDiameter));
            svg = ReplaceTextById(svg, "minor", FormatValue(minor));
            svg = EnsureTransparentBackground(svg);
            svg = ApplyThemeLineColors(svg);
            return svg;
        }

        private static string ReplaceTextById(string svg, string id, string value)
        {
            var pattern = $"(<text[^>]*id=\"{Regex.Escape(id)}\"[^>]*>)(.*?)(</text>)";
            return Regex.Replace(
                svg,
                pattern,
                match => $"{match.Groups[1].Value}{value}{match.Groups[3].Value}",
                RegexOptions.Singleline);
        }

        private static string EnsureTransparentBackground(string svg)
        {
            svg = Regex.Replace(svg, @"(<svg\b[^>]*?)\sstyle=""([^""]*)""", @"$1 style=""$2;background:transparent;""", RegexOptions.IgnoreCase);
            if (!Regex.IsMatch(svg, @"<svg\b[^>]*\sstyle=""[^""]*""", RegexOptions.IgnoreCase))
            {
                svg = Regex.Replace(svg, @"<svg\b", @"<svg style=""background:transparent;""", RegexOptions.IgnoreCase);
            }

            svg = Regex.Replace(svg, @"fill\s*:\s*#(?:fff|ffffff)\b", "fill:transparent", RegexOptions.IgnoreCase);
            svg = Regex.Replace(svg, @"fill\s*=\s*""#(?:fff|ffffff)""", "fill=\"transparent\"", RegexOptions.IgnoreCase);
            return svg;
        }

        private static string ApplyThemeLineColors(string svg)
        {
            var isDark = Application.Current?.RequestedTheme == AppTheme.Dark;
            var outlineColor = isDark ? "#FFFFFF" : "#000000";
            var grayColor = isDark ? "#CFCFCF" : "#AAAAAA";

            svg = Regex.Replace(svg, "#000000", outlineColor, RegexOptions.IgnoreCase);
            svg = Regex.Replace(svg, "#aaaaaa", grayColor, RegexOptions.IgnoreCase);

            return svg;
        }

        private static string FormatValue(double value)
        {
            return $"{value.ToString("F3", CultureInfo.InvariantCulture)} mm";
        }
    }
}
