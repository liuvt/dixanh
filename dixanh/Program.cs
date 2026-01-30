using dixanh.Components;
using dixanh.Data;
using dixanh.Data.Seeds;
using dixanh.Libraries.Models;
using dixanh.Servers;
using dixanh.Servers.Contracts;
using dixanh.Services;
using dixanh.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Net;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//dotnet publish -c Release --output ./Publish dixanh.csproj
/*
sudo systemctl restart dixanh
sudo systemctl status dixanh --no-pager
*/

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddCircuitOptions(o => o.DetailedErrors = true);

// API: Add controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// API: call HTTP client Hub để lấy dữ liệu API từ bên ngoài
builder.Services.AddHttpClient();
builder.Services.AddScoped<CookieContainer>();

//Add identity
builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<dixanhDBContext>()
    .AddDefaultTokenProviders();

#region AddPooledDbContextFactory giúp giảm overhead tạo context (rất hợp cho Blazor Server). Connect to SQL Server
// AddDbContext vẫn giữ cho Identity
builder.Services.AddDbContext<dixanhDBContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration["ConnectionStrings:Vps"]
        ?? throw new InvalidOperationException("Can't found ConnectionStrings:Vps"));
    //opt.UseSqlServer(builder.Configuration["ConnectionStrings:Default"] ?? throw new InvalidOperationException("Can't found [Secret Key] in appsettings.json !"));

},
    contextLifetime: ServiceLifetime.Scoped,
    optionsLifetime: ServiceLifetime.Singleton);

// ✅ Thêm factory để dùng trong Services (tránh concurrency)
builder.Services.AddPooledDbContextFactory<dixanhDBContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration["ConnectionStrings:Vps"]
        ?? throw new InvalidOperationException("Can't found ConnectionStrings:Vps"));
    //opt.UseSqlServer(builder.Configuration["ConnectionStrings:Default"] ?? throw new InvalidOperationException("Can't found [Secret Key] in appsettings.json !"));

});
#endregion

// UI: Get httpClient API default
builder.Services.AddScoped(
    defaultClient => new HttpClient
    {
        //BaseAddress = new Uri(builder.Configuration["API:Default"] ?? throw new InvalidOperationException("Can't found [Secret Key] in appsettings.json !"))
        BaseAddress = new Uri(builder.Configuration["API:FontEnd"] ?? throw new InvalidOperationException("Can't found [Secret Key] in appsettings.json !"))
    });

// API: Add Jwt, Gooogle Authentication
// Sử dụng cả Cookie Identity (web nội bộ) và JWT Bearer (API)
builder.Services.AddAuthentication(options =>
{
    // Cookie Identity làm mặc định cho UI nội bộ
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, jwtBearerOptions =>
{
    jwtBearerOptions.RequireHttpsMetadata = false;
    jwtBearerOptions.SaveToken = true;
    jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            builder.Configuration["JWT:Secret"] ?? throw new InvalidOperationException("Missing JWT:Secret"))
        ),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
        ValidAudience = builder.Configuration["JWT:ValidAudience"]
    };
});

// API: Add SwaggerGen (dotnet add package Swashbuckle.AspNetCore)
builder.Services.AddSwaggerGen(
    opt =>
    {
        opt.SwaggerDoc("v1", new OpenApiInfo { Title = "Blazor Server Core API", Version = "v1" });
        opt.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            In = ParameterLocation.Header,
            Description = "Standard Authorization header using the Bearer scheme (\"bearer {token}\")"
        });

        //Add filter to block case authorize: Swashbuckle.AspNetCore.Filters
        opt.OperationFilter<SecurityRequirementsOperationFilter>();
    }
);

#region Back-end Register serivces
//ASP.NET Core server – Web API, MVC controller : [Authorize]
// Sử dụng cả Cookie Identity (web nội bộ) và JWT Bearer (API)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CookieOrJwt", policy =>
    {
        policy.AddAuthenticationSchemes(
            IdentityConstants.ApplicationScheme,
            JwtBearerDefaults.AuthenticationScheme);

        policy.RequireAuthenticatedUser();
    });
});

//For SQL Server
builder.Services.AddScoped<IAuthServer, AuthServer>();

// Register application services
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IVehicleStatusService, VehicleStatusService>();
builder.Services.AddScoped<IVehicleStatusHistoryService, VehicleStatusHistoryService>();
#endregion

#region Font-end Register services
// JWT (API/Postman/mobile)
builder.Services.AddScoped<IAuthJwtService, AuthJwtService>();
builder.Services.AddScoped<IAuthCookieService, AuthCookieService>();
// Sử dụng Cookie Identity không cần custom AuthenticationStateProvider
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

//For SQL Server
#endregion

builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = "dixanh.AntiForgery.v2.1";
    options.Cookie.SameSite = SameSiteMode.Lax; // tránh bị chặn bởi Facebook WebView
    options.Cookie.HttpOnly = true;

    // Host free CHỈ HTTP → dùng SameAsRequest (HTTP thì không Secure, HTTPS thì có Secure)
    // AUTO:
    // - Nếu request là HTTP  → cookie không gắn Secure
    // - Nếu request là HTTPS → cookie gắn Secure
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    /*
        Thuần HTTPS chuẩn, chỉ cần đổi thành:
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    */
});

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Unspecified;

    options.OnAppendCookie = ctx =>
    {
        if (ctx.CookieOptions.SameSite == SameSiteMode.None)
            ctx.CookieOptions.SameSite = SameSiteMode.Unspecified;
    };

    options.OnDeleteCookie = ctx =>
    {
        if (ctx.CookieOptions.SameSite == SameSiteMode.None)
            ctx.CookieOptions.SameSite = SameSiteMode.Unspecified;
    };
});

// Cookie không redirect 302 cho /api/* (trả 401)
builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath = "/login";

    opt.Events.OnRedirectToLogin = ctx =>
    {
        if (ctx.Request.Path.StartsWithSegments("/api"))
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }
        ctx.Response.Redirect(ctx.RedirectUri);
        return Task.CompletedTask;
    };
});

var app = builder.Build();

// apply migration trước
/*
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<dixanhDBContext>();
    await db.Database.MigrateAsync();
}
await SeedVehicles.SeedVehiclesAsync(app.Services);
await SeedIdentitys.SeedIdentityAsync(app.Services);
*/

// Đọc X-Forwarded-Proto, X-Forwarded-Host từ Nginx
/* var baseUrl = $"{Request.Scheme}://{Request.Host}";
 * Nếu bạn chạy Kestrel sau Nginx / reverse proxy mà chưa cấu hình ForwardedHeaders, thì:
    Bên ngoài user truy cập: https://tienichtaixe.io.vn
    Nhưng bên trong ASP.NET Core: Request.Scheme vẫn là "http"
    → Sitemap sẽ sinh link dạng: http://tienichtaixe.io.vn/...
    → Tool SEO / Google Search Console sẽ báo “sitemap dùng http nhưng site dùng https”.
    => VPS cần bổ sung thêm:
    proxy_set_header X-Forwarded-Proto $scheme;
    proxy_set_header X-Forwarded-Host  $host;
*/
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
});
// FIX HEAD 405 (đặt ngay sau ForwardedHeaders, trước Routing)
app.Use(async (ctx, next) =>
{
    if (HttpMethods.IsHead(ctx.Request.Method))
    {
        // Chuyển HEAD -> GET để match endpoint GET (Blazor/RazorComponents)
        ctx.Request.Method = HttpMethods.Get;

        // Nhưng vẫn đảm bảo không trả body như chuẩn HEAD
        var originalBody = ctx.Response.Body;
        ctx.Response.Body = Stream.Null;

        await next();

        ctx.Response.Body = originalBody;
        return;
    }

    await next();
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
else // API: Add run Swagger UI: 
{
    app.UseMigrationsEndPoint();
    app.UseSwagger();
    app.UseSwaggerUI(
        opt =>
        {
            opt.SwaggerEndpoint($"/swagger/v1/swagger.json", "Manager Business API V1");
        }
    );
}
app.UseHttpsRedirection();   // nếu host free không có HTTPS thì có thể tắt dòng này
app.UseStaticFiles();

app.UseCookiePolicy();           // ⭐ thêm dòng này

app.UseRouting();

// API: Add Authoz and Authen
app.UseAuthentication();
app.UseAuthorization();

// Anti-forgery middleware – để SAU routing, TRƯỚC MapEndpoints
app.UseAntiforgery();

app.MapControllers();

// Xử lý header iframe, CSP
app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        context.Response.Headers.Remove("X-Frame-Options");
        context.Response.Headers.Remove("Content-Security-Policy");
        context.Response.Headers["Content-Security-Policy"] = "frame-ancestors *";
        return Task.CompletedTask;
    });

    await next();
});
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();