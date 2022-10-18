using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MVCTemplate.Api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MVCTemplate.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration configuration;

        public AuthController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [HttpPost]
        public IActionResult  Authenticate([FromBody]TokenModel tokenModel)
        {
            tokenModel.Claims.Add(new Claim(ClaimTypes.Name, tokenModel.User.UserName));
            tokenModel.Claims.Add(new Claim(ClaimTypes.Email, tokenModel.User.Email));
            var expiresAt = DateTime.UtcNow.AddMinutes(15);

            return Ok(new
            {
                access_token = CreateToken(tokenModel.Claims, expiresAt),
                expires_at = expiresAt
            });
        }

        private string CreateToken(IEnumerable<Claim> claims, DateTime expiresAt)
        {
            var secretKey = Encoding.ASCII.GetBytes(configuration.GetValue<string>("SecretKey"));

            var jwt = new JwtSecurityToken(
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiresAt,
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(secretKey),
                    SecurityAlgorithms.HmacSha256Signature)
                );

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}