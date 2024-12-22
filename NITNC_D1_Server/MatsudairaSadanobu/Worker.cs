using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Google.Protobuf.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using NITNC_D1_Server.DataContext;
using Npgsql;

namespace MatsudairaSadanobu;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IDbContextFactory<ApplicationDbContext> Factroy;
    private readonly DiscordSocketClient Client;
    private readonly CommandService Commands;
    private readonly IConfiguration Configuration;
    private readonly DiscordBotBasic.InteractionHandler Handler;
    private ulong GuildId;
    
    public Worker(ILogger<Worker> logger, DiscordSocketClient client, IConfiguration configuration, DiscordBotBasic.InteractionHandler handler)
    {
        _logger = logger;
        Client = client;
        Configuration = configuration;
        Handler = handler;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        GuildId = Convert.ToUInt64(Configuration["GuildId"]);
        await Handler.InitializeAsync();
        
        Client.Log += LogAsync;
        Client.Ready += ReadyAsync;
        Client.UserJoined += ClientOnUserJoined;
        Client.ButtonExecuted += ClientOnButtonExecuted;
        
        await Client.LoginAsync(tokenType: TokenType.Bot, Configuration["Token"]);
        await Client.StartAsync();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }

            await Task.Delay(100000, stoppingToken);
        }
    }

    private Task ClientOnButtonExecuted(SocketMessageComponent arg)
    {
        var customId = arg.Data.CustomId;
        if (string.IsNullOrWhiteSpace(customId) || customId[..5] != "role-") return Task.CompletedTask;
        var role = Convert.ToUInt64(customId[5..]);
        var user = Client.GetGuild(GuildId).GetUser(arg.User.Id);
        var isDel = true;
        if (user.Roles.Any(x => x.Id == role)) user.RemoveRoleAsync(role);
        else
        {
            user.AddRoleAsync(role);
            isDel = false;
        }
        arg.RespondAsync(ephemeral: true, text: "あなたをロール" + Client.GetGuild(GuildId).GetRole(role).Name + (isDel?"から除外":"に追加")+"しました．");
        return Task.CompletedTask;
    }

    private Task ClientOnUserJoined(SocketGuildUser arg)
    {
        if (arg.IsBot || arg.IsWebhook) return Task.CompletedTask;
        arg.Guild.GetTextChannel(Convert.ToUInt64(Configuration["JoinChId"]))
            .SendMessageAsync("<@" + arg.Id + ">さん，こんにちは．D1サーバで，人事担当をしております，松平定信と申します．" +
                                    arg.Guild.Name +"サーバに参加を歓迎します．まずは，次のWebサイトを参考に，私にあなたの名前と学籍番号を登録して下さい． " +
                                    "https://github.com/Smallbasic-n/NITNC_D1_BOT?tab=readme-ov-file#%E6%9D%BE%E5%B9%B3%E5%AE%9A%E4%BF%A1 " +
                                    "みんなでよいサーバを作っていきましょう！");
        return Task.CompletedTask;
    }

    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log.ToString());
        return Task.CompletedTask;
    }

    private async Task ReadyAsync()
    {
        Console.WriteLine("logged in as "+Client.CurrentUser.Username);
        await Client.GetGuild(GuildId).CurrentUser.ModifyAsync(x => x.Nickname = "松平定信");
    }
}
