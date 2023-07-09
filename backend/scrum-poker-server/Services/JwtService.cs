using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using scrum_poker_server.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace scrum_poker_server.Services
{
    public interface IJwtService
    {
        public string GenerateToken(User user);
    }

    public class JwtService : IJwtService
    {
        private IConfiguration _configuration { get; set; }
        private SigningCredentials Credentials { get; set; }
        private JwtSecurityTokenHandler JwtTokenHandler { get; set; }

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            Credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
            JwtTokenHandler = new JwtSecurityTokenHandler();
        }

        public string GenerateToken(User user)
        {
            var claims = new ClaimsIdentity(new[] { new Claim("UserId", user.Id.ToString()), new Claim(ClaimTypes.Name, user.Name) });
            bool isEmailNull = String.IsNullOrEmpty(user.Email);
            if (!isEmailNull)
            {
                claims.AddClaim(new Claim(ClaimTypes.Email, user.Email));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Expires = isEmailNull ? DateTime.Now.AddMonths(3) : DateTime.Now.AddMinutes(30),
                Subject = claims,
                SigningCredentials = Credentials,
            };

            var securityToken = JwtTokenHandler.CreateToken(tokenDescriptor);

            return JwtTokenHandler.WriteToken(securityToken);
        }
    }
}