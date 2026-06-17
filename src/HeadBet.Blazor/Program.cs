using System.Globalization;
using HeadBet.Blazor.Components;
using HeadBet.Blazor.Infrastructure.Data;
using HeadBet.Blazor.Infrastructure.Identity;
using HeadBet.Core.Domain.Interfaces;
using HeadBet.Core.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Camada Core (EF, Mediator, AutoMapper, repositórios, serviços de domínio) ---
builder.Services.AddCore(builder.Configuration);

// --- Identidade do request (depende de AuthenticationStateProvider no Blazor) ---
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, BlazorUserContext>();
builder.Services.AddScoped<SignInService>();

// --- Data Protection: persiste as chaves em disco com app-name fixo.
//     Sem isso, hosts compartilhados regeneram as chaves
//     a cada recycle do app pool, invalidando todos os cookies de auth. ---
var keysPath = Path.Combine(builder.Environment.ContentRootPath, "DataProtection-Keys");
Directory.CreateDirectory(keysPath);
builder.Services
    .AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
    .SetApplicationName("HeadBet.Blazor");

// --- MudBlazor ---
builder.Services.AddMudServices(opts =>
{
    opts.SnackbarConfiguration.PositionClass = MudBlazor.Defaults.Classes.Position.BottomRight;
    opts.SnackbarConfiguration.PreventDuplicates = false;
    opts.SnackbarConfiguration.NewestOnTop = true;
    opts.SnackbarConfiguration.VisibleStateDuration = 4000;
    opts.SnackbarConfiguration.HideTransitionDuration = 300;
    opts.SnackbarConfiguration.ShowTransitionDuration = 300;
});

// --- Autenticação via cookie (independente do MVC: cookie próprio) ---
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/account/login";
        options.LogoutPath = "/account/logout";
        options.AccessDeniedPath = "/account/access-denied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.Cookie.Name = "HeadBet.Blazor.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

// --- Blazor Web App: Razor Components + Interactive Server ---
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// --- Antiforgery para endpoints de auth ---
builder.Services.AddAntiforgery();

var app = builder.Build();

// --- Migrations + seeds ---
await app.InitializeAsync();

// --- Atrás de proxy reverso (Nginx no host, TLS terminado lá): honra
//     X-Forwarded-Proto/For para o app saber que o request original é HTTPS.
//     Sem isso, o Blazor negocia o circuito com esquema errado (http://localhost:5000)
//     e o UseHttpsRedirection não acha a porta https ("Failed to determine the https port").
//     Precisa ser o PRIMEIRO middleware. Kestrel só escuta em localhost, então o único
//     upstream é o proxy local — seguro limpar as listas de proxies/redes confiáveis. ---
var forwardedOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
};
forwardedOptions.KnownIPNetworks.Clear();
forwardedOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedOptions);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    app.UseHsts();
}

var ptBr = new CultureInfo("pt-BR");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(ptBr),
    SupportedCultures = [ptBr],
    SupportedUICultures = [ptBr],
});

app.UseHttpsRedirection();
app.MapStaticAssets();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// Endpoints de auth (sign-in/sign-out via MVC pattern, fora do circuito Blazor)
app.MapPost("/account/sign-in", (Delegate)SignInService.SignInEndpoint).DisableAntiforgery();
app.MapPost("/account/sign-out", (Delegate)SignInService.SignOutEndpoint).DisableAntiforgery();
app.MapPost("/account/change-password", (Delegate)PasswordEndpoints.ChangePasswordEndpoint).DisableAntiforgery();
app.MapPost("/account/forgot-password", (Delegate)PasswordEndpoints.ForgotPasswordEndpoint).DisableAntiforgery();
app.MapPost("/account/reset-password", (Delegate)PasswordEndpoints.ResetPasswordEndpoint).DisableAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
