using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using NITNC_D1_Server.DataContext;
//using NITNC_D1_Server.MigrationService;
using Npgsql.EntityFrameworkCore.PostgreSQL;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();

var conectionString = builder.Configuration.GetConnectionString("d1system");
ApplicationDbContext._encryptionKey = Convert.FromBase64String("QvCbcZwrdQV1IoND0E47bIIJkKBqB6H6V0IsK/vTi40=");//(builder.Configuration["EncryptionKey"]);
ApplicationDbContext._encryptionIV = Convert.FromBase64String("BKATsvpY7Uwu9AmdZtVj9Q==");//(builder.Configuration["EncryptionIV"]);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(conectionString, sqlOptions =>
    {
        sqlOptions.ExecutionStrategy(c => new NpgsqlRetryingExecutionStrategy(c));

   }));

var host = builder.Build();

using var scope = host.Services.CreateScope();
await using (var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
{
    await dbContext.Database.MigrateAsync();
}
host.Run();
