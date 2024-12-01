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
builder.Build().Run();