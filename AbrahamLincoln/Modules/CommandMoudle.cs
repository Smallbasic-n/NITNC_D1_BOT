using Discord;
using Discord.Interactions;
using DiscordBotBasic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NITNC_D1_Server.Data;
using static DiscordBotBasic.Supports;
namespace AbrahamLincoln.Modules;

public class CommandMoudle(IDbContextFactory<ApplicationDbContext> context, IConfiguration configuration) : Module(context, configuration)
{
    [SlashCommand("result", "チャンクで英単語，FACTBOOK これからの英文法 暗唱例文集のクイズでの全体成績を表示します．")]
    public async Task Result()
    {
        var chankChamp = DbContext.MatsudairaDatas.OrderByDescending(x => x.Chank).Take(5).ToArray();
        if (chankChamp.Length<=0||chankChamp[0].Chank <= 0)
        {
            await RespondAsync("チャンクで英単語での正答者がいません．", ephemeral: true);
            return;
        }
        
        var factbookChamp = DbContext.MatsudairaDatas.OrderByDescending(x => x.FactBook).Take(5).ToArray();;
        if (factbookChamp.Length<=0||factbookChamp[0].FactBook <= 0)
        {
            await RespondAsync("暗唱例文での正答者がいません．", ephemeral: true);
            return;
        }

        var config =await DbContext.LincolnConfiguration.SingleOrDefaultAsync();
        if (config == null)
        {
            await RespondAsync("リンカンの設定をしてください．．", ephemeral: true);
            return;
        }

        await RespondAsync(ephemeral: false,
            embed: EmbedInstanceCreator(
                "リンカン調べ！チャンクで英単語, 暗唱例文チャンプ！",
                "チャンク・暗唱例文クイズの結果を表示します．",
                new EmbedFieldBuilder()
                    .WithName("チャンクチャンプ/正答数")
                    .WithValue("<@"+chankChamp[0].AccountId+">/"+chankChamp[0].Chank),
                new EmbedFieldBuilder()
                    .WithName("FACTBOOKチャンプ/正答数")
                    .WithValue("<@"+factbookChamp[0].AccountId+"/"+factbookChamp[0].FactBook),
                new EmbedFieldBuilder()
                    .WithName("チャンク四天王")
                    .WithValue("<@"+chankChamp[1].AccountId+">/<@"+"<@"+chankChamp[2].AccountId+">/<@"+
                               "<@"+chankChamp[3].AccountId+">/<@"+"<@"+chankChamp[4].AccountId+">"),
                new EmbedFieldBuilder()
                    .WithName("FACTBOOK四天王")
                    .WithValue("<@"+factbookChamp[1].AccountId+">/<@"+"<@"+factbookChamp[2].AccountId+">/<@"+
                               "<@"+factbookChamp[3].AccountId+">/<@"+"<@"+factbookChamp[4].AccountId+">"),
                new EmbedFieldBuilder()
                    .WithName("チャンクの延べ正答数／延べ回答数／延べ出題数")
                    .WithValue(config.ChankSolved+"／"+config.ChankAnswered+"／"+config.ChankIssued),
                new EmbedFieldBuilder()
                    .WithName("チャンクの正答率(回答数を母数とする)／回答率")
                    .WithValue(Convert.ToDecimal(config.ChankSolved) / Convert.ToDecimal(config.ChankAnswered)*100+"％／"+
                               Convert.ToDecimal(config.ChankAnswered) / Convert.ToDecimal(config.ChankIssued)*100+"％"),
                new EmbedFieldBuilder()
                    .WithName("FACTBOOKの延べ正答数／延べ回答数／延べ出題数")
                    .WithValue(config.ChankSolved+"／"+config.ChankAnswered+"／"+config.FactbookIssued),
                new EmbedFieldBuilder()
                    .WithName("FACTBOOKの正答率(回答数を母数とする)／回答率")
                    .WithValue(Convert.ToDecimal(config.FactbookSolved) / Convert.ToDecimal(config.FactbookAnswered)*100+"％／"+
                               Convert.ToDecimal(config.FactbookAnswered) / Convert.ToDecimal(config.FactbookIssued)*100+"％")
            )
        );
    }

    [SlashCommand("myresult", "チャンクで英単語，FACTBOOK これからの英文法 暗唱例文集のクイズでのあなたの成績を表示します．")]
    public async Task MyResult()
    {
        var userData = await DbContext.MatsudairaDatas.SingleOrDefaultAsync(x=>x.AccountId==Context.User.Id);
        if (userData == null)
        {
            await RespondAsync(ephemeral: true,text:ApplicationName+"のサービスをご利用になる場合は，まず松平定信botにご自身の名前と学籍番号をご登録下さい．\n" +
                                                    "マニュアルは次のとおりです． https://github.com/Smallbasic-n/NITNC_D1_BOT?tab=readme-ov-file#%E6%9D%BE%E5%B9%B3%E5%AE%9A%E4%BF%A1" +
                                                    "\nご協力よろしくお願いいたします．\n"+ ApplicationPrefix+"　"+ApplicationName);
        }
        await RespondAsync(ephemeral: true,
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
        );
    }

    [SlashCommand("range", "チャンクで英単語，FACTBOOK これからの英文法 暗唱例文集のクイズ出題範囲を表示・設定します．")]
    public async Task Range(
        [Summary("chank","チャンクで英単語の出題STEP番号を指定します．")] int chank=-1,
        [Summary("start","FACTBOOK これからの英文法 暗唱例文集の出題開始番号を指定します．")] int start=-1,
        [Summary("end","FACTBOOK これからの英文法 暗唱例文集の出題終了番号を指定します．")] int end=-1)
    {
        var data = DbContext.LincolnConfiguration.Single();
        if (start > 0 && end > 0)
        {
            data.RangeStart = start;
            data.RangeEnds=end;
        }
        if (chank>0) data.RangeStep = chank;
        await DbContext.SaveChangesAsync();
        await RespondAsync(ephemeral: true,
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
        );
    }
}