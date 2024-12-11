using System.ComponentModel;
using System.Data;
using System.Globalization;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NITNC_D1_Server.DataContext;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using NpgsqlTypes;

namespace AbrahamLincoln;

class Program
{
    private static DiscordSocketClient Client;
    private static SocketGuild Guild;
    private static ApplicationDbContext Context;
    
    private static readonly ulong GuildId= Convert.ToUInt64(Environment.GetEnvironmentVariable("GuildId"));
    private static readonly ulong ChankChannel= Convert.ToUInt64(Environment.GetEnvironmentVariable("ChankChId"));
    private static readonly ulong ChankRangeChannel=Convert.ToUInt64(Environment.GetEnvironmentVariable("ChankRangeChId"));
    private static readonly ulong FactbookChannel=Convert.ToUInt64(Environment.GetEnvironmentVariable("FactbookChId"));
    private static readonly ulong FactbookRangeChannel=Convert.ToUInt64(Environment.GetEnvironmentVariable("FactbookRangeChId"));

    private static string ChankAnswer { get; set; } = "";
    private static string ChankRangeAnswer{ get; set; } = "";
    private static string FactbookAnswer { get; set; } = "";
    private static string FactbookRangeAnswer { get; set; } = "";

    private static int ChankStep { get; set; } = 0;
    private static int FactbookStart { get; set; } = 0;
    private static int FactbookEnd { get; set; } = 0;
    private static readonly int Waiting=Convert.ToInt32(Environment.GetEnvironmentVariable("Iteration"))*1000;
    static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        ApplicationDbContext._encryptionKey =
            Convert.FromBase64String(Environment.GetEnvironmentVariable("EncyptionKey")??"");
        ApplicationDbContext._encryptionIV =
            Convert.FromBase64String(Environment.GetEnvironmentVariable("EncyptionIV")??"");
        
        var builder = new ConfigurationBuilder()
            .AddEnvironmentVariables();
        var configuration = builder.Build();
        
        var services = new ServiceCollection()
            .AddSingleton(configuration)
            .AddSingleton(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers |
                                 GatewayIntents.MessageContent
            })
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton(config =>
                new InteractionService(config.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<InteractionHandler>()
            .AddDbContext<ApplicationDbContext>(options=>
                options.UseNpgsql(configuration.GetConnectionString("d1system"), sqlOptions =>
                {
                    sqlOptions.ExecutionStrategy(c => new NpgsqlRetryingExecutionStrategy(c));
                    
                }))
            .BuildServiceProvider();
        Client=services.GetRequiredService<DiscordSocketClient>();
        Context = services.GetRequiredService<ApplicationDbContext>();
        await services.GetRequiredService<InteractionHandler>().InitializeAsync();
        
        Client.Log += LogAsync;
        Client.Ready += ReadyAsync;
        Client.MessageReceived += MessageReceivedAsync;
        
        await Client.LoginAsync(tokenType: TokenType.Bot,Environment.GetEnvironmentVariable("Token"));
        await Client.StartAsync();
        await Task.Delay(-1);
    }

    private static Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log.ToString());
        return Task.CompletedTask;
    }

    private static Task ReadyAsync()
    {
        Console.WriteLine("logged in as "+Client.CurrentUser);
        Guild = Client.GetGuild(GuildId);
        var background = new BackgroundWorker();
        background.DoWork += async (sender, args) =>
        {
            while (true)
            {
                var data = Context.LincolnConfiguration.Single();
                ChankStep = data.RangeStep;
                FactbookStart = data.RangeStart;
                FactbookEnd = data.RangeEnds;

                await ChankAllRange();
                await ChankStepRange();
                await FactbookAllRange();
                await FactbookStaStpRange();
                Thread.Sleep(Waiting);
            }
        };
        background.RunWorkerAsync();
        
        return Task.CompletedTask;
    }
    
    private static async Task ChankAllRange()
    {
        var data=Context.ChankQuestions.Where(x => x.Step <= ChankStep).OrderBy(x => Guid.NewGuid()).Take(1).Single();
        ChankAnswer=data.Answer;
        await Guild.GetTextChannel(ChankChannel).SendMessageAsync($"日本語：{data.Japanese}\n英語：{data.English}");
    }
    
    private static async Task ChankStepRange()
    {
        var data=Context.ChankQuestions.Where(x => x.Step == ChankStep).OrderBy(x => Guid.NewGuid()).Take(1).Single();
        ChankAnswer=data.Answer;
        await Guild.GetTextChannel(ChankRangeChannel).SendMessageAsync($"日本語：{data.Japanese}\n英語：{data.English}");
    }
    
    private static async Task FactbookAllRange()
    {
        var data=Context.FactbookQuestions.Where(x => x.Id<= FactbookEnd ).OrderBy(x => Guid.NewGuid()).Take(1).Single();
        ChankAnswer=data.Answer;
        await Guild.GetTextChannel(FactbookChannel).SendMessageAsync($"日本語：{data.Japanese}");
    }
    
    private static async Task FactbookStaStpRange()
    {
        var data=Context.FactbookQuestions.Where(x => x.Id >= FactbookStart && x.Id<= FactbookEnd ).OrderBy(x => Guid.NewGuid()).Take(1).Single();
        ChankAnswer=data.Answer;
        await Guild.GetTextChannel(FactbookRangeChannel).SendMessageAsync($"日本語：{data.Japanese}");
    }
    
    private static async Task ScoreReg(bool isChank,ulong accountId)
    {
        var user=Context.MatsudairaDatas.Single(x => x.AccountId == accountId);
        if (isChank) user.Chank += 1;
        else user.FactBook += 1;
        await Context.SaveChangesAsync();
    }
    
    private static async Task MessageReceivedAsync(SocketMessage message)
    {
        if (message.Author.Id == Client.CurrentUser.Id) return;
        var channelId = message.Channel.Id.ToString();
        var isChankA = channelId == ChankChannel.ToString();
        var isChankB = channelId == ChankRangeChannel.ToString();
        var isFactA= channelId == FactbookChannel.ToString();
        var isFactB= channelId == FactbookRangeChannel.ToString();
        var answerData=isChankA ? ChankAnswer : isChankB ? ChankRangeAnswer : isFactA ? FactbookAnswer : FactbookRangeAnswer;
        if (!isChankA && !isChankB && !isFactA && !isFactB)  return;
        var emoji = "\ud83e\udd14";
        
        if (message.Content.Replace(" ", "").Replace("　", "").Replace(",", "").Replace("、", "").ToUpper() ==
            answerData.ToUpper())
        {
            emoji = "\ud83d\udcaf";
            await ScoreReg(isChankA||isChankB, message.Author.Id);
        }
        await message.AddReactionAsync(new Emoji(emoji));
    }
    
}