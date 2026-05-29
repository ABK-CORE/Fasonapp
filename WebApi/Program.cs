using Autofac;
using Autofac.Extensions.DependencyInjection;
using Business.Abstract;
using Business.DependencyRepository.Autofac;
using Core.Utilities.Security.Encryption;
using Core.Utilities.Security.JWT;
using DataAccess.Abstract;
using DataAccess.Concrete.EntityFramework;
using Enigma;
using Hangfire;
using Hangfire.SqlServer;
using Infrastructure.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Security.Claims;
using System.Security.Cryptography;

internal class Program
{
    private static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.File("logs/error_.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        var builder = WebApplication.CreateBuilder(args);
        IConfiguration configuration = builder.Configuration;

        builder.Services.Configure<KestrelServerOptions>(options =>
        {
            options.Limits.MaxRequestBodySize = null;
        });

        builder.Host.UseSerilog();
        builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
        builder.Host.ConfigureContainer<ContainerBuilder>(container =>
        {
            container.RegisterModule(new AutofacBusinessModule(builder.Configuration));
        });

        var aesSettings = configuration.GetSection("AesSettings").Get<AesSettings>();
        KeyCase.Instance.SetAesKeys(aesSettings.Key, aesSettings.Vektor);
        builder.Services.AddSingleton(KeyCase.Instance);

        Enigma.Processor processor = new Enigma.Processor();
        string encryptedConn = configuration["ConnectionStrings:DatabaseConnection"];
        string encryptedKey = configuration["JwtSettings:SecurityKey"];
        string connectionString = "", securityKey = "";

        try
        {
            using var aes = Aes.Create();
            connectionString = processor.DecryptorSymmetric(encryptedConn, aes);
            securityKey = processor.DecryptorSymmetric(encryptedKey, aes);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Şifre çözme hatası: " + ex.Message);
            // Uygulamanın burada sonlanması daha güvenli olabilir.
            return;
        }

        // 5. DbContext ve Dapper connection
        DataAccess.Concrete.Dapper.Context.ContextDb.Configure(builder.Configuration);
        builder.Services.AddDbContext<Entities.Concrete.EntityFramework.Context.ContextDb>(opts =>
            opts.UseSqlServer(connectionString));
        builder.Services.AddScoped(sp => new SqlConnection(DataAccess.Concrete.Dapper.Context.ContextDb.ConnectionStringDefault));

        // 6. Genel servisler
        builder.Services.AddMemoryCache();
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddTransient<CacheService>();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddHttpClient();
        builder.Services.AddSession(opts =>
        {
            opts.IdleTimeout = TimeSpan.FromMinutes(30);
            opts.Cookie.HttpOnly = true;
            opts.Cookie.IsEssential = true;
        });

        // ====================================================================
        // ===                    HANGFIRE YAPILANDIRMASI                   ===
        // ====================================================================

        // 1. Hangfire için appsettings.json'dan connection string'i al
        //var hangfireConnectionString = configuration.GetConnectionString("HangfireConnection");

        // 2. Hangfire servislerini DI container'a ekle
        //builder.Services.AddHangfire(config => config
        //    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
        //    .UseSimpleAssemblyNameTypeSerializer()
        //    .UseRecommendedSerializerSettings()
        //    .UseSqlServerStorage(hangfireConnectionString, new SqlServerStorageOptions
        //    {
        //        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        //        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        //        QueuePollInterval = TimeSpan.Zero,
        //        UseRecommendedIsolationLevel = true,
        //        DisableGlobalLocks = true
        //    }));

        // 3. Hangfire'ın arkaplan işlerini çalıştıracak sunucuyu ekle
        //builder.Services.AddHangfireServer(options =>
        //{
        //    options.WorkerCount = 20; // İhtiyaca göre ayarlanabilir
        //});

        // ====================================================================

        // 7. CORS
        var allowedOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? Array.Empty<string>();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", policy =>
            {
                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    //.AllowCredentials() // Sadece cookie/withCredentials gerekiyorsa açın
                    .SetIsOriginAllowedToAllowWildcardSubdomains();
            });
        });

        // 8. JWT Authentication
        var tokenOptions = configuration.GetSection("TokenOptions").Get<TokenOptions>();
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opts =>
            {
                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = tokenOptions.Issuer,
                    ValidAudience = tokenOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = SecurityKeyHelper.CreateSecurityKey(securityKey),
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = ClaimTypes.Role
                };
            });

        // 9. Swagger
        var projectName = configuration["ProjectSettings:ProjectName"];
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(opts =>
        {
            opts.SwaggerDoc("v1", new OpenApiInfo { Title = projectName + " API", Version = "v1" });
            opts.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Bearer {token}",
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey
            });
            opts.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
                    Array.Empty<string>()
                }
            });
        });

        // 10. MVC / Authorization
        builder.Services.AddControllers(options =>
        {
            var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            options.Filters.Add(new AuthorizeFilter(policy));
        });
        builder.Services.AddAutoMapper(typeof(Program).Assembly);

        // 11. Activity & Error Log Dal’lar
        builder.Services.AddScoped<IUserActivityLogDal, EfUserActivityLogDal>();
        builder.Services.AddScoped<IErrorLogDal, EfErrorLogDal>();

        // 12. Uygulamayı oluştur
        var app = builder.Build();

        // ===== MIDDLEWARE SIRALAMASI =====

        app.UseRouting();
        app.UseCors("CorsPolicy");

        app.Use(async (ctx, next) =>
        {
            if (HttpMethods.IsOptions(ctx.Request.Method))
            {
                ctx.Response.StatusCode = StatusCodes.Status204NoContent;
                return;
            }
            await next();
        });

        // Local dev: allow calling the HTTP endpoint (e.g. http://localhost:5109)
        // without forcing redirect to HTTPS (which can fail if the dev cert isn't trusted).
        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }
        app.UseStaticFiles();
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "MainFile")),
            RequestPath = "/MainFile"
        });

