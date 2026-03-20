using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Wcomas.Components;
using Wcomas.Data;
using Wcomas.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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
builder.Services.AddScoped<InquiryService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<BrandService>();
builder.Services.AddScoped<PatternService>();
builder.Services.AddSingleton<LoginService>();
builder.Services.AddScoped<WhatsAppNotificationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
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
        var remaining = lockoutEnd.Value - DateTime.UtcNow;
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

app.Run();