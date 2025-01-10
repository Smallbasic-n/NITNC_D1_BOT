using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DiscordBotBasic;

public class DiscordWorkerService(
    ILogger<DiscordWorkerService> logger,
    DiscordSocketClient client,
    IConfiguration configuration,
    InteractionHandler handler)
    : BackgroundService
{
    protected ulong GuildId;
    protected SocketGuild Guild { get; set; } 

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        GuildId = Convert.ToUInt64(configuration["GuildId"]);
        await handler.InitializeAsync();
        
        client.Log += LogAsync;
        client.Ready += ReadyAsync;
        await client.LoginAsync(tokenType: TokenType.Bot, configuration["Token"]);
        await client.StartAsync();
        
        await MainProcessAsync(stoppingToken);
        
    }

    protected virtual async Task MainProcessAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }

            await Task.Delay(100000, stoppingToken);
        }
    }
    protected virtual Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log.ToString());
        return Task.CompletedTask;
    }

    protected virtual async Task ReadyAsync()
    {
        Console.WriteLine("logged in as "+client.CurrentUser.Username);
        Guild= client.GetGuild(GuildId);
        await Guild.CurrentUser.ModifyAsync(x => x.Nickname = Supports.ApplicationName);
    }
}
