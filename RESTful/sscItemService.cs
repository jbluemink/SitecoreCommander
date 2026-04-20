using SitecoreCommander.RESTful.Model;
using SitecoreCommander.Utils;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace SitecoreCommander.RESTful
{
    internal class SscItemService
    {
        private static string GetBaseUrl()
        {
            return Config.RestFullApiHostname;
        }

        public static async Task<CookieContainer> SourceLogInAsync()
        {
            var authUrl = GetBaseUrl().TrimEnd('/') + "/sitecore/api/ssc/auth/login";
            var authData = new Authentication
            {
                Domain = "sitecore",
                Username = Config.RestFullSitecoreUser,
                Password = Config.RestFullSitecorePassword
            };

            var cookies = new CookieContainer();
            using var handler = new HttpClientHandler
            {
                CookieContainer = cookies,
                UseCookies = true,
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            using var client = new HttpClient(handler);
            using var content = new StringContent(JsonSerializer.Serialize(authData), Encoding.UTF8, "application/json");
            using var response = await client.PostAsync(authUrl, content);

            response.EnsureSuccessStatusCode();
            Console.WriteLine($"Login Status:\n\r{response.StatusCode}");

            return cookies;
        }


        //path needs to start with /sitecore
        public static async Task<bool> TestItemExistsAsync(string path, CookieContainer cookies)
        {
            var url = GetBaseUrl().TrimEnd('/') + "/sitecore/api/ssc/item/?database=master&language=" + Config.DefaultLanguage + "&path=" + WebUtility.UrlEncode(path);

            using var client = CreateClient(cookies);

            try
            {
                using var response = await client.GetAsync(url);
                return response.StatusCode == HttpStatusCode.OK;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        public static async Task<Guid?> GetItemGuidAsync(string path, CookieContainer cookies, string language)
        {
            var url = GetBaseUrl().TrimEnd('/') + "/sitecore/api/ssc/item/?database=master&language=" + language + "&path=" + WebUtility.UrlEncode(path);

            using var client = CreateClient(cookies);

            try
            {
                using var response = await client.GetAsync(url);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var jsonobject = JsonSerializer.Deserialize<StandardSscItem>(json);
                    return jsonobject?.ItemID;
                }
            }
            catch (HttpRequestException)
            {
            }
            return null;
        }

        public static async Task<StandardSscItemExtended?> GetItemAsync(string path, CookieContainer cookies, string language)
        {
            var url = GetBaseUrl().TrimEnd('/') + "/sitecore/api/ssc/item/?database=master&language=" + language + "&path=" + WebUtility.UrlEncode(path) + "&includeStandardTemplateFields=true&includeMetadata=true&fields";

            using var client = CreateClient(cookies);

            try
            {
                using var response = await client.GetAsync(url);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<StandardSscItemExtended>(json);
                }
            }
            catch (HttpRequestException)
            {
            }
            return null;
        }

        public static async Task<StandardSscItemExtended?> GetItemByIdAsync(string id, CookieContainer cookies, string language)
        {
            var url = GetBaseUrl().TrimEnd('/') + "/sitecore/api/ssc/item/" + id + "?database=master&language=" + language + "&includeStandardTemplateFields=true&includeMetadata=true&fields";

            using var client = CreateClient(cookies);

            try
            {
                using var response = await client.GetAsync(url);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<StandardSscItemExtended>(json);
                }
            }
            catch (HttpRequestException)
            {
            }
            return null;
        }

        public static async Task<byte[]?> DownloadMediaBytesByIdAsync(string id, string language)
        {
            var cookies = await SourceLogInAsync();

            // Build /-/media/{compactGuid}.ashx directly from the item id — avoids shell/thumbnail URLs.
            var cleanGuid = id.Trim().TrimStart('{').TrimEnd('}').Replace("-", string.Empty, StringComparison.Ordinal).ToUpperInvariant();
            var mediaUrl = $"{GetBaseUrl().TrimEnd('/')}/-/media/{cleanGuid}.ashx";
            await SimpleLogger.LogAsync($"[SSC] media-download-url | itemId={id} | url={mediaUrl}");

            using var client = CreateClient(cookies);
            foreach (var candidate in BuildMediaUrlCandidates(mediaUrl))
            {
                using var response = await client.GetAsync(candidate);
                if (!response.IsSuccessStatusCode)
                {
                    await SimpleLogger.LogAsync($"[SSC] media-download-failed | itemId={id} | status={(int)response.StatusCode} | url={candidate}");
                    continue;
                }

                var bytes = await response.Content.ReadAsByteArrayAsync();
                if (bytes.Length == 0)
                {
                    await SimpleLogger.LogAsync($"[SSC] media-download-empty | itemId={id} | url={candidate}");
                    continue;
                }

                await SimpleLogger.LogAsync($"[SSC] media-download-succeeded | itemId={id} | bytes={bytes.Length} | url={candidate}");
                return bytes;
            }

            return null;
        }

        private static IEnumerable<string> BuildMediaUrlCandidates(string mediaUrl)
        {
            var urls = new List<string>();
            void Add(string? url)
            {
                if (!string.IsNullOrWhiteSpace(url) && !urls.Contains(url, StringComparer.OrdinalIgnoreCase))
                    urls.Add(url);
            }

            Add(mediaUrl);

            // Try without query params (SSC often returns thumbnail URLs)
            var queryIndex = mediaUrl.IndexOf('?', StringComparison.Ordinal);
            if (queryIndex > 0)
                Add(mediaUrl.Substring(0, queryIndex));

            if (mediaUrl.Contains("/-/media/", StringComparison.OrdinalIgnoreCase))
            {
                Add(mediaUrl.Replace("/-/media/", "/~/media/", StringComparison.OrdinalIgnoreCase));
                if (queryIndex > 0)
                {
                    var noQuery = mediaUrl.Substring(0, queryIndex);
                    Add(noQuery.Replace("/-/media/", "/~/media/", StringComparison.OrdinalIgnoreCase));
                }
            }

            if (mediaUrl.Contains("/~/media/", StringComparison.OrdinalIgnoreCase))
            {
                Add(mediaUrl.Replace("/~/media/", "/-/media/", StringComparison.OrdinalIgnoreCase));
                if (queryIndex > 0)
                {
                    var noQuery = mediaUrl.Substring(0, queryIndex);
                    Add(noQuery.Replace("/~/media/", "/-/media/", StringComparison.OrdinalIgnoreCase));
                }
            }

            var guidMatch = System.Text.RegularExpressions.Regex.Match(
                mediaUrl,
                @"(?<id>[a-fA-F0-9]{32}|[a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12})\.ashx",
                System.Text.RegularExpressions.RegexOptions.CultureInvariant);

            if (guidMatch.Success && Uri.TryCreate(mediaUrl, UriKind.Absolute, out var parsed))
            {
                var compact = guidMatch.Groups["id"].Value.Replace("-", string.Empty, StringComparison.Ordinal);
                Add($"{parsed.Scheme}://{parsed.Authority}/sitecore/shell/Applications/-/media/{compact}.ashx");
                Add($"{parsed.Scheme}://{parsed.Authority}/sitecore/shell/Applications/~/media/{compact}.ashx");
            }

            return urls;
        }

        public static async Task<StandardSscItemExtended[]?> GetChildrenAsync(string id, CookieContainer cookies, string language)
        {
            var url = GetBaseUrl().TrimEnd('/') + "/sitecore/api/ssc/item/" + id + "/children?database=master&language=" + language + "&includeStandardTemplateFields=true&includeMetadata=true&fields";

            using var client = CreateClient(cookies);

            try
            {
                using var response = await client.GetAsync(url);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<StandardSscItemExtended[]>(json);
                }
            }
            catch (HttpRequestException)
            {
            }
            return null;
        }

        private static HttpClient CreateClient(CookieContainer cookies)
        {
            var handler = new HttpClientHandler
            {
                CookieContainer = cookies,
                UseCookies = true,
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            return new HttpClient(handler);
        }
    }
}
