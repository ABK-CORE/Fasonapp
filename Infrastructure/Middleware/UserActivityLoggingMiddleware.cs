using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Middleware
{
    public class UserActivityLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<UserActivityLoggingMiddleware> _logger;
        private readonly IUserActivityLogDal _activityLogDal;

        public UserActivityLoggingMiddleware(
            RequestDelegate next,
            ILogger<UserActivityLoggingMiddleware> logger,
            IUserActivityLogDal activityLogDal)
        {
            _next = next;
            _logger = logger;
            _activityLogDal = activityLogDal;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 1) CORS preflight OPTIONS isteklerini atla
            if (context.Request.Method == HttpMethods.Options)
            {
                await _next(context);
                return;
            }

            // 2) İstek gövdesini oku
            context.Request.EnableBuffering();
            string requestBody = "";
            using (var reader = new StreamReader(
                       context.Request.Body,
                       Encoding.UTF8,
                       detectEncodingFromByteOrderMarks: false,
                       leaveOpen: true))
            {
                requestBody = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
            }

            // 3) Response akışını yakalamak için tampon stream tak
            var originalBodyStream = context.Response.Body;
            await using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            // 4) Sonraki middleware’i çalıştır
            await _next(context);

            // 5) Cevabı oku ve orijinale dök
            responseBodyStream.Position = 0;
            string responseBody = await new StreamReader(responseBodyStream).ReadToEndAsync();
            responseBodyStream.Position = 0;
            await responseBodyStream.CopyToAsync(originalBodyStream);
            context.Response.Body = originalBodyStream;

            // 6) Endpoint’ten Controller/Action bilgisi al
            var endpoint = context.GetEndpoint();
            var cad = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();
            var controllerName = cad?.ControllerName;
            var actionName = cad?.ActionName;

            // 7) Kullanıcı GUID’i (JWT “sub” ya da NameIdentifier claim’i)
            Guid? userGuid = null;
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var sub = context.User.FindFirst("sub")?.Value
                       ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(sub, out var g)) userGuid = g;
            }

            // 8) Diğer bilgiler
            var path = $"{context.Request.Method} {context.Request.Path}{context.Request.QueryString}";
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();
            var browser = context.Request.Headers["User-Agent"].FirstOrDefault();

            // 9) Log objesini oluştur
            var log = new UserActivityLog
            {
                UserGuid = userGuid,
                ActivityDate = DateTime.UtcNow,
                ActivityType = $"{controllerName}.{actionName}",
                ActivityDetail = path,
                ActivityPage = context.Request.Path,
                Ipaddress = ipAddress,
                BrowserInfo = browser,
                AdditionalData = JsonSerializer.Serialize(new
                {
                    RequestBody = requestBody,
                    ResponseBody = responseBody,
                    StatusCode = context.Response.StatusCode
                })
            };

            // 10) Veritabanına kaydet
            try
            {
                await _activityLogDal.AddAsync(log);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UserActivityLog yazılırken hata oluştu.");
            }
        }
    }
}
