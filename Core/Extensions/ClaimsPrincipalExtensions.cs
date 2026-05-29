using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Core.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserGuid(this ClaimsPrincipal user)
        {
            var sid = user.FindFirst(ClaimTypes.Sid)?.Value;
            if (Guid.TryParse(sid, out var guid))
                return guid;
            throw new UnauthorizedAccessException("Geçersiz kullanıcı bilgisi (Sid).");
        }

        public static string GetUsername(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Name)?.Value
                   ?? throw new UnauthorizedAccessException("Geçersiz kullanıcı bilgisi (Name).");
        }

        public static string GetEmail(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Email)?.Value
                   ?? string.Empty;
        }

        public static string[] GetRoles(this ClaimsPrincipal user)
        {
            return user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
        }
    }
}
