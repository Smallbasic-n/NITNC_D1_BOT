using System.Security.Cryptography;

var builder = DistributedApplication.CreateBuilder(args);
var postgres = 
    builder.AddPostgres("pgsql",password:builder.AddParameter("Password",secret: true),port: 5432)
            .WithEnvironment("POSTGRES_DB", "d1system")
            .WithDataBindMount("./postgres1")
            .AddDatabase("d1system");

var keyparam = builder.AddParameter("EncryptionKey", true);
var ivparam = builder.AddParameter("EncryptionIV", true);

var migration=builder.AddProject<Projects.NITNC_D1_Server_MigrationService>("migrations").WithReference(postgres)
    .WaitFor(postgres)
    .WithEnvironment("EncryptionKey",keyparam)
    .WithEnvironment("EncryptionIV", ivparam);
var email = builder.AddParameter("EmailDomain");
var guildId = builder.AddParameter("GuildId");

/*builder.AddProject<Projects.ImagawaYoshimoto>("imagawayoshimoto").WithReference(postgres)
    .WaitForCompletion(migration)
    .WithEnvironment("Microsoft-ClientId",builder.AddParameter("Microsoft-ClientId", true))
    .WithEnvironment("Microsoft-ClientSecret",builder.AddParameter("Microsoft-ClientSecret", true))
    .WithEnvironment("EmailDomain",email)
    .WithEnvironment("EmailDept", builder.AddParameter("EmailDept"))
    .WithEnvironment("Supervisor", builder.AddParameter("Supervisor"))
    .WithEnvironment("NoDeptStudent", builder.AddParameter("NoDeptStudent"))
    .WithEnvironment("NoKosenStudent", builder.AddParameter("NoKosenStudent"))
    .WithEnvironment("ASPNETCORE_URLS","https://+:5002;http://+:5001")
    .WithEnvironment("ASPNETCORE_HTTPS_PORTS","5002")
    .WithEnvironment("EncryptionKey",keyparam)
    .WithEnvironment("EncryptionIV", ivparam);

builder.AddProject<Projects.AbrahamLincoln>("abrahamlincoln").WithReference(postgres)
   .WithEnvironment("ClientId", builder.AddParameter("Lincoln-ClientId"))
   .WithEnvironment("Token", builder.AddParameter("Lincoln-Token", true))
   .WithEnvironment("GuildId", guildId)
   .WithEnvironment("ChankChId", builder.AddParameter("ChankChId"))
   .WithEnvironment("ChankRangeChId", builder.AddParameter("ChankRangeChId"))
   .WithEnvironment("FactbookChId", builder.AddParameter("FactbookChId"))
   .WithEnvironment("FactbookRangeChId", builder.AddParameter("FactbookRangeChId"))
   .WithEnvironment("Interval", builder.AddParameter("Interval"))
   .WithEnvironment("EncryptionKey",keyparam)
   .WithEnvironment("EncryptionIV", ivparam);
*/
builder.AddProject<Projects.MatsudairaSadanobu>("matsudairasadanobu").WithReference(postgres)
   .WithEnvironment("ClientId", builder.AddParameter("Matsudaira-ClientId"))
   .WithEnvironment("Token", builder.AddParameter("Matsudaira-Token", true))
   .WithEnvironment("GuildId", guildId)
   .WithEnvironment("JoinChId", builder.AddParameter("JoinChId"))
   .WithEnvironment("EmailDomain", email)
   .WithEnvironment("EncryptionKey",keyparam)
   .WithEnvironment("EncryptionIV", ivparam);
   /*
builder.AddProject<Projects.TairanoKiyomori>("tairanokiyomori").WithReference(postgres)
    .WithEnvironment("ClientId", builder.AddParameter("Kiyomori-ClientId", true))
    .WithEnvironment("Token", builder.AddParameter("Kiyomori-Token", true))
    .WithEnvironment("GuildId", guildId)
    .WithEnvironment("ScheduleChId", builder.AddParameter("ScheduleChId"))
    .WithEnvironment("EncryptionKey",keyparam)
    .WithEnvironment("EncryptionIV", ivparam);
*/
builder.Build().Run();