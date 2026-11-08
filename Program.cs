using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using MiniERP.Data;
using MiniERP.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllersWithViews()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    x => x.Key,
                    x => x.Value!.Errors
                        .Select(e => e.ErrorMessage)
                        .ToArray()
                );

            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(
                new
                {
                    Message = "資料驗證失敗。",
                    Errors = errors
                });
        };
    });

builder.Services.AddOpenApi();


var databaseProvider =
    builder.Configuration["DatabaseProvider"] ?? "SqlServer";

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "找不到 DefaultConnection 連線字串。");

switch (databaseProvider)
{
    case "SqlServer":
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));
        break;

    case "PostgreSQL":
        builder.Services.AddDbContext<
            ApplicationDbContext,
            PostgreSqlApplicationDbContext>(
            options => options.UseNpgsql(connectionString));
        break;

    default:
        throw new InvalidOperationException(
            $"不支援的資料庫 Provider：{databaseProvider}");
}


builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;

        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;

        options.Lockout.AllowedForNewUsers = true;

        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.Zero;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";

    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode =
                StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }

        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode =
                StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }

        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };

    options.Events.OnValidatePrincipal = async context =>
    {
        var validator = context.HttpContext.RequestServices
            .GetRequiredService<ISecurityStampValidator>();

        await validator.ValidateAsync(context);

        if (context.Principal?.Identity?.IsAuthenticated != true)
            return;

        var userManager = context.HttpContext.RequestServices
            .GetRequiredService<UserManager<ApplicationUser>>();

        var user = await userManager.GetUserAsync(
            context.Principal
        );

        if (user == null || await userManager.IsLockedOutAsync(user))
        {
            context.RejectPrincipal();

            await context.HttpContext.SignOutAsync(
                IdentityConstants.ApplicationScheme
            );
        }
    };
});

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
});

builder.Services.AddExceptionHandler<MiniERP.Middleware.GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    // Render 的代理 IP 可能變動。
    // 僅在 Render Production 環境使用此設定。
    if (builder.Environment.IsProduction() &&
        builder.Configuration["RENDER"] == "true")
    {
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();

        options.ForwardLimit = 1;
    }
});


var app = builder.Build();

// 初始化角色與管理員
using (var scope = app.Services.CreateScope())
{
    await DbInitializer.SeedAsync(
        scope.ServiceProvider,
        app.Configuration);
}

// Render 的反向代理資訊應在 HTTPS 相關 Middleware 前處理
app.UseForwardedHeaders();

// 統一使用 GlobalExceptionHandler
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();

app.Run();