#if DEBUG // Sadece geliştirme ortamında çalışsın
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.RoutePrefix = projectName?.Trim();
            c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{projectName} API v1");
            c.InjectStylesheet("/MainFile/swagger-mycustom.css");
        });
        app.UseWhen(context => context.Request.Path.StartsWithSegments("/swagger"), swaggerApp =>
        {
            swaggerApp.UseMiddleware<SwaggerBasicAuthMiddleware>();
        });
#endif

        app.UseAuthentication();
        app.UseAuthorization();

        // ====================================================================
        // ===                 HANGFIRE DASHBOARD VE İŞLER                  ===
        // ====================================================================

        // Hangfire yönetim arayüzünü /hangfire adresinde aktif hale getirir.
        // Gerekirse bir yetkilendirme filtresi eklenebilir. Örn: new DashboardNoAuthorizationFilter()
        //app.UseHangfireDashboard();

        // Tekrarlanan (Recurring) işleri tanımla. Bu işler uygulama başlar başlamaz zamanlanır.
        // Görev 1: 5 saatte bir hatırlatma maili gönder.
        //RecurringJob.AddOrUpdate<INotificationJobService>(
        //    "pending-approval-reminder",
        //    job => job.SendPendingApprovalReminders(),
        //    "0 */5 * * *" // Her 5 saatin başında (0. dakika)
        //);

        // Görev 2: Her gün Türkiye saati ile 20:00'de özet maili gönder.
        // TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time") Windows için geçerlidir.
        // Linux için "Europe/Istanbul" kullanılmalıdır.
        //RecurringJob.AddOrUpdate<INotificationJobService>(
        //    "daily-summary-for-requesters",
        //    job => job.SendDailySummariesToRequesters(),
        //    "0 17 * * *", // UTC olarak 17:00, Türkiye saatiyle (GMT+3) 20:00'ye denk gelir.
        //    new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc }
        //);

        // ====================================================================

        app.MapControllers();

        app.Use(async (context, next) =>
        {
            await next();
            if (context.Response.StatusCode == 404 && !Path.HasExtension(context.Request.Path.Value))
            {
                context.Request.Path = "/index.html";
                await next();
            }
        });

        app.Run();
    }
}