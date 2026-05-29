using DataAccess.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Security.Claims;

namespace Infrastructure.Middleware
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequireDbRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public RequireDbRoleAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // AllowAnonymous varsa atla
            if (context.ActionDescriptor.EndpointMetadata
                .OfType<AllowAnonymousAttribute>()
                .Any())
            {
                return;
            }

            var user = context.HttpContext.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // NameIdentifier claim'ı içinde sakladığımız UserId'yi al
            var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(idClaim) || !int.TryParse(idClaim, out var userId))
            {
                context.Result = new ForbidResult();
                return;
            }

            // DI kayıtlı olmalı; yoksa exception fırlatır
            var userRoleDal = context.HttpContext.RequestServices
                .GetRequiredService<IUserRoleDal>();

            // Kullanıcının RoleName'lerini çek
            var userRoles = userRoleDal
                .GetList(ur => ur.UserId == userId, ur => ur.Role)
                .Select(ur => ur.Role.RoleName)
                .ToList();

            // En az bir tanesi eşleşiyor mu?
            var hasMatch = userRoles
                .Intersect(_roles, StringComparer.OrdinalIgnoreCase)
                .Any();

            if (!hasMatch)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}