using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Data.SqlClient;
using wl.chat.Data;
using wl.chat.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<SiteSettings>(builder.Configuration.GetSection("SiteSettings"));

builder.Services.AddAuth0WebAppAuthentication(options => {
    options.Domain = builder.Configuration["Auth0:Domain"];
    options.ClientId = builder.Configuration["Auth0:ClientId"];
    options.ClientSecret = builder.Configuration["Auth0:ClientSecret"];
});

builder.Services.AddAuthorizationCore();

// Add services to the container.
builder.Services.AddSingleton<IMarkdownService, MarkdigService>();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddHttpClient();
builder.Services.AddScoped<ModelService>();
builder.Services.Configure<LMStudioOptions>(
    builder.Configuration.GetSection("LMStudio"));
builder.Services.AddScoped<ChatService>();
builder.Services.AddSingleton<ISqlDataService, SqlDataService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/login", async (HttpContext httpContext, string returnUrl = "/") =>
{
    var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
            .WithRedirectUri(returnUrl)
            .Build();

    await httpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
});

app.MapGet("/signup", async (HttpContext httpContext, string returnUrl = "/") =>
{
    var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
            .WithRedirectUri(returnUrl)
            .WithParameter("screen_hint", "signup")
            .Build();

    await httpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
});

app.MapGet("/logout", async (HttpContext httpContext) =>
{
    var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
            .WithRedirectUri("/")
            .Build();

    await httpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
});

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
