using System.Globalization;
using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using NITNC_D1_Server.DataContext;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using static DiscordBotBasic.Supports;

namespace MatsudairaSadanobu;

public class Program
{
    
    private static readonly InteractionServiceConfig _interactionServiceConfig = new()
    {
        
    };
    public static void Main(string[] args)
    {

        ApplicationName = "松平定信";
        ApplicationPrefix = "老中";
        
        var builder = Host.CreateApplicationBuilder(args);

        builder.Configuration.AddEnvironmentVariables();
        ApplicationDbContext._encryptionKey= Convert.FromBase64String(builder.Configuration["EncryptionKey"]);
        ApplicationDbContext._encryptionIV = Convert.FromBase64String(builder.Configuration["EncryptionIV"]);
        builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("d1system"), sqlOptions =>
            {
                sqlOptions.ExecutionStrategy(c => new NpgsqlRetryingExecutionStrategy(c));

            }));
        builder.Services.AddSingleton(new DiscordSocketConfig()
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers |
                             GatewayIntents.MessageContent | GatewayIntents.Guilds | GatewayIntents.GuildMessageReactions | GatewayIntents.All,
            UseInteractionSnowflakeDate = false
        });
        
        builder.Services.AddSingleton<DiscordSocketClient>();
        builder.Services.AddHostedService<Worker>();
        builder.Services.AddSingleton(x =>
            new InteractionService(x.GetRequiredService<DiscordSocketClient>(), _interactionServiceConfig));

        builder.Services.AddSingleton<Modules.WhoisCommandModule>();
        builder.Services.AddSingleton<Modules.RoleCommandModule>();
        builder.Services.AddSingleton<DiscordBotBasic.InteractionHandler>();

        builder.EnrichNpgsqlDbContext<ApplicationDbContext>();
        
        var host = builder.Build();
        
        host.Run();
    }
}