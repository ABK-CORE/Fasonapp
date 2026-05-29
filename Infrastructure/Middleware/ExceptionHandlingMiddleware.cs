using Core.Utilities.Result.Concrete;
using Entities.Concrete.EntityFramework.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IErrorLogDal _errorLogDal;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IErrorLogDal errorLogDal)
        {
            _next = next;
            _logger = logger;
            _errorLogDal = errorLogDal;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // 1) İstek gövdesini oku (isteğe bağlı)
            string? userInput = null;
            try
            {
                context.Request.EnableBuffering();
                using var reader = new StreamReader(
                    context.Request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    leaveOpen: true);
                userInput = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
            }
            catch { /* okunamazsa atla */ }

            // 2) Route bilgilerini al
            var routeValues = context.GetRouteData().Values;
            var controllerName = routeValues["controller"]?.ToString();
            var actionName = routeValues["action"]?.ToString();

            // 3) StackTrace’den ilk Service/Manager sınıfını bul
            var trace = new StackTrace(exception, true);
            var frames = trace.GetFrames() ?? Array.Empty<StackFrame>();

            string? serviceName = frames
                .Select(f => f.GetMethod()?.DeclaringType?.Name)
                .FirstOrDefault(n => !string.IsNullOrEmpty(n) && n!.EndsWith("Service"));

            string? managerName = frames
                .Select(f => f.GetMethod()?.DeclaringType?.Name)
                .FirstOrDefault(n => !string.IsNullOrEmpty(n) && n!.EndsWith("Manager"));

            // 4) Kullanıcı GUID’i (jwt vs) al (örnek claim “sub”)
            Guid userGuid = Guid.Empty;
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var sub = context.User.FindFirst("sub")?.Value;
                if (Guid.TryParse(sub, out var g)) userGuid = g;
            }

            // 5) ErrorLog nesnesini oluştur
            var log = new ErrorLog
            {
                UserGuid = userGuid,
                ControllerName = controllerName,
                ActionName = actionName,
                ServiceName = serviceName,
                ManagerName = managerName,
                Message = exception.Message,
                StackTrace = exception.ToString(),
                UserInput = userInput,
                DateCreated = DateTime.UtcNow,
                System = "Purchase Order App"
            };

            // 6) Veritabanına kaydet
            await _errorLogDal.AddAsync(log);

            // 7) Kullanıcıya standart dönüş
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.OK;

            var result = new ErrorDataResult<object>(null, exception.Message);
            var json = JsonSerializer.Serialize(result);
            await context.Response.WriteAsync(json);
        }
    }
}
