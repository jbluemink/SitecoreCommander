using SitecoreCommander.Login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SitecoreCommander.ContentManagement
{
    internal class RetrieveListOfUsers
    {
        internal static async Task<string?> GetUsers(JwtTokenResponse token)

        {
            System.Net.Http.HttpClient client = new()
            {
                DefaultRequestHeaders =
                  {
                    {"Authorization", "Bearer " + token.access_token},
                  }
            };

            using HttpResponseMessage request = await client.GetAsync("https://edge-platform.sitecorecloud.io/cs" + "/api/v2/cm/users" + "?SortBy=string&MinimumPageSize=1&Type=string");
            string response = await request.Content.ReadAsStringAsync();

            Console.WriteLine(response);
            return response;
        }
    }
}
