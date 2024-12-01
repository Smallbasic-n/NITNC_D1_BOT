using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ImagawaYoshimoto2nd1.Components;
using ImagawaYoshimoto2nd1.Components.Account;
using ImagawaYoshimoto2nd1.Data;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Radzen;

namespace ImagawaYoshimoto2nd1;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Add services to the container.

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents().AddInteractiveWebAssemblyComponents();

        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddScoped<IdentityUserAccessor>();
        builder.Services.AddScoped<IdentityRedirectManager>();
        builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
        builder.Services.AddRadzenComponents();
        builder.Services.AddRadzenCookieThemeService();
        builder.Services.AddRadzenQueryStringThemeService();
        
        builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            }).AddMicrosoftAccount(options =>
            {
                options.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"];
                options.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"];
            }).AddIdentityCookies();

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                               throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        
        builder.AddNpgsqlDbContext<ApplicationDbContext>("d1system");
        
        builder.Services.AddDbContextPool<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, sqlOptions =>
            {
                // Workaround for https://github.com/dotnet/aspire/issues/1023
                sqlOptions.ExecutionStrategy(c => new NpgsqlRetryingExecutionStrategy(c));
            }));
        builder.EnrichNpgsqlDbContext<ApplicationDbContext>(settings =>
            {
                settings.ConnectionString = connectionString;
                settings.DisableRetry = true;
            }
            // Disable Aspire default retries as we're using a custom execution strategy
            );
        
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

       builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
           .AddEntityFrameworkStores<ApplicationDbContext>()
           .AddSignInManager()
            .AddDefaultTokenProviders();
       
        builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
        
        
        
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        //app.UseHttpsRedirection();

        app.MapStaticAssets();
        app.UseAntiforgery();
        app.UseBlazorFrameworkFiles();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode().AddInteractiveWebAssemblyRenderMode();

        // Add additional endpoints required by the Identity /Account Razor components.
        app.MapAdditionalIdentityEndpoints();

        app.Run("http://localhost:5000/");
    }
}
