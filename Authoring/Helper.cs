using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SitecoreCommander.Authoring
{
    class Helper
    {
        public static string ToValidItemName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "item";

            string normalized = input.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            foreach (char c in normalized)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            string withoutDiacritics = sb.ToString().Normalize(NormalizationForm.FormC);

            string cleaned = Regex.Replace(withoutDiacritics, @"[^a-zA-Z0-9 _-]", "");

            string final = Regex.Replace(cleaned.Trim(), @"\s+", " ");

            return string.IsNullOrWhiteSpace(final) ? "item" : final;
        }
        public static string StripHtmlTags(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            return Regex.Replace(input, "<.*?>", string.Empty);
        }

        public static string GetYouTubeVideoId(string url)
        {
            if (string.IsNullOrEmpty(url))
                return null;

            // Regex patterns for different YouTube URL formats
            var regex = new Regex(@"(?:youtu\.be/|youtube\.com/(?:watch\?v=|embed/|v/|shorts/))([^\s&?/]+)");

            var match = regex.Match(url);
            if (match.Success && match.Groups.Count > 1)
            {
                return match.Groups[1].Value;
            }

            return null;
        }
    }
}
