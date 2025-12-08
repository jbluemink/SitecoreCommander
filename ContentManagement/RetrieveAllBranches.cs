using SitecoreCommander.Login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SitecoreCommander.ContentManagement
{
    internal class RetrieveAllBranches
    {
        internal static async Task<string?> GetBranches(JwtTokenResponse token)

        {
            System.Net.Http.HttpClient client = new()
            {
                DefaultRequestHeaders =
                  {
                    {"Authorization", "Bearer " + token.access_token},
                  }
            };

            using HttpResponseMessage request = await client.GetAsync("https://edge-platform.sitecorecloud.io/cs" + "/api/v2/cm/branches/");
            string response = await request.Content.ReadAsStringAsync();

            Console.WriteLine(response);
            return response;
        }
    }
}
