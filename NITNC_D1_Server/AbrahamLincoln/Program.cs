using System.ComponentModel;
using System.Data;
using Discord;
using Discord.WebSocket;
using Npgsql;
using NpgsqlTypes;

namespace AbrahamLincoln;

class Program
{
    private static readonly DiscordSocketClient Client = new (new DiscordSocketConfig()
    {
        GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers | GatewayIntents.MessageContent
    });
    
    private static SocketGuild Guild;

    private static readonly NpgsqlConnection Context = new (Environment.GetEnvironmentVariable("ConnectionStrings__d1system"));
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

    private static async Task<Task> ReadyAsync()
    {
        Console.WriteLine("logged in as "+Client.CurrentUser);
        Guild = Client.GetGuild(GuildId);
        await Context.OpenAsync();
        var background = new BackgroundWorker();
        background.DoWork += async (sender, args) =>
        {
            while (true)
            {
                var confsql = Context.CreateCommand();
                confsql.CommandText =
                    "SELECT \"RangeStep\", \"RangeStart\", \"RangeEnds\" FROM \"LincolnConfiguration\";";
                using (var chk = await confsql.ExecuteReaderAsync())
                {
                    while (chk.Read())
                    {
                        ChankStep = Convert.ToInt32(chk["RangeStep"]);
                        FactbookStart = Convert.ToInt32(chk["RangeStart"]);
                        FactbookEnd = Convert.ToInt32(chk["RangeEnds"]);
                    }

                    await chk.CloseAsync();
                }

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
        var chanksql = Context.CreateCommand();
        chanksql.CommandText = "SELECT \"Japanese\", \"English\", \"Answer\" FROM \"ChankQuestions\" WHERE \"Step\" <= "+ChankStep +" AND \"Id\"=(SELECT (max(\"Id\") * random())::int FROM \"ChankQuestions\");";
        using (var chk=await chanksql.ExecuteReaderAsync())
        {
            while (chk.Read())
            {
                ChankAnswer=chk.GetString(2);
                await Guild.GetTextChannel(ChankChannel).SendMessageAsync($"日本語：{chk.GetString(0)}\n英語：{chk.GetString(1)}");
            }
            await chk.CloseAsync();
        }
    }
    
    private static async Task ChankStepRange()
    {
        var chanksql = Context.CreateCommand();
        chanksql.CommandText = "SELECT \"Japanese\", \"English\", \"Answer\" FROM \"ChankQuestions\" WHERE \"Step\" = "+ChankStep +" AND \"Id\"=(SELECT (max(\"Id\") * random())::int FROM \"ChankQuestions\");";
        using (var chk=await chanksql.ExecuteReaderAsync())
        {
            while (chk.Read())
            {
                ChankRangeAnswer=chk.GetString(2);
                await Guild.GetTextChannel(ChankRangeChannel).SendMessageAsync($"日本語：{chk.GetString(0)}\n英語：{chk.GetString(1)}");
            }
            await chk.CloseAsync();
        }
    }
    
    private static async Task FactbookAllRange()
    {
        var chanksql = Context.CreateCommand();
        chanksql.CommandText = "SELECT \"Japanese\", \"Answer\" FROM \"FactbookQuestions\" WHERE \"Id\" <= "+FactbookEnd +" AND \"Id\"=(SELECT (max(\"Id\") * random())::int FROM \"FactbookQuestions\");";
        using (var chk=await chanksql.ExecuteReaderAsync())
        {
            while (chk.Read())
            {
                FactbookAnswer=chk.GetString(1);
                await Guild.GetTextChannel(FactbookChannel).SendMessageAsync($"日本語：{chk.GetString(0)}");
            }
            await chk.CloseAsync();
        }
    }
    
    private static async Task FactbookStaStpRange()
    {
        var chanksql = Context.CreateCommand();
        chanksql.CommandText = "SELECT \"Japanese\", \"Answer\" FROM \"FactbookQuestions\" WHERE \"Id\" >= "+FactbookStart+" AND \"Id\" <= "+FactbookEnd +" AND \"Id\"=(SELECT (max(\"Id\") * random())::int FROM \"FactbookQuestions\");";
        using (var chk=await chanksql.ExecuteReaderAsync())
        {
            while (chk.Read())
            {
                FactbookRangeAnswer=chk.GetString(1);
                await Guild.GetTextChannel(FactbookRangeChannel).SendMessageAsync($"日本語：{chk.GetString(0)}");
            }
            await chk.CloseAsync();
        }
    }
    
    private static async Task ScoreReg(string whichAdd,string accountId)
    {
        var updatesql = Context.CreateCommand();
        updatesql.CommandText = "UPDATE \"MatsudairaDatas\" SET \""+whichAdd+"\"=\""+whichAdd+"\"+1 WHERE \"AccountId\" = "+accountId+";";
        using (var chk=await updatesql.ExecuteReaderAsync())
        {
            while (chk.Read())
            {
                FactbookRangeAnswer=chk.GetString(1);
                await Guild.GetTextChannel(FactbookRangeChannel).SendMessageAsync($"日本語：{chk.GetString(0)}");
            }
            await chk.CloseAsync();
        }
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
            await ScoreReg(isChankA||isChankB ? "Chank" : "FactBook", message.Author.Id.ToString());
        }
        await message.AddReactionAsync(new Emoji(emoji));
    }
}