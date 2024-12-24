using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using NITNC_D1_Server.DataContext;
using Npgsql.EntityFrameworkCore.PostgreSQL;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<ApiDbInitializer>();

builder.AddServiceDefaults();

ApplicationDbContext._encryptionKey = Convert.FromBase64String(builder.Configuration["EncryptionKey"]?? "");
ApplicationDbContext._encryptionIV = Convert.FromBase64String(builder.Configuration["EncryptionIV"]?? "");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("d1system"), sqlOptions =>
    {
        sqlOptions.MigrationsAssembly("NITNC_D1_Server.MigrationService");
        sqlOptions.ExecutionStrategy(c => new NpgsqlRetryingExecutionStrategy(c));

   }));

builder.EnrichNpgsqlDbContext<ApplicationDbContext>(settings =>
    // Disable Aspire default retries as we're using a custom execution strategy
    settings.DisableRetry = true);

var host = builder.Build();

host.Run();
