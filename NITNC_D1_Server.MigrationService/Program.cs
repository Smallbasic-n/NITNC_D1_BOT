using NITNC_D1_Server.Data;
using NITNC_D1_Server.MigrationService;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddHostedService<Worker>();

ApplicationDbContext._encryptionKey = Convert.FromBase64String(Environment.GetEnvironmentVariable("EncryptionKey"));
ApplicationDbContext._encryptionIV = Convert.FromBase64String(Environment.GetEnvironmentVariable("EncryptionIV"));
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(Worker.ActivitySourceName));

builder.AddNpgsqlDbContext<ApplicationDbContext>("d1system");

var host = builder.Build();
host.Run();