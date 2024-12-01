using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ImagawaYoshimoto.Components;
using ImagawaYoshimoto.Components.Account;
using ImagawaYoshimoto.Data;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Radzen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services.AddRadzenComponents();
builder.Services.AddRadzenCookieThemeService();
builder.Services.AddRadzenQueryStringThemeService();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    }).AddMicrosoftAccount(options =>
{
    options.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"];
}).AddIdentityCookies();

var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__d1system");

builder.Configuration["ConnectionStrings:postgresdb"]=connectionString;


builder.AddNpgsqlDbContext<ApplicationDbContext>(connectionName: "d1system");
builder.EnrichNpgsqlDbContext<ApplicationDbContext>(
    configureSettings: settings =>
    {
        settings.ConnectionString = connectionString;
        settings.DisableRetry = false;
        settings.CommandTimeout = 30;
    });
  builder.Services.AddDbContextPool<ApplicationDbContext>(options =>
      options.UseNpgsql(connectionString, sqlOptions =>
      {
          sqlOptions.ExecutionStrategy(c => new NpgsqlRetryingExecutionStrategy(c));
      }));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(ImagawaYoshimoto.Client._Imports).Assembly);

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();