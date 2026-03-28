using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Wcomas.Components;
using Wcomas.Data;
using Wcomas.Services;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.StaticFiles;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "Data", "Keys")));

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContextFactory<WcomasDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/access-denied";
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<UserSessionInfo>();
builder.Services.AddScoped<InquiryService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<BrandService>();
builder.Services.AddScoped<PatternService>();
builder.Services.AddSingleton<LoginService>();
builder.Services.AddScoped<WhatsAppNotificationService>();
builder.Services.AddSingleton<UserTrackerService>();
builder.Services.AddScoped<CircuitHandler, UserCircuitHandler>();

var app = builder.Build();

// Ensure database tables exist (SQLite specific)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WcomasDbContext>();
    db.Database.EnsureCreated();
    
    // Explicitly create VisitorLogs for existing DBs
    var sql = @"
        CREATE TABLE IF NOT EXISTS VisitorLogs (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            IpAddress TEXT NOT NULL,
            UserAgent TEXT,
            CurrentUrl TEXT,
            Country TEXT,
            City TEXT,
            Latitude REAL,
            Longitude REAL,
            IsLocated INTEGER,
            CreatedAt TEXT
        );";
    db.Database.ExecuteSqlRaw(sql);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Configure static file serving with 3D model support
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".glb"] = "model/gltf-binary";
provider.Mappings[".gltf"] = "model/gltf+json";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});
app.UseAntiforgery();


app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapPost("/auth/login", async (HttpContext context, [FromForm] string username, [FromForm] string password, [FromQuery] string? returnUrl, LoginService loginService) =>
{
    if (loginService.IsLockedOut(username))
    {
        var lockoutEnd = loginService.GetLockoutEnd(username);
        var remaining = (lockoutEnd ?? DateTime.UtcNow) - DateTime.UtcNow;
        return Results.Redirect($"/login?error=Account locked. Try again in {Math.Ceiling(remaining.TotalMinutes)} minutes.");
    }

    if (username == "admin" && password == "Wcomas2026")
    {
        loginService.ResetAttempts(username);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
        var authProperties = new AuthenticationProperties { IsPersistent = true };

        await context.SignInAsync("Cookies", new ClaimsPrincipal(claimsIdentity), authProperties);

        return Results.Redirect(string.IsNullOrEmpty(returnUrl) ? "/admin/products" : returnUrl);
    }

    loginService.RecordFailedAttempt(username);
    return Results.Redirect("/login?error=Invalid credentials");
}).DisableAntiforgery();

app.MapGet("/sitemap.xml", async (Wcomas.Data.WcomasDbContext db) =>
{
    var products = await db.Products.Select(p => p.Id).ToListAsync();
    var categories = await db.Categories.Select(c => c.Id).ToListAsync();
    var sb = new System.Text.StringBuilder();
    sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
    sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
    
    var baseUrl = "https://wcomas.com";
    var pages = new[] { "/", "/contact" };
    foreach (var page in pages)
    {
        sb.AppendLine($"<url><loc>{baseUrl}{page}</loc><changefreq>weekly</changefreq><priority>1.0</priority></url>");
    }

    foreach (var catId in categories)
    {
        sb.AppendLine($"<url><loc>{baseUrl}/category/{catId}</loc><changefreq>monthly</changefreq><priority>0.8</priority></url>");
    }

    foreach (var prodId in products)
    {
        sb.AppendLine($"<url><loc>{baseUrl}/product/{prodId}</loc><changefreq>weekly</changefreq><priority>0.9</priority></url>");
    }

    sb.AppendLine("</urlset>");
    return Results.Text(sb.ToString(), "application/xml");
});

app.Run();