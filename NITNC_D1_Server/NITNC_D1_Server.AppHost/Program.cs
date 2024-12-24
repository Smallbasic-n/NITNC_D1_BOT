using System.Security.Cryptography;

var builder = DistributedApplication.CreateBuilder(args);
var postgres = 
    builder.AddPostgres("pgsql",builder.AddParameter("UserName",secret: true),builder.AddParameter("Password",secret: true))
            .WithEnvironment("POSTGRES_DB", "d1system")
            .WithDataBindMount("./postgres1", false)
            .WithInitBindMount("./initialSQL",true)
            .WithImageTag("17.2")
            .WithPgAdmin()
            .AddDatabase("d1system");

var keyparam = builder.AddParameter("EncryptionKey", true);
var ivparam = builder.AddParameter("EncryptionIV", true);
if (string.IsNullOrWhiteSpace(keyparam.Resource.Value))
{
    
    using var aes = Aes.Create();
    aes.KeySize = 256;
    aes.GenerateKey();
    aes.GenerateIV();
    var key = Convert.ToBase64String(aes.Key);
    var iv = Convert.ToBase64String(aes.IV);
    Console.WriteLine(key);
    Console.WriteLine();
    Console.WriteLine(iv);
    keyparam=builder.AddParameter("EncryptionKey", key);
    ivparam=builder.AddParameter("EncryptionIV", iv);
}
builder.AddProject<Projects.ImagawaYoshimoto>("ImagawaYoshimoto").WithReference(postgres)
    .WithEnvironment("EncryptionKey",keyparam)
    .WithEnvironment("EncryptionIV", ivparam);

/*
builder.AddProject<Projects.AbrahamLincoln>("AbrahamLincoln").WithReference(postgres)
    .WithEnvironment("ClientId", builder.AddParameter("ClientId", true))
    .WithEnvironment("Token", builder.AddParameter("Token", true))
    .WithEnvironment("GuildId", builder.AddParameter("GuildId", true))
    .WithEnvironment("ChankChId", builder.AddParameter("ChankChId", true))
    .WithEnvironment("ChankRangeChId", builder.AddParameter("ChankRangeChId", true))
    .WithEnvironment("FactbookChId", builder.AddParameter("FactbookChId", true))
    .WithEnvironment("FactbookRangeChId", builder.AddParameter("FactbookRangeChId", true))
    .WithEnvironment("Interval", builder.AddParameter("Interval", true))
    .WithEnvironment("EncyptionKey",keyparam)
    .WithEnvironment("EncyptionIV", ivparam);

builder.AddProject<Projects.MatsudairaSadanobu>("MatsudairaSadanobu").WithReference(postgres)
    .WithEnvironment("ClientId", builder.AddParameter("ClientId", true))
    .WithEnvironment("Token", builder.AddParameter("Token", true))
    .WithEnvironment("GuildId", builder.AddParameter("GuildId", true))
    .WithEnvironment("JoinChId", builder.AddParameter("JoinChId", true))
    .WithEnvironment("EmailDomain", builder.AddParameter("EmailDomain", true))
    .WithEnvironment("EncryptionKey",keyparam)
    .WithEnvironment("EncryptionIV", ivparam);
*/

builder.AddProject<Projects.TairanoKiyomori>("TairanoKiyomori").WithReference(postgres)
    .WithEnvironment("ClientId", builder.AddParameter("ClientId", true))
    .WithEnvironment("Token", builder.AddParameter("Token", true))
    .WithEnvironment("GuildId", builder.AddParameter("GuildId", true))
    .WithEnvironment("ScheduleChId", builder.AddParameter("ScheduleChId", true))
    .WithEnvironment("EncryptionKey",keyparam)
    .WithEnvironment("EncryptionIV", ivparam);

builder.AddProject<Projects.NITNC_D1_Server_MigrationService>("migration")
    .WithReference(postgres)
    .WithEnvironment("EncryptionKey",keyparam)
    .WithEnvironment("EncryptionIV", ivparam);
builder.Build().Run();