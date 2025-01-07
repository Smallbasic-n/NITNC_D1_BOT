using Discord;
using Discord.Interactions;
using DiscordBotBasic;
using Microsoft.EntityFrameworkCore;
using NITNC_D1_Server.Data;
using NITNC_D1_Server.Data.Models;
using static DiscordBotBasic.Supports;

namespace MatsudairaSadanobu.Modules;

public class WhoisCommandModule(IDbContextFactory<ApplicationDbContext> context, IConfiguration configuration): Module(context, configuration)
{
    private readonly string _emailDomain = configuration["EmailDomain"]??"";

    [SlashCommand(
        "whois",
        "松平定信公に指定したアカウントの中の人を照会します．"
        )]
    public async Task WhoisSlashCommand(
        [Summary("account","中の人を知りたいアカウント")] IUser account
        )
    {
        var counts = DbContext.MatsudairaDatas.Count(x => x.AccountId == account.Id);
        Console.WriteLine(counts);
        if (counts==0)
        {
            await RespondAsync("このユーザの登録はありません．", ephemeral: true);
            return;
        }
        var sql = DbContext.MatsudairaDatas.Single(x => x.AccountId == account.Id);

        var roles = "";
        foreach (var role1 in Context.Guild.GetUser(account.Id).Roles)
        {
            roles+=role1.Name +", ";
        }
        await RespondAsync(
            ephemeral: true,
            embed: EmbedInstanceCreator(
                "@" + account.Username + " の中の人",
                "アカウントの中の人を松平定信公に照会しました．",
                new EmbedFieldBuilder()
                    .WithName("名前")
                    .WithValue(sql.GivenName + "　" + sql.FirstName),
                new EmbedFieldBuilder()
                    .WithName("Name")
                    .WithValue(sql.RohmeGivenName + "　" + sql.RohmeFirstName),
                new EmbedFieldBuilder()
                    .WithName("学籍番号")
                    .WithValue(sql.Email.Replace("@" + _emailDomain, "")),
                new EmbedFieldBuilder()
                    .WithName("ロール")
                    .WithValue(roles[..^1])
            )
        );
    }

    [SlashCommand(
        "iam",
        "松平定信公にあなたの情報を登録します．(登録情報はこのDiscordサーバ上で公開されます．また，その情報は定信公がデプロイされているサーバ上に暗号化処理を施して安全に保存されます．)"
        )]
    public async Task IamSlashCommand(
        [Discord.Interactions.Summary("givenname","日本語での名字(例: 静岡)")] string givenname,
        [Discord.Interactions.Summary("firstname","日本語での名前(例: 太郎)")] string firstname,
        [Discord.Interactions.Summary("rohmegivenname", "ローマ表記での名字(例: Shizuoka)")] string rohmegivenname,
        [Discord.Interactions.Summary("rohmefirstname", "ローマ表記での名前(例: Taro)")] string rohmefirstname,
        [Discord.Interactions.Summary("studentId", "学籍番号(例: D24199)")] int studentId
        )
    {

                var count = DbContext.MatsudairaDatas.Count(x => x.AccountId == Context.User.Id);
                // (x => x.AccountId == arg.User.Id);
                if (count==0)
                {
                    DbContext.MatsudairaDatas.Add(new MatsudairaDatas()
                    {
                        AccountId = Context.User.Id, Chank = 0, Email = studentId + "@"+_emailDomain, FactBook = 0,
                        FirstName = firstname, GivenName = givenname.ToUpper(), RohmeFirstName = rohmefirstname, RohmeGivenName = rohmegivenname
                    });
                }
                else
                {
                    var existsuser = DbContext.MatsudairaDatas.FirstOrDefault(x => x.AccountId == Context.User.Id);
                    existsuser.Email = studentId + "@"+_emailDomain;
                    existsuser.FirstName = firstname;
                    existsuser.GivenName = givenname;
                    existsuser.RohmeFirstName = rohmefirstname;
                    existsuser.RohmeGivenName = rohmegivenname.ToUpper();
                }
                await DbContext.SaveChangesAsync();

                await RespondAsync(
                    ephemeral: false,
                    text: "<@"+Context.User.Id+">",
                    embed: EmbedInstanceCreator(
                        "@" + Context.User.Username + " の中の人",
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
    }
}