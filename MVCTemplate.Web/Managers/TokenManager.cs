using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MVCTemplate.Web.Authorization;
using MVCTemplate.Web.Data;
using MVCTemplate.Web.Models;
using Newtonsoft.Json;

namespace MVCTemplate.Web.Managers
{
    public static class TokenManager
    {
        public static async Task<JwtToken> Authenticate(IHttpClientFactory httpClientFactory, TokenModel tokenModel)
        {
            var httpClient = httpClientFactory.CreateClient("Api");
            var res = await httpClient.PostAsJsonAsync("Auth", tokenModel);
            res.EnsureSuccessStatusCode();
            string strJwt = await res.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<JwtToken>(strJwt);
        }
    }
}
