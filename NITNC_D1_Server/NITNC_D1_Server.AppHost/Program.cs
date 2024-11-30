using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);
var postgres =
        builder.AddPostgresContainer("pgsql")
            .WithEnvironment("POSTGRES_DB", "d1system")
            .WithVolumeMount("./postgres1", "/var/lib/postgresql/data")
            .WithVolumeMount("./init.sql","/docker-entrypoint-initdb.d/init.sql", VolumeMountType.Bind,true)
            .AddDatabase("d1system")
    ;

builder.AddProject<Projects.ImagawaYoshimoto>("ImagawaYoshimoto").WithReference(postgres).WithEnvironment(options =>
{
    options.EnvironmentVariables.Add("CONNECTION_STRING", postgres.Resource.GetConnectionString());
});
builder.Build().Run();