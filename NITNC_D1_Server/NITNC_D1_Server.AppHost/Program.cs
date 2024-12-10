using Aspire.Hosting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

var builder = DistributedApplication.CreateBuilder(args);
var postgres = 
    builder.AddPostgres("pgsql",builder.AddParameter("UserName",secret: true),builder.AddParameter("Password",secret: true))
            .WithEnvironment("POSTGRES_DB", "d1system")
            .WithDataBindMount("./postgres1", false)
            .WithInitBindMount("./initialSQL",true)
            .WithPgAdmin()
            .AddDatabase("d1system");

builder.AddProject<Projects.ImagawaYoshimoto>("ImagawaYoshimoto").WithReference(postgres);
builder.AddProject<Projects.AbrahamLincoln>("AbrahamLincoln").WithReference(postgres)
    .WithEnvironment("ClientId", builder.AddParameter("ClientId", true))
    .WithEnvironment("Token", builder.AddParameter("Token", true))
    .WithEnvironment("GuildId", builder.AddParameter("GuildId", true))
    .WithEnvironment("ChankChId", builder.AddParameter("ChankChId", true))
    .WithEnvironment("ChankRangeChId", builder.AddParameter("ChankRangeChId", true))
    .WithEnvironment("FactbookChId", builder.AddParameter("FactbookChId", true))
    .WithEnvironment("FactbookRangeChId", builder.AddParameter("FactbookRangeChId", true))
    .WithEnvironment("Iteration", builder.AddParameter("Iteration", true));
builder.Build().Run();