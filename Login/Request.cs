using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SitecoreCommander.Authoring.Model;

namespace SitecoreCommander.Login
{
    internal class Request
    {
        //private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly HttpClient _httpClient = new HttpClient(
            new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            }
        );

        /// <summary>
        /// Calls a specified GraphQL endpoint with the specified query and variables.
        /// </summary>
        internal static async Task<GraphQLResponse<TResponse>> CallGraphQLAsync<TResponse>(
            Uri endpoint,
            HttpMethod method,
            string accessToken,
            string apiKey,
            string query,
            object variables,
            CancellationToken cancellationToken,
            TimeSpan? requestTimeout = null) // Optional per-request timeout
        {
            var content = new StringContent(SerializeGraphQLCall(query, variables), Encoding.UTF8, "application/json");
            var httpRequestMessage = new HttpRequestMessage
            {
                Method = method,
                Content = content,
                RequestUri = endpoint,
            };
            httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (!string.IsNullOrEmpty(accessToken))
            {
                httpRequestMessage.Headers.Add("Authorization", "Bearer " + accessToken);
            }
            if (!string.IsNullOrEmpty(apiKey))
            {
                httpRequestMessage.Headers.Add("sc_apikey", apiKey);
            }

            // Use a linked token source to support per-request timeout
            using var cts = requestTimeout.HasValue
                ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
                : null;
            if (requestTimeout.HasValue)
                cts.CancelAfter(requestTimeout.Value);

            try
            {
                using var response = await _httpClient.SendAsync(
                    httpRequestMessage,
                    cts?.Token ?? cancellationToken
                ).ConfigureAwait(false);

                if (response?.Content.Headers.ContentType?.MediaType == "application/json")
                {
                    var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return DeserializeGraphQLCall<TResponse>(responseString);
                }
                else
                {
                    throw new ApplicationException($"Unable to contact '{endpoint}': {response.StatusCode} - {response.ReasonPhrase}");
                }
            }
            catch (TaskCanceledException ex) when (!(cancellationToken.IsCancellationRequested))
            {
                throw new TimeoutException($"The request to '{endpoint}' timed out after {(requestTimeout ?? _httpClient.Timeout).TotalSeconds} seconds.", ex);
            }
        }


        internal static async Task<Uploaded> CallGraphQLUploadAsync<TResponse>(HttpMethod method, string accessToken, string presignedUploadUrl, string filePathOrUrl, CancellationToken cancellationToken)
        {
            byte[] fileBytes;
            string fileName;

            if (Uri.TryCreate(filePathOrUrl, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                // It's a URL, download the file
                try
                {
                    using (var response = await _httpClient.GetAsync(uri, cancellationToken).ConfigureAwait(false))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            Console.WriteLine($"Failed to download file from URL '{filePathOrUrl}': {response.StatusCode} - {response.ReasonPhrase}");
                            throw new ApplicationException($"Failed to download file from URL '{filePathOrUrl}': {response.StatusCode} - {response.ReasonPhrase}");
                        }

                        fileBytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                        fileName = Path.GetFileName(uri.LocalPath); // Extract file name from URL
                    }
                } 
                catch (HttpRequestException ex)
                {
                    throw new ApplicationException($"HTTP error while downloading file from '{filePathOrUrl}': {ex.Message}", ex);
                }
                catch (TaskCanceledException ex) when (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException("File download was canceled.", ex, cancellationToken);
                }
            }
            else
            {
                // It's a local file path
                fileBytes = File.ReadAllBytes(filePathOrUrl);
                fileName = Path.GetFileName(filePathOrUrl);
            }

            var formContent = new MultipartFormDataContent
            {
                // Send the file
                { new StreamContent(new MemoryStream(fileBytes)), "imagekey", fileName }
            };

            var httpRequestMessage = new HttpRequestMessage
            {
                Method = method,
                Content = formContent,
                RequestUri = new Uri(presignedUploadUrl),
            };

            // Add authorization headers if necessary
            httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (!string.IsNullOrEmpty(accessToken))
            {
                httpRequestMessage.Headers.Add("Authorization", "Bearer " + accessToken);
            }

            using (var response = await _httpClient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false))
            {
                if (response?.Content.Headers.ContentType?.MediaType == "application/json")
                {
                    var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return DeserializeGraphQLUploadCall(responseString);
                }
                else
                {
                    throw new ApplicationException($"Unable to contact '{presignedUploadUrl}': {response.StatusCode} - {response.ReasonPhrase}");
                }
            }
        }

        public class GraphQLErrorLocation
        {
            public int Line { get; set; }
            public int Column { get; set; }
        }

        public class GraphQLError
        {
            public string Message { get; set; }
            public List<GraphQLErrorLocation> Locations { get; set; }
            public List<object> Path { get; set; }
        }

        public class GraphQLResponse<TResponse>
        {
            public List<GraphQLError> Errors { get; set; }
            public TResponse Data { get; set; }
        }

        /// <summary>
        /// Serializes a query and variables to JSON to be sent to the GraphQL endpoint.
        /// </summary>
        private static string SerializeGraphQLCall(string query, object variables)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            var json = JsonSerializer.Serialize(new
            {
                query,
                variables,
            }, options);

            return json;
        }

        private static Uploaded DeserializeGraphQLUploadCall(string response)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,//PascalCase
                WriteIndented = false
            };

            var result = JsonSerializer.Deserialize<Uploaded>(response, options);
             return result;
        }

        /// <summary>
        /// Deserializes a GraphQL response.
        /// </summary>
        private static GraphQLResponse<TResponse> DeserializeGraphQLCall<TResponse>(string response)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            var result = JsonSerializer.Deserialize<GraphQLResponse<TResponse>>(response, options);
            return result;
        }
    }
}
