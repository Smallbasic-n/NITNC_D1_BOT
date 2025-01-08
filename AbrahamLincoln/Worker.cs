using System.ComponentModel;
using Discord;
using Discord.WebSocket;
using DiscordBotBasic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NITNC_D1_Server.Data;
using NITNC_D1_Server.Data.Models;

namespace AbrahamLincoln;

public class Worker(ILogger<DiscordWorkerService> logger, DiscordSocketClient client, IConfiguration configuration, InteractionHandler handler,IDbContextFactory<ApplicationDbContext> context)
    :DiscordWorkerService(logger, client, configuration, handler)
{
    
    private ApplicationDbContext _dbContext=context.CreateDbContext();
    
    private readonly ulong ChankChannel= Convert.ToUInt64(Environment.GetEnvironmentVariable("ChankChId"));
    private readonly ulong ChankRangeChannel=Convert.ToUInt64(Environment.GetEnvironmentVariable("ChankRangeChId"));
    private readonly ulong FactbookChannel=Convert.ToUInt64(Environment.GetEnvironmentVariable("FactbookChId"));
    private readonly ulong FactbookRangeChannel=Convert.ToUInt64(Environment.GetEnvironmentVariable("FactbookRangeChId"));

    private string ChankAnswer { get; set; } = "";
    private string ChankRangeAnswer{ get; set; } = "";
    private string FactbookAnswer { get; set; } = "";
    private string FactbookRangeAnswer { get; set; } = "";
    protected override async Task MainProcessAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Waiting for configure lincoln...");
        if (!await _dbContext.LincolnConfiguration.AnyAsync(stoppingToken))
        {
            await _dbContext.LincolnConfiguration.AddAsync(new LincolnConfiguration()
            {
                ChankAnswered = 0, ChankSolved = 0, FactbookAnswered = 0, FactbookSolved = 0, RangeEnds = 0,
                RangeStart = 0, RangeStep = 0
            }, stoppingToken);
            await _dbContext.SaveChangesAsync(stoppingToken);
        }
        logger.LogInformation("Detected lincoln configuration");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            var data = _dbContext.LincolnConfiguration.AsNoTracking().Single();
            var chankStep = data.RangeStep;
            var factbookStart = data.RangeStart;
            var factbookEnd = data.RangeEnds;
            Console.WriteLine($"chank:{chankStep},start:{factbookStart},end:{factbookEnd}");
            if (_dbContext.ChankQuestions.Count() != 0)
            {
                await ChankAllRange(chankStep);
                await ChankStepRange(chankStep);
            }

            if (_dbContext.FactbookQuestions.Count() != 0)
            {
                await FactbookAllRange(factbookEnd);
                await FactbookStaStpRange(factbookStart, factbookEnd);
            }

            Thread.Sleep(Convert.ToInt32(configuration["Interval"]) * 1000);
        }
    }

    protected override async Task ReadyAsync()
    {
        client.MessageReceived+= ClientOnMessageReceived;
        await base.ReadyAsync();
    }

    private async Task ClientOnMessageReceived(SocketMessage message)
    {
        if (message.Author.Id == client.CurrentUser.Id) return;
        var channelId = message.Channel.Id;
        var isChankA = channelId == ChankChannel;
        var isChankB = channelId == ChankRangeChannel;
        var isFactA= channelId == FactbookChannel;
        var isFactB= channelId == FactbookRangeChannel;
        var answerData=isChankA ? ChankAnswer : isChankB ? ChankRangeAnswer : isFactA ? FactbookAnswer : FactbookRangeAnswer;
        if (!isChankA && !isChankB && !isFactA && !isFactB)  return;
        if ((isChankA && ChankAnswer == "")||(isChankB && ChankRangeAnswer == "")||(isFactA && FactbookAnswer == "")||(isFactB && FactbookRangeAnswer == "")) return;
        var config = await _dbContext.LincolnConfiguration.SingleAsync();
        if (isChankA || isChankB) config.ChankAnswered++;
        else config.FactbookAnswered++;
        var emoji = "\ud83e\udd14";
        
        if (message.Content.Replace(" ", "").Replace("　", "").Replace(",", "").Replace("、", "").Replace(".","").ToUpper() ==
            answerData.Replace(" ", "").Replace("　", "").Replace(",", "").Replace("、", "").Replace(".","").ToUpper())
        {
            if (isChankA || isChankB) config.ChankSolved++;
            else config.FactbookSolved++;
            emoji = "\ud83d\udcaf";
            await ScoreReg(isChankA||isChankB, message.Author.Id);
            if (isChankA) ChankAnswer = "";
            else if (isChankB) ChankRangeAnswer = "";
            else if (isFactA) FactbookAnswer = "";
            else if (isFactB) FactbookRangeAnswer = "";
        }
        await _dbContext.SaveChangesAsync();
        await message.AddReactionAsync(new Emoji(emoji));
    }

    private async Task ChankAllRange(int chankStep)
    {
        try
        {
            var data=await _dbContext.ChankQuestions.Where(x => x.Step <= chankStep).OrderBy(x => Guid.NewGuid()).Take(1).SingleOrDefaultAsync();
            if (data == null) return;
            ChankAnswer=data.Answer;
            await Guild.GetTextChannel(ChankChannel).SendMessageAsync($"日本語：{data.Japanese}\n英語：{data.English}");
            var config = await _dbContext.LincolnConfiguration.SingleAsync();
            config.ChankIssued++;
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    private async Task ChankStepRange(int chankStep)
    {
        try
        {
            var data=await _dbContext.ChankQuestions.Where(x => x.Step == chankStep).OrderBy(x => Guid.NewGuid()).Take(1).SingleOrDefaultAsync();
            if (data == null) return;
            ChankRangeAnswer=data.Answer;
            await Guild.GetTextChannel(ChankRangeChannel).SendMessageAsync($"日本語：{data.Japanese}\n英語：{data.English}");
            var config = await _dbContext.LincolnConfiguration.SingleAsync();
            config.ChankIssued++;
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    private async Task FactbookAllRange(int factbookEnd)
    {
        try
        {
            var data=await _dbContext.FactbookQuestions.Where(x => x.Id<= factbookEnd )
                .OrderBy(x => Guid.NewGuid()).Take(1).SingleOrDefaultAsync();
            if (data == null) return;
            FactbookAnswer=data.Answer;
            await Guild.GetTextChannel(FactbookChannel).SendMessageAsync($"日本語：{data.Japanese}");
            var config = await _dbContext.LincolnConfiguration.SingleAsync();
            config.FactbookIssued++;
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    private async Task FactbookStaStpRange(int factbookStart,int factbookEnd)
    {
        try
        {
            var data=await _dbContext.FactbookQuestions.Where(x => x.Id >= factbookStart && x.Id<= factbookEnd )
                .OrderBy(x => Guid.NewGuid()).Take(1).SingleOrDefaultAsync();
            if (data == null) return;
            FactbookRangeAnswer=data.Answer;
            await Guild.GetTextChannel(FactbookRangeChannel).SendMessageAsync($"日本語：{data.Japanese}");
            var config = await _dbContext.LincolnConfiguration.SingleAsync();
            config.FactbookIssued++;
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    private async Task ScoreReg(bool isChank,ulong accountId)
    {
        var user=await _dbContext.MatsudairaDatas.SingleOrDefaultAsync(x => x.AccountId == accountId);
        if (user == null)
        {
            var discordUser = Guild.Users.Single(x => x.Id == accountId);
            await discordUser.CreateDMChannelAsync();
            await discordUser.SendMessageAsync(Supports.ApplicationName+"のサービスをご利用になる場合は，まず松平定信botにご自身の名前と学籍番号をご登録下さい．\n" +
                                                                       "マニュアルは次のとおりです． https://github.com/Smallbasic-n/NITNC_D1_BOT?tab=readme-ov-file#%E6%9D%BE%E5%B9%B3%E5%AE%9A%E4%BF%A1" +
                                                                       "\nご協力よろしくお願いいたします．\n"+Supports.ApplicationPrefix+"　"+Supports.ApplicationName);
            return;
        }
        if (isChank) user.Chank += 1;
        else user.FactBook += 1;
        await _dbContext.SaveChangesAsync();
    }
}