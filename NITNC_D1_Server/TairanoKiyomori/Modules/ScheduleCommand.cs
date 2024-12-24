using Discord;
using Discord.Interactions;
using DiscordBotBasic;
using Microsoft.EntityFrameworkCore;
using NITNC_D1_Server.DataContext;

namespace TairanoKiyomori.Modules;

public class ScheduleCommand(IDbContextFactory<ApplicationDbContext> context, IConfiguration configuration) :
    Module(context, configuration)
{
    [SlashCommand("schedule", "平清盛公に授業変更を通達します．")]
    public async Task Schedule(
        [Summary("subjectname", "教科名(ex: 基礎数学I)")]
        string subjectname,
        [Summary("year", "変更後の実施日(年)(ex: 2024)\"")]
        int year,
        [Summary("month", "変更後の実施日(月)(ex: 11)")]
        int month,
        [Summary("day", "変更後の実施日(日)(ex: 25)")] int day,
        [Summary("start", "変更後の授業開始時数(ex: 1,2限ならばここでは1)")]
        int start,
        [Summary("end", "変更後の授業終了時数(ex: 1,2限ならばここでは2)")]
        int end
    )
    {
        var dts = new DateTime(year:year, month:month, day:day, kind:DateTimeKind.Utc,hour:0,minute:0,second:0,millisecond:0);
        await DbContext.KiyomoriSchedule.AddAsync(new KiyomoriSchedule() {SubjectName = subjectname, Date = dts, StartHour = start, EndHour = end});
        await DbContext.SaveChangesAsync();
        await RespondAsync(
            ephemeral: false,
            embed: Supports.EmbedInstanceCreator(
                subjectname + "の授業変更について",
                subjectname + "で授業変更がありました．皆さんお気をつけ下さい．",
                new EmbedFieldBuilder()
                    .WithName("教科名")
                    .WithValue(subjectname),
                new EmbedFieldBuilder()
                    .WithName("変更後の実施日時，時限")
                    .WithValue(dts.ToString("yyyy年MM月d日")+"　" + start + "限～" + end + "限")
                )
        );
    }
}