using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using WeddingInvitation.Api.Logging;
using WeddingInvitation.Api.Middlewares;
using WeddingInvitation.Application.Abstractions;
using WeddingInvitation.Application.Options;
using WeddingInvitation.Application.Services;
using WeddingInvitation.Infrastructure.ConnectionStrings;
using WeddingInvitation.Infrastructure.Persistence;
using WeddingInvitation.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// 1. 连接串：从取串接口获取密文并按接口描述解密（AES-256-GCM），失败可回退本地连接串
// ---------------------------------------------------------------------------
using var bootstrapHttpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
var connectionString = await PostgreSqlConnectionStringResolver.ResolveAsync(builder.Configuration, bootstrapHttpClient);

// ---------------------------------------------------------------------------
// 2. 日志：控制台 + 调试 + 轻量文件 Provider（写 logs/api-yyyyMMdd.log）
//    说明：显式清空默认 Provider，避免 Windows 事件日志（EventLog）在无写权限的
//    运行账号下抛异常导致请求处理中断；如需事件日志，请单独 AddEventLog 并授予权限。
// ---------------------------------------------------------------------------
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddProvider(new FileLoggerProvider(
    Path.Combine(builder.Environment.ContentRootPath, "logs"),
    LogLevel.Information));

// ---------------------------------------------------------------------------
// 3. 配置绑定
// ---------------------------------------------------------------------------
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<UploadOptions>(builder.Configuration.GetSection(UploadOptions.SectionName));
builder.Services.PostConfigure<UploadOptions>(options =>
{
    options.RootPath = ResolveUploadRoot(builder.Environment.ContentRootPath, options.RootPath);
});
builder.Services.Configure<AdminSeedOptions>(builder.Configuration.GetSection(AdminSeedOptions.SectionName));
builder.Services.Configure<InviteOptions>(builder.Configuration.GetSection(InviteOptions.SectionName));

// ---------------------------------------------------------------------------
// 4. 数据访问与业务服务
// ---------------------------------------------------------------------------
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());
builder.Services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<InviteService>();
builder.Services.AddScoped<RsvpService>();
builder.Services.AddScoped<GuestService>();
builder.Services.AddScoped<PhotoService>();
builder.Services.AddScoped<ConfigService>();
builder.Services.AddScoped<StatisticsService>();
builder.Services.AddMemoryCache();

// ---------------------------------------------------------------------------
// 5. Web 基础设施：控制器 / 统一模型校验错误 / OpenAPI + Scalar / CORS / JWT
// ---------------------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var message = context.ModelState
            .SelectMany(kv => kv.Value?.Errors ?? [])
            .Select(e => e.ErrorMessage)
            .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m)) ?? "请求参数不合法";
        return new BadRequestObjectResult(new { code = 400, message });
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi("v1");

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddPolicy("default", policy =>
{
    if (allowedOrigins.Length == 0)
    {
        policy.AllowAnyOrigin();
    }
    else
    {
        policy.WithOrigins(allowedOrigins);
    }

    policy.AllowAnyHeader().AllowAnyMethod();
}));

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwtOptions.Key) || Encoding.UTF8.GetByteCount(jwtOptions.Key) < 32)
{
    throw new InvalidOperationException("请配置 Security:Jwt:Key（至少 32 字节）。");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });
builder.Services.AddAuthorization();

var hostingUrls = builder.Configuration["Hosting:Urls"];
if (!string.IsNullOrWhiteSpace(hostingUrls))
{
    builder.WebHost.UseUrls(hostingUrls.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
}

var app = builder.Build();

// ---------------------------------------------------------------------------
// 6. 中间件管线
// ---------------------------------------------------------------------------
app.UseMiddleware<ExceptionHandlingMiddleware>();

var openApiEnabled = app.Environment.IsDevelopment() || builder.Configuration.GetValue("OpenApi:Enabled", false);
if (openApiEnabled)
{
    // OpenAPI 文档：GET /openapi/v1.json
    app.MapOpenApi();
}

var uploadOptions = app.Services.GetRequiredService<IOptions<UploadOptions>>().Value;
var uploadRoot = uploadOptions.RootPath;
var uploadRootReady = false;
try
{
    Directory.CreateDirectory(uploadRoot);
    uploadRootReady = true;
}
catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Security.SecurityException)
{
    app.Logger.LogWarning(ex, "上传目录不可用：{UploadRoot}。应用将继续启动，但上传文件访问会不可用。", uploadRoot);
}

if (uploadRootReady)
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(uploadRoot),
        RequestPath = uploadOptions.RequestPath,
    });
}

app.UseCors("default");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

if (openApiEnabled)
{
    // Scalar API 文档 UI：GET /scalar/v1
    app.MapScalarApiReference("/scalar", options => options
        .WithTitle("婚礼电子请柬 API")
        .WithOpenApiRoutePattern("/openapi/{documentName}.json")
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient));
}

app.MapGet("/", () => Results.Redirect("~/scalar"))       .ExcludeFromDescription();
app.MapGet("/health", () => Results.Ok(new { status = "ok", time = DateTimeOffset.UtcNow }));

await DbInitializer.InitializeAsync(app.Services);

app.Run();

static string ResolveUploadRoot(string contentRootPath, string? configuredRootPath)
{
    var rootPath = string.IsNullOrWhiteSpace(configuredRootPath) ? "upload" : configuredRootPath;
    return Path.GetFullPath(Path.IsPathRooted(rootPath)
        ? rootPath
        : Path.Combine(contentRootPath, rootPath));
}
