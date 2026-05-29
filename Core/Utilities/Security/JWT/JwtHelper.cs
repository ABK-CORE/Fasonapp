using Core.Entities.Concrete;
using Core.Entities.Extensions;
using Core.Utilities.Security.Encryption;
using Enigma;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Core.Utilities.Security.JWT
{
    public class JwtHelper : ITokenHelper
    {
        private readonly TokenOptions _tokenOptions;
        private DateTime _accessTokenExpiration;
        private readonly Processor _processor;

        public JwtHelper(IConfiguration configuration, Processor processor)
        {
            _processor = processor;

            var securityKeyEncrypted = configuration["JwtSettings:SecurityKey"];

            var tokenOptions = configuration.GetSection("TokenOptions").Get<TokenOptions>();
            if (string.IsNullOrEmpty(securityKeyEncrypted))
                throw new Exception("JwtSettings:SecurityKey appsettings'te tanımlı değil veya boş.");
            if (tokenOptions == null)
                throw new Exception("TokenOptions appsettings'te tanımlı değil veya boş.");
            _tokenOptions = tokenOptions;

            using (Aes aes = Aes.Create())
            {
                _tokenOptions.SecurityKey = _processor.DecryptorSymmetric(securityKeyEncrypted, aes);
            }
        }

        public AccessToken CreateToken(TokenUser user)
        {
            _accessTokenExpiration = DateTime.Now.AddMinutes(_tokenOptions.AccessTokenExpiration);
            var securityKey = SecurityKeyHelper.CreateSecurityKey(_tokenOptions.SecurityKey);
            var signingCredentials = SigningCredentialsHelper.CreateSigningCredentials(securityKey);
            var jwt = CreateJwtSecurityToken(_tokenOptions, user, signingCredentials);
            var token = new JwtSecurityTokenHandler().WriteToken(jwt);

            return new AccessToken
            {
                Token = token,
                Expiration = _accessTokenExpiration,
                User = user,
                IsSuccessful = true
            };
        }

        public JwtSecurityToken CreateJwtSecurityToken(
            TokenOptions tokenOptions,
            TokenUser user,
            SigningCredentials signingCredentials)
        {
            return new JwtSecurityToken(
                issuer: tokenOptions.Issuer,
                audience: tokenOptions.Audience,
                expires: _accessTokenExpiration,
                notBefore: DateTime.Now,
                claims: SetClaims(user),
                signingCredentials: signingCredentials
            );
        }

        private IEnumerable<Claim> SetClaims(TokenUser user)
        {
            var claims = new List<Claim>();

            // UserId
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()));

            // UserGuid (BaseApiController.CurrentUserGuid bu claim'i kullanir)
            if (user.UserGuid != Guid.Empty)
                claims.Add(new Claim(ClaimTypes.Sid, user.UserGuid.ToString()));

            // Username
            claims.Add(new Claim(ClaimTypes.Name, user.Username ?? string.Empty));

            // CompanyName (özel claim olarak ekleniyor)
            if (!string.IsNullOrEmpty(user.CompanyName))
                claims.Add(new Claim("CompanyName", user.CompanyName));

            // Roles
            if (user.Roles != null)
            {
                foreach (var role in user.Roles.Where(r => !string.IsNullOrWhiteSpace(r)))
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            return claims;
        }
    }
}