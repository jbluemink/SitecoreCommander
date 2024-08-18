using SitecoreCommander.RESTful.Model;
using System.Net;
using System.Text;
using System.Text.Json;

namespace SitecoreCommander.RESTful
{
    internal class SscItemService
    {
        public static CookieContainer SourceLogIn()
        {
            var authUrl = Config.RestFullApiHostname + "/sitecore/api/ssc/auth/login";
            var authData = new Authentication
            {
                Domain = "sitecore",
                Username = Config.RestFullSitecoreUser,
                Password = Config.RestFullSitecorePassword
            };

            var authRequest = (HttpWebRequest)WebRequest.Create(authUrl);

            authRequest.Method = "POST";
            authRequest.ContentType = "application/json";

            var requestAuthBody = JsonSerializer.Serialize(authData);

            var authDatas = new UTF8Encoding().GetBytes(requestAuthBody);

            using (var dataStream = authRequest.GetRequestStream())
            {
                dataStream.Write(authDatas, 0, authDatas.Length);
            }

            CookieContainer cookies = new CookieContainer();

            authRequest.CookieContainer = cookies;

            var authResponse = authRequest.GetResponse();

            Console.WriteLine($"Login Status:\n\r{((HttpWebResponse)authResponse).StatusDescription}");

            authResponse.Close();

            return cookies;
        }


        //path needs to start with /sitecore
        public static bool TestItemExists(string path, CookieContainer cookies)
        {

            var url = Config.RestFullApiHostname + "/sitecore/api/ssc/item/?database=master&language=" + Config.DefaultLanguage + "&path=" + System.Web.HttpUtility.UrlEncode(path);

            var request = (HttpWebRequest)WebRequest.Create(url);

            request.Method = "GET";
            request.ContentType = "application/json";
            request.CookieContainer = cookies;

            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
            }
            catch (WebException)
            {

            }
            return false;
        }

        public static Guid? GetItemGuid(string path, CookieContainer cookies, string language)
        {

            var url = Config.RestFullApiHostname + "/sitecore/api/ssc/item/?database=master&language=" + language + "&path=" + System.Web.HttpUtility.UrlEncode(path);

            var request = (HttpWebRequest)WebRequest.Create(url);

            request.Method = "GET";
            request.ContentType = "application/json";
            request.CookieContainer = cookies;

            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                        string json = reader.ReadToEnd();
                        StandardSscItem jsonobject = JsonSerializer.Deserialize<StandardSscItem>(json);
                        return jsonobject.ItemID;
                    }
                }
            }
            catch (WebException)
            {

            }
            return null;
        }
        public static StandardSscItemExtended GetItem(string path, CookieContainer cookies, string language)
        {

            var url = Config.RestFullApiHostname + "/sitecore/api/ssc/item/?database=master&language=" + language + "&path=" + System.Web.HttpUtility.UrlEncode(path) + "&includeStandardTemplateFields=true&includeMetadata=true&fields";

            var request = (HttpWebRequest)WebRequest.Create(url);

            request.Method = "GET";
            request.ContentType = "application/json";
            request.CookieContainer = cookies;

            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                        string json = reader.ReadToEnd();
                        StandardSscItemExtended jsonobject = JsonSerializer.Deserialize<StandardSscItemExtended>(json);
                        return jsonobject;
                    }
                }
            }
            catch (WebException)
            {

            }
            return null;
        }
        public static StandardSscItemExtended GetItemById(string id, CookieContainer cookies, string language)
        {

            var url = Config.RestFullApiHostname + "/sitecore/api/ssc/item/" + id + "?database=master&language=" + language + "&includeStandardTemplateFields=true&includeMetadata=true&fields";

            var request = (HttpWebRequest)WebRequest.Create(url);

            request.Method = "GET";
            request.ContentType = "application/json";
            request.CookieContainer = cookies;

            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                        string json = reader.ReadToEnd();
                        StandardSscItemExtended jsonobject = JsonSerializer.Deserialize<StandardSscItemExtended>(json);
                        return jsonobject;
                    }
                }
            }
            catch (WebException)
            {

            }
            return null;
        }
        public static StandardSscItemExtended[] GetChilderen(string id, CookieContainer cookies, string language)
        {

            var url = Config.RestFullApiHostname + "/sitecore/api/ssc/item/" + id + "/children?database=master&language=" + language + "&includeStandardTemplateFields=true&includeMetadata=true&fields";

            var request = (HttpWebRequest)WebRequest.Create(url);

            request.Method = "GET";
            request.ContentType = "application/json";
            request.CookieContainer = cookies;

            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                        string json = reader.ReadToEnd();
                        StandardSscItemExtended[] jsonobject = JsonSerializer.Deserialize<StandardSscItemExtended[]>(json);
                        return jsonobject;
                    }
                }
            }
            catch (WebException)
            {

            }
            return null;
        }
    }
}
