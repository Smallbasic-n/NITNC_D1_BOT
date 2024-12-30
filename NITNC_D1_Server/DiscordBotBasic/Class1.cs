using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NITNC_D1_Server.Data;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace DiscordBotBasic;

public static class Supports
{
    /// <summary>
    /// アプリケーションの名前　例：今川義元
    /// </summary>
    public static string ApplicationName { get; private set; } = "";
    /// <summary>
    /// アプリケーション名の接頭辞　例：駿河国
    /// </summary>
    public static string ApplicationPrefix { get; private set; } = "";
    /// <summary>
    /// 指定された情報をもとにEmbedメッセージを作成します．
    /// </summary>
    /// <param name="title">Embedメッセージのタイトル</param>
    /// <param name="description">Embedメッセージの詳細</param>
    /// <param name="fields">載せる情報．任意</param>
    /// <returns>作成されたEmbed</returns>
    public static Embed EmbedInstanceCreator(string title, string description, params EmbedFieldBuilder[] fields)
    {
        return new EmbedBuilder()
            .WithAuthor(ApplicationName)
            .WithDescription(description)
            .WithTitle(title)
            .WithFields(fields)
            .WithTimestamp(DateTimeOffset.Now)
            .WithFooter(
                new EmbedFooterBuilder()
                    .WithText(ApplicationPrefix+" "+ApplicationName+" https://github.com/Smallbasic-n/NITNC_D1_BOT")
            ).Build();
    }
    
    /// <summary>
    /// 開始関数．これをMain関数呼び出すことでサービスを構成・実行できます．
    /// </summary>
    /// <param name="args">コマンドライン引数</param>
    /// <param name="name">アプリケーションの名前を指定します．例：今川義元</param>
    /// <param name="prefix">アプリケーション名の接頭辞を指定します．例：駿河国</param>
    /// <param name="addServices">メインのサービスとインタラクション・サービスを追加します．</param>
    public static void Start(string[] args,string name,string prefix,Action<IServiceCollection> addServices)
    {

        ApplicationName = name;
        ApplicationPrefix = prefix;
        
        var builder = Host.CreateApplicationBuilder(args);

        builder.Configuration.AddEnvironmentVariables();
        ApplicationDbContext._encryptionKey= Convert.FromBase64String(builder.Configuration["EncryptionKey"]??"");
        ApplicationDbContext._encryptionIV = Convert.FromBase64String(builder.Configuration["EncryptionIV"]??"");
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
        
        builder.Services.AddSingleton(x =>
            new InteractionService(x.GetRequiredService<DiscordSocketClient>(), new InteractionServiceConfig()));

        builder.Services.AddSingleton<InteractionHandler>();
        addServices(builder.Services);

        builder.EnrichNpgsqlDbContext<ApplicationDbContext>();
        
        var host = builder.Build();
        
        host.Run();
    }
}