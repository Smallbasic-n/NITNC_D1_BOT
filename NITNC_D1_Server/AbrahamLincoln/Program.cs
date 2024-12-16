using System.ComponentModel;
using System.Data;
using System.Globalization;
using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
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
                                 GatewayIntents.MessageContent | GatewayIntents.Guilds
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
        Client.SlashCommandExecuted += ClientOnSlashCommandExecuted;
        
        await Client.LoginAsync(tokenType: TokenType.Bot,Environment.GetEnvironmentVariable("Token"));
        await Client.StartAsync();
        await Task.Delay(-1);
    }

    private static Task ClientOnSlashCommandExecuted(SocketSlashCommand arg)
    {
        if (Context.MatsudairaDatas.Count() <= 0)
        {
            arg.RespondAsync("松平定信にユーザ情報が一つも登録されていません．", ephemeral: true).Wait();
            return Task.CompletedTask;
        }
        switch (arg.Data.Name)
        {
            case "result":
                var chankChamp = Context.MatsudairaDatas.MaxBy(x => x.Chank);
                if (chankChamp.Chank <= 0)
                {
                    arg.RespondAsync("チャンクで英単語での正答者がいません．", ephemeral: true).Wait();
                    return Task.CompletedTask;
                }
        
                var factbookChamp = Context.MatsudairaDatas.MaxBy(x => x.FactBook);
                if (factbookChamp.FactBook <= 0)
                {
                    arg.RespondAsync("暗唱例文での正答者がいません．", ephemeral: true).Wait();
                    return Task.CompletedTask;
                }
                arg.RespondAsync(ephemeral: false,
                    embed: EmbedInstanceCreator(
                        "リンカン調べ！チャンクで英単語, 暗唱例文チャンプ！",
                        "チャンク・暗唱例文クイズの結果を表示します．",
                        new EmbedFieldBuilder()
                            .WithName("チャンクチャンプ")
                            .WithValue(chankChamp.AccountId),
                        new EmbedFieldBuilder()
                            .WithName("チャンク正答数")
                            .WithValue(chankChamp.Chank),
                        new EmbedFieldBuilder()
                            .WithName("FACTBOOKチャンプ")
                            .WithValue(factbookChamp.AccountId),
                        new EmbedFieldBuilder()
                            .WithName("FACTBOOK正答数")
                            .WithValue(factbookChamp.FactBook)
                    )
                ).Wait();
                break;
            case "myresult":
                var userData = Context.MatsudairaDatas.Single(x=>x.AccountId==arg.User.Id);
                arg.RespondAsync(ephemeral: true,
                    embed: EmbedInstanceCreator(
                        "リンカン調べ！チャンクで英単語, 暗唱例文チャンプ！",
                        "あなたのチャンク・暗唱例文クイズの結果を表示します．",
                        new EmbedFieldBuilder()
                            .WithName("チャンク正答数")
                            .WithValue(userData.Chank),
                        new EmbedFieldBuilder()
                            .WithName("FACTBOOK正答数")
                            .WithValue(userData.FactBook)
                    )
                ).Wait();
                break;
            case "range":
                var chankStep = -1;
                var factbookStart = -1;
                var factbookEnd = -1;
                foreach (var data1 in arg.Data.Options)
                {
                    switch (data1.Name)
                    {
                        case "chank":
                            chankStep=(int)data1.Value;
                            break;
                        case "start":
                            factbookStart=(int)data1.Value;
                            break;
                        case "end":
                            factbookEnd=(int)data1.Value;
                            break;
                    }
                }

                var data = Context.LincolnConfiguration.Single();
                if (factbookStart > 0 && factbookEnd > 0)
                {
                    data.RangeStart = factbookStart;
                    data.RangeEnds=factbookEnd;
                }
                if (chankStep>0) data.RangeStep = chankStep;
                Context.SaveChanges();
                arg.RespondAsync(ephemeral: true,
                    embed: EmbedInstanceCreator(
                        "リンカン調べ！チャンクで英単語, 暗唱例文チャンプ！",
                        "チャンク・暗唱例文クイズの出題範囲を表示・設定します．",
                        new EmbedFieldBuilder()
                            .WithName("チャンクSTEP")
                            .WithValue(data.RangeStep),
                        new EmbedFieldBuilder()
                            .WithName("FACTBOOK開始～終了")
                            .WithValue(data.RangeStart + "～"+data.RangeEnds)
                    )
                ).Wait();
                
                break;
        }
        return Task.CompletedTask;
    }

    private static Embed EmbedInstanceCreator(string title, string description, params EmbedFieldBuilder[] fields)
    {
        return new EmbedBuilder()
            .WithAuthor("松平定信")
            .WithDescription(description)
            .WithTitle(title)
            .WithFields(fields)
            .WithTimestamp(DateTimeOffset.Now)
            .WithFooter(
                new EmbedFooterBuilder()
                    .WithText("老中 松平定信 https://github.com/Smallbasic-n/NITNC_D1_BOT")
            ).Build();
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
        Guild.CurrentUser.ModifyAsync(x => x.Nickname = "エイブラハム・リンカン");
        
        var commandResult = new SlashCommandBuilder()
            .WithName("result")
            .WithDescription("チャンクで英単語，FACTBOOK これからの英文法 暗唱例文集のクイズでの全体成績を表示します．")
            .Build();
        var commandUserResult = new SlashCommandBuilder()
            .WithName("myresult")
            .WithDescription("チャンクで英単語，FACTBOOK これからの英文法 暗唱例文集のクイズでのあなたの成績を表示します．")
            .Build();
        var commandRange = new SlashCommandBuilder()
            .WithName("range")
            .WithDescription("チャンクで英単語，FACTBOOK これからの英文法 暗唱例文集のクイズ出題範囲を表示・設定します．")
            .AddOptions(
                new SlashCommandOptionBuilder()
                    .WithName("chank")
                    .WithRequired(false)
                    .WithDescription("チャンクで英単語の出題STEP番号を指定します．")
                    .WithType(ApplicationCommandOptionType.Integer),
                new SlashCommandOptionBuilder()
                    .WithName("start")
                    .WithRequired(false)
                    .WithDescription("FACTBOOK これからの英文法 暗唱例文集の出題開始番号を指定します．")
                    .WithType(ApplicationCommandOptionType.Integer),
                new SlashCommandOptionBuilder()
                    .WithName("end")
                    .WithRequired(false)
                    .WithDescription("FACTBOOK これからの英文法 暗唱例文集の出題終了番号を指定します．")
                    .WithType(ApplicationCommandOptionType.Integer)
                )
            .Build();
        
        try
        {
            Console.WriteLine("Registering Result Command");
            Guild.CreateApplicationCommandAsync(commandResult).Wait();
            Console.WriteLine("Registering UserResult Command");
            Guild.CreateApplicationCommandAsync(commandUserResult).Wait();
            Console.WriteLine("Registering Range Command");
            Guild.CreateApplicationCommandAsync(commandRange).Wait();
        }
        catch(HttpException exception)
        {
            var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
            Console.WriteLine(json);
        }

        if (Context.LincolnConfiguration.Count() <= 0)
        {
            Context.LincolnConfiguration.Add(
                new LincolnConfiguration() { RangeStep = 0, RangeStart = 1, RangeEnds = 2 });
            Context.SaveChanges();
        }
        var background = new BackgroundWorker();
        background.DoWork += async (sender, args) =>
        {
            while (true)
            {
                var data = Context.LincolnConfiguration.Single();
                ChankStep = data.RangeStep;
                FactbookStart = data.RangeStart;
                FactbookEnd = data.RangeEnds;
                if (Context.ChankQuestions.Count() != 0)
                {
                    await ChankAllRange();
                    await ChankStepRange();
                }
                if (Context.FactbookQuestions.Count() != 0)
                {
                    await FactbookAllRange();
                    await FactbookStaStpRange();
                }
                Thread.Sleep(Waiting);
            }
        };
        background.RunWorkerAsync();
        
        return Task.CompletedTask;
    }
    
    private static async Task ChankAllRange()
    {
        try
        {
            var data=Context.ChankQuestions.Where(x => x.Step <= ChankStep).OrderBy(x => Guid.NewGuid()).Take(1).Single();
            ChankAnswer=data.Answer;
            await Guild.GetTextChannel(ChankChannel).SendMessageAsync($"日本語：{data.Japanese}\n英語：{data.English}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    private static async Task ChankStepRange()
    {
        try
        {
            var data=Context.ChankQuestions.Where(x => x.Step == ChankStep).OrderBy(x => Guid.NewGuid()).Take(1).Single();
            ChankAnswer=data.Answer;
            await Guild.GetTextChannel(ChankRangeChannel).SendMessageAsync($"日本語：{data.Japanese}\n英語：{data.English}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    private static async Task FactbookAllRange()
    {
        try
        {
            var data=Context.FactbookQuestions.Where(x => x.Id<= FactbookEnd ).OrderBy(x => Guid.NewGuid()).Take(1).Single();
            FactbookAnswer=data.Answer;
            await Guild.GetTextChannel(FactbookChannel).SendMessageAsync($"日本語：{data.Japanese}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    private static async Task FactbookStaStpRange()
    {
        try
        {
            var data=Context.FactbookQuestions.Where(x => x.Id >= FactbookStart && x.Id<= FactbookEnd ).OrderBy(x => Guid.NewGuid()).Take(1).Single();
            FactbookRangeAnswer=data.Answer;
            await Guild.GetTextChannel(FactbookRangeChannel).SendMessageAsync($"日本語：{data.Japanese}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    private static async Task ScoreReg(bool isChank,ulong accountId)
    {
        if (Context.MatsudairaDatas.Count(x => x.AccountId == accountId) == 0)
        {
            var discordUser = Guild.Users.Single(x => x.Id == accountId);
            await discordUser.CreateDMChannelAsync();
            await discordUser.SendMessageAsync("エイブラハム・リンカンのサービスをご利用になられる場合は，まず松平定信botにご自身の名前と学籍番号をご登録下さい．\n" +
                                               "マニュアルは次のとおりです． https://github.com/Smallbasic-n/NITNC_D1_BOT?tab=readme-ov-file#%E6%9D%BE%E5%B9%B3%E5%AE%9A%E4%BF%A1" +
                                               "\nご協力よろしくお願いいたします．\n");
            return;
        }
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
        
        if (message.Content.Replace(" ", "").Replace("　", "").Replace(",", "").Replace("、", "").Replace(".","").ToUpper() ==
            answerData.Replace(" ", "").Replace("　", "").Replace(",", "").Replace("、", "").Replace(".","").ToUpper())
        {
            emoji = "\ud83d\udcaf";
            await ScoreReg(isChankA||isChankB, message.Author.Id);
        }
        await message.AddReactionAsync(new Emoji(emoji));
    }
    
}