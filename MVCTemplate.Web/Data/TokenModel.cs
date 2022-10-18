using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace MVCTemplate.Web.Data
{
    public class TokenModel
    {
        public IdentityUser User { get; set; }

        public IList<Claim> Claims { get; set; }
    }
}
