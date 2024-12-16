using System.Drawing;
using System.Text;
using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NITNC_D1_Server.DataContext;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Color = Discord.Color;

namespace MatsudairaSadanobu;

class Program
{
    
    private static DiscordSocketClient Client;
    private static SocketGuild Guild;
    private static ApplicationDbContext Context;
    
    private static readonly ulong GuildId= Convert.ToUInt64(Environment.GetEnvironmentVariable("GuildId"));
    private static readonly ulong JoinChId= Convert.ToUInt64(Environment.GetEnvironmentVariable("JoinChId"));
    private static readonly string EmailDomain= Convert.ToString(Environment.GetEnvironmentVariable("EmailDomain"));
    
    static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build();
        
        ApplicationDbContext._encryptionKey= Convert.FromBase64String(Environment.GetEnvironmentVariable("EncryptionKey"));
        ApplicationDbContext._encryptionIV = Convert.FromBase64String(Environment.GetEnvironmentVariable("EncryptionIV"));
        
        
        var services = new ServiceCollection()
            .AddSingleton(configuration)
            .AddSingleton(new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers |
                                 GatewayIntents.MessageContent | GatewayIntents.Guilds | GatewayIntents.GuildMessageReactions | GatewayIntents.All,
                
            })
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton(config =>
                new InteractionService(config.GetRequiredService<DiscordSocketClient>()))
            .AddDbContext<ApplicationDbContext>(options=>
                options.UseNpgsql(configuration.GetConnectionString("d1system"), sqlOptions =>
                {
                    sqlOptions.ExecutionStrategy(c => new NpgsqlRetryingExecutionStrategy(c));
                    
                }))
            .BuildServiceProvider();
        
        Client = services.GetRequiredService<DiscordSocketClient>();
        Context = services.GetRequiredService<ApplicationDbContext>();
        Client.Log += LogAsync;
        Client.Ready += ReadyAsync;
        Client.SlashCommandExecuted += SlashCommandHandler;
        
        Client.ReactionAdded += ReactionAdded;
        Client.ReactionRemoved += ReactionRemoved;
        Client.UserJoined += ClientOnUserJoined;
        
        await Client.LoginAsync(tokenType: TokenType.Bot,Environment.GetEnvironmentVariable("Token"));
        await Client.StartAsync();
        await Task.Delay(-1);
    }

    private static Task ClientOnUserJoined(SocketGuildUser arg)
    {
        if (arg.IsBot || arg.IsWebhook) return Task.CompletedTask;
        Guild.GetTextChannel(JoinChId).SendMessageAsync("<@" + arg.Id + ">さん，こんにちは．D1サーバで，人事担当をしております，松平定信と申します．" +
                                                        Guild.Name +"サーバに参加を歓迎します．まずは，次のWebサイトを参考に，私にあなたの名前と学籍番号を登録して下さい． " +
                                                        "https://github.com/Smallbasic-n/NITNC_D1_BOT?tab=readme-ov-file#%E6%9D%BE%E5%B9%B3%E5%AE%9A%E4%BF%A1 " +
                                                        "みんなでよいサーバを作っていきましょう！");
        return Task.CompletedTask;
    }

    private static List<SurveyItem> emojis = new ();
    private static ulong surveyMessageId = 0;
    private static async Task SlashCommandHandler(SocketSlashCommand arg)
    {
        switch (arg.Data.Name)
        {
            case "iam":
                var givenname = "";
                var firstname = "";
                var rohmegivenname = "";
                var rohmefirstname = "";
                var studentId = "";
                foreach (var opts in arg.Data.Options)
                {
                    switch (opts.Name)
                    {
                        case "givenname":
                            givenname = (string)opts.Value;
                            break;
                        case "firstname":
                            firstname = (string)opts.Value;
                            break;
                        case "rohmegivenname":
                            rohmegivenname = (string)opts.Value;
                            break;
                        case "rohmefirstname": 
                            rohmefirstname = (string)opts.Value;
                            break;
                        case "studentid":
                            studentId = (string)opts.Value;
                            break;
                        default:
                            break;
                    }
                }

                var count = Context.MatsudairaDatas.Count(x => x.AccountId == arg.User.Id);
                // (x => x.AccountId == arg.User.Id);
                if (count==0)
                {
                    Context.MatsudairaDatas.Add(new MatsudairaDatas()
                    {
                        AccountId = arg.User.Id, Chank = 0, Email = studentId + "@"+EmailDomain, FactBook = 0,
                        FirstName = firstname, GivenName = givenname.ToUpper(), RohmeFirstName = rohmefirstname, RohmeGivenName = rohmegivenname
                    });
                }
                else
                {
                    var existsuser = Context.MatsudairaDatas.FirstOrDefault(x => x.AccountId == arg.User.Id);
                    existsuser.Email = studentId + "@"+EmailDomain;
                    existsuser.FirstName = firstname;
                    existsuser.GivenName = givenname;
                    existsuser.RohmeFirstName = rohmefirstname;
                    existsuser.RohmeGivenName = rohmegivenname.ToUpper();
                }
                await Context.SaveChangesAsync();
                
                await arg.RespondAsync(
                    ephemeral: false,
                    text: "<@"+arg.User.Id+">",
                    embed: EmbedInstanceCreator(
                        "@" + arg.User.Username + " の中の人",
                        "アカウントの中の人の情報を松平定信公に登録しました．",
                        new EmbedFieldBuilder()
                                        .WithName("名前")
                                        .WithValue(givenname + "　" + firstname),
                                    new EmbedFieldBuilder()
                                        .WithName("Name")
                                        .WithValue(rohmegivenname + "　" + rohmefirstname),
                                    new EmbedFieldBuilder()
                                        .WithName("学籍番号")
                                        .WithValue(studentId)
                        )
                    );
                break;
            case "whois":
                var data = (SocketGuildUser)arg.Data.Options.First().Value;
                var counts = Context.MatsudairaDatas.Count(x => x.AccountId == data.Id);
                Console.WriteLine(counts);
                if (counts==0)
                {
                    await arg.RespondAsync("このユーザの登録はありません．", ephemeral: true);
                    return;
                }
                var sql = Context.MatsudairaDatas.Where(x => x.AccountId == data.Id).Single();

                var roles = "";
                foreach (var role1 in data.Roles)
                {
                    roles+=role1.Name +", ";
                }
                await arg.RespondAsync(
                    ephemeral: true,
                    embed: EmbedInstanceCreator(
                        "@" + data.Username + " の中の人",
                        "アカウントの中の人を松平定信公に照会しました．",
                        new EmbedFieldBuilder()
                                        .WithName("名前")
                                        .WithValue(sql.GivenName + "　" + sql.FirstName),
                                    new EmbedFieldBuilder()
                                        .WithName("Name")
                                        .WithValue(sql.RohmeGivenName + "　" + sql.RohmeFirstName),
                                    new EmbedFieldBuilder()
                                        .WithName("学籍番号")
                                        .WithValue(sql.Email.Replace("@" + EmailDomain, "")),
                                    new EmbedFieldBuilder()
                                        .WithName("ロール")
                                        .WithValue(roles[..^1])
                        )
                    );
                break;
            case "create":
                var roleName = "";
                var roleColor = "";
                foreach (var opts in arg.Data.Options)
                {
                    switch (opts.Name)
                    {
                        case "name":
                            roleName = (string)opts.Value;
                            break;
                        case "color":
                            roleColor = (string)opts.Value;
                            break;
                    }
                }
                if (Guild.Roles.Count(x => x.Name==roleName) > 0)
                {
                    await arg.RespondAsync(ephemeral: true,text:"このロールは既に存在します．このロールを松平定信に登録する場合はaddコマンドを使用してください．");
                    return;
                }
                
                var createdRole=await Guild.CreateRoleAsync(roleName, color: Color.Parse(roleColor));
                Context.MatsudairaROles.Add(new MatsudairaRoles() { RoleId = createdRole.Id });
                await Context.SaveChangesAsync();
                await arg.RespondAsync(ephemeral: true, embed: EmbedInstanceCreator("ロール"+roleName+"を作成しました",""));
                break;
            case "add":
                var addRoleName=(SocketRole)arg.Data.Options.First().Value;
                if (Guild.Roles.Count(x => x.Id==addRoleName.Id) == 0)
                {
                    await arg.RespondAsync(ephemeral: true,text:"このロールは存在しません．このロールを松平定信に登録する場合はcreateコマンドを使用してください．");
                    return;
                }
                var role=Guild.Roles.Single(x=>x.Id==addRoleName.Id);
                if (Context.MatsudairaROles.Count(x => x.RoleId == role.Id) > 0)
                {
                    await arg.RespondAsync(ephemeral: true,text:"このロールは既に松平定信に登録されています．");
                    return;
                }
                Context.MatsudairaROles.Add(new MatsudairaRoles() { RoleId = role.Id });
                await Context.SaveChangesAsync();
                await arg.RespondAsync(ephemeral: true, embed: EmbedInstanceCreator("ロール"+addRoleName.Name+"を登録しました",""));
                break;
            case "list":
                var roleList = "";
                foreach (var role1 in Context.MatsudairaROles)
                {
                    roleList += "<@"+role1.RoleId + ">, ";
                }
                if (string.IsNullOrWhiteSpace(roleList))
                {
                    await arg.RespondAsync(
                        ephemeral: true,
                        embed: EmbedInstanceCreator(
                            "管理しているロールはありません．",
                            ""
                        )
                    );
                    break;
                }
                await arg.RespondAsync(
                    ephemeral: true,
                    embed: EmbedInstanceCreator(
                        "次のロールを管理しています．",
                        "",
                        new EmbedFieldBuilder()
                                        .WithName("ロール")
                                        .WithValue(roleList[..^1])
                        )
                    );
                break;
            case "delete":
                if (arg.Data.Options.Count == 0)
                {
                    await arg.RespondAsync(ephemeral: true,text:"ロール・ロールIdを指定してください．");
                    return;
                }
                foreach (var opts in arg.Data.Options)
                {
                    switch (opts.Name)
                    {
                        case "role":
                            var delRoleName = (SocketRole)arg.Data.Options.First().Value;
                            if (Context.MatsudairaROles.Count(x => x.RoleId==delRoleName.Id) == 0)
                            {
                                await arg.RespondAsync(ephemeral: true,text:"このロールは松平定信に登録されていません．");
                                return;
                            }
                            Context.MatsudairaROles.Remove(Context.MatsudairaROles.Single(x => x.RoleId == delRoleName.Id));
                            await Context.SaveChangesAsync();
                            await arg.RespondAsync(ephemeral: true, embed: EmbedInstanceCreator("ロール"+delRoleName.Name+"の登録を解除しました．ロール自体を削除するには，Discordの設定から行って下さい．",""));
                            break;
                        case "roleint":
                            var delRoleInt = (ulong)arg.Data.Options.First().Value;
                            Context.MatsudairaROles.Remove(Context.MatsudairaROles.Single(x => x.RoleId == delRoleInt));
                            await Context.SaveChangesAsync();
                            await arg.RespondAsync(ephemeral: true, embed: EmbedInstanceCreator("ロール"+delRoleInt+"の登録を解除しました．",""));
                            break;
                    }
                }
                
                break;
            case "survey" :
                var i = 0;
                var str = "";
                foreach (var roleDt1 in Context.MatsudairaROles)
                {
                    emojis.Add(new SurveyItem(){RoleId = roleDt1.RoleId, Emoji = char.ConvertFromUtf32(0x1F600+i)});
                    str += emojis[i].ExplainText()+"\n";
                    i++;
                }
                if (string.IsNullOrWhiteSpace(str))
                {
                    await arg.RespondAsync(ephemeral: true,text:"ロールが松平定信に一つも登録されていません．");
                    return;
                }
                
               var message= await Guild.GetTextChannel(JoinChId).SendMessageAsync(
                    embed: EmbedInstanceCreator(
                        "松平定信公によるロール付与の令",
                        "自分が該当するロールにリアクションをつけてください．",
                        new EmbedFieldBuilder()
                            .WithName("ロール")
                            .WithValue(str[..^1])
                    ),
                    text: "@everyone"
                );
                surveyMessageId = message.Id;
                foreach (var emoji in emojis)
                {//ote.Parse(emoji.Emoji)
                    await message.AddReactionAsync(new Emoji(emoji.Emoji));
                }
                break;
        }
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
    private static Task ReactionRemoved(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, SocketReaction arg3)
    {
        if (arg3.UserId==Client.CurrentUser.Id|| arg3.MessageId!= surveyMessageId) return Task.CompletedTask;
        var emojiId= char.ConvertToUtf32(arg3.Emote.Name, 0)-0x1F600;
        try
        {
            Guild.GetUser(arg3.User.Value.Id).RemoveRoleAsync(Guild.GetRole(emojis[emojiId].RoleId)).Wait();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        return Task.CompletedTask;
    }

    private static Task ReactionAdded(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, SocketReaction arg3)
    {
        Console.Write("ReactionAdded: IsBot: ");
        Console.WriteLine(arg3.User.Value.IsBot);
        Console.Write("MessageId: ");
        Console.WriteLine(arg3.MessageId);
        Console.Write("Expected MessageId: ");
        Console.WriteLine(surveyMessageId);
        if (arg3.User.Value.IsBot|| arg3.MessageId != surveyMessageId) return Task.CompletedTask;
        Console.Write("EmojiID: ");
        var emojiId= char.ConvertToUtf32(arg3.Emote.Name, 0)-0x1F600;
        Console.WriteLine(emojiId);
        Console.Write("Role Id: ");
        Console.WriteLine(emojis[emojiId].RoleId);
        try
        {
            Guild.GetUser(arg3.User.Value.Id).AddRoleAsync(Guild.GetRole(emojis[emojiId].RoleId)).Wait();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        return Task.CompletedTask;
    }

    private static Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log.ToString());
        return Task.CompletedTask;
    }

    private static async Task ReadyAsync()
    {
        Console.WriteLine("logged in as "+Client.CurrentUser);
        Guild = Client.GetGuild(GuildId);
        var commandIam = new SlashCommandBuilder()
        #region IamCommand
            .WithName("iam")
            .WithDescription(
                "松平定信公にあなたの情報を登録します．(登録情報はこのDiscordサーバ上で公開されます．また，その情報は定信公がデプロイされているサーバ上に暗号化処理を施して安全に保存されます．)")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("givenname")
                .WithDescription("日本語での名字(例: 静岡)")
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.String))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("firstname")
                .WithDescription("日本語での名前(例: 太郎)")
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.String))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("rohmegivenname")
                .WithDescription("ローマ表記での名字(例: Shizuoka)")
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.String))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("rohmefirstname")
                .WithDescription("ローマ表記での名前(例: Taro)")
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.String))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("studentid")
                .WithDescription("学籍番号(例: D24127)")
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.String))
            .Build();
        #endregion
        
        var commandWhois = new SlashCommandBuilder()
        #region WhoisCommand
            .WithName("whois")
            .WithDescription(
                "松平定信公に指定したアカウントの中の人を照会します．")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("account")
                .WithDescription("中の人を知りたいアカウント")
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.User))
            .Build();
        #endregion
        
        var commandRoleCreate = new SlashCommandBuilder()
        #region RoleCreateCommand
            .WithName("create")
            .WithDescription(
                "松平定信公にロールを作成してもらいます．")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("name")
                .WithDescription("ロール名")
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.String))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("color")
                .WithDescription("ロールの色を指定します")
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.String))
            .Build();
        #endregion
        
        var commandRoleAdd = new SlashCommandBuilder()
        #region RoleAddCommand
            .WithName("add")
            .WithDescription(
                "ロールを松平定信公の管理下に置きます．")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("role")
                .WithDescription("松平定信公の管理下に置きたいロール")
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.Role))
            .Build();
        #endregion
        
        var commandRoleList = new SlashCommandBuilder()
        #region RoleListCommand
            .WithName("list")
            .WithDescription(
                "松平定信公の管理下に置かれているロールを表示します．")
            .Build();
        #endregion
        
        var commandRoleDelete = new SlashCommandBuilder()
        #region RoleDeleteCommand
            .WithName("delete")
            .WithDescription(
                "ロールを松平定信公の管理下からはずします．")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("role")
                .WithDescription("松平定信公の管理下より外したいロール")
                .WithRequired(false)
                .WithType(ApplicationCommandOptionType.Role))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("roleint")
                .WithDescription("松平定信公の管理下より外したいロールId(すでに削除されたロールに使用)")
                .WithRequired(false)
                .WithType(ApplicationCommandOptionType.Integer))
            .Build();
        #endregion
        
        var commandSurvey = new SlashCommandBuilder()
        #region SurveyCommand
            .WithName("survey")
            .WithDescription(
                "松平定信公によるロール付与令を発行します．")
            .Build();
        #endregion

        try
        {
            Console.WriteLine("Registering Iam Command");
            await Client.CreateGlobalApplicationCommandAsync(commandIam);
            Console.WriteLine("Registering Whois Command");
            await Guild.CreateApplicationCommandAsync(commandWhois);
            Console.WriteLine("Registering Create Command");
            await Guild.CreateApplicationCommandAsync(commandRoleCreate);
            Console.WriteLine("Registering Add Command");
            await Guild.CreateApplicationCommandAsync(commandRoleAdd);
            Console.WriteLine("Registering List Command");
            await Guild.CreateApplicationCommandAsync(commandRoleList);
            Console.WriteLine("Registering Delete Command");
            await Guild.CreateApplicationCommandAsync(commandRoleDelete);
            Console.WriteLine("Registering Survey Command"); 
            await Client.CreateGlobalApplicationCommandAsync(commandSurvey);
        }
        catch(HttpException exception)
        {
            var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
            Console.WriteLine(json);
        }
    }
}

class SurveyItem
{
    public ulong RoleId { get; set; }
    public string Emoji { get; set; }
    public string ExplainText() => Emoji + "：<@&" + RoleId + ">";
}