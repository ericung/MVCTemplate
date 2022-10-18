using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace MVCTemplate.Api.Models
{
    public class TokenModel
    {
        public IdentityUser User { get; set; }

        public IList<Claim> Claims { get; set; }
    }
}
