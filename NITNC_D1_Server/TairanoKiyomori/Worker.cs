using Discord.WebSocket;
using DiscordBotBasic;
using Microsoft.EntityFrameworkCore;
using NITNC_D1_Server.Data;

namespace TairanoKiyomori;

public class Worker(IDbContextFactory<ApplicationDbContext> DbContext,ILogger<DiscordWorkerService> logger, DiscordSocketClient client, IConfiguration configuration, InteractionHandler handler) :
    DiscordWorkerService(logger, client, configuration, handler)
{
    protected override async Task MainProcessAsync(CancellationToken stoppingToken)
    {
        var dbContext=DbContext.CreateDbContext();
        while (Guild==null){}
        var textch = Guild.GetTextChannel(Convert.ToUInt64(configuration["ScheduleChId"]));
        if (textch == null) throw new NullReferenceException();
        while (!stoppingToken.IsCancellationRequested)
        {
            var JST = DateTime.UtcNow.AddHours(9);

            if (JST.Hour == 6)
            {
                foreach (var schedule in dbContext.KiyomoriSchedule.Where(x => x.Date.Year == JST.Year&&x.Date.Month == JST.Month&&x.Date.Day == JST.Day&&!x.IsAlerted).ToList())
                {
                    await textch.SendMessageAsync(
                        "@everyone \n# 重要　本日の授業変更について \n本日" + schedule.Date.ToString("yyyy年MM月d日") + "の" +
                        schedule.StartHour + "限～" + schedule.EndHour + "限は**" + schedule.SubjectName +
                        "**が実施されます．お忘れ物をないさいませぬよう，お気をつけ下さい．\n "+Supports.ApplicationPrefix+" "+Supports.ApplicationName);
                    schedule.IsAlerted = true;
                    await dbContext.SaveChangesAsync();
                }
            }else if (JST.Hour == 20)
            {
                var compJST = new DateTime(JST.Year, JST.Month, JST.Day, 0, 0, 0,DateTimeKind.Utc);
                foreach (var schedule in dbContext.KiyomoriSchedule.Where(x => x.Date >= compJST.AddDays(1)).ToList())
                {
                    await textch.SendMessageAsync(
                        "@everyone \n# 重要　明日の授業変更について \n明日" + schedule.Date.ToString("yyyy年MM月d日") + "の" +
                        schedule.StartHour + "限～" + schedule.EndHour + "限は**" + schedule.SubjectName +
                        "**が実施されます．お忘れ物をないさいませぬよう，お気をつけ下さい．\n "+Supports.ApplicationPrefix+" "+Supports.ApplicationName);
                    dbContext.KiyomoriSchedule.Remove(schedule);
                    await dbContext.SaveChangesAsync();
                }
                
            }
        }
    }
}