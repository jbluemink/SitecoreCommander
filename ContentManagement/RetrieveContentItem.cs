using SitecoreCommander.Agent.Model;
using SitecoreCommander.Login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SitecoreCommander.ContentManagement
{
    internal class RetrieveContentItem
    {
        internal static async Task<string?> GetItemById(JwtTokenResponse token, string pageId)

        {
            System.Net.Http.HttpClient client = new()
            {
                DefaultRequestHeaders =
                  {
                    {"Authorization", "Bearer " + token.access_token},
                  }
            };

            var BranchId = "acceptance";
            var ContentItemId = "z0OPkxwS2kyM5xYE6Y7jc";
            using HttpResponseMessage request = await client.GetAsync("https://edge-platform.sitecorecloud.io/cs" + "/api/v2/cm/branches/" + BranchId + "/content-items/" + ContentItemId + "?locale=string&version=0");
            string response = await request.Content.ReadAsStringAsync();

             Console.WriteLine(response);
            return response;
        }
    }
}
