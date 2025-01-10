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
        
        while (!stoppingToken.IsCancellationRequested)
        {
            SocketTextChannel textch;
            try
            {
                textch = Guild.GetTextChannel(Convert.ToUInt64(configuration["ScheduleChId"]));
            }
            catch (Exception e)
            {
                continue;
            }
            var JST = DateTime.UtcNow.AddHours(9);
            //Console.WriteLine(JST.ToString("yyyy/MM/dd HH:mm:ss")+"Hour="+JST.Hour);
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
                var assignments = "";
                foreach (var schedule in 
                         dbContext.KiyomoriAssignment.Where(x => x.Deadline.Year == JST.Year&&x.Deadline.Month == JST.Month&&x.Deadline.Day == JST.Day)
                             .Include(a=>a.KiyomoriWorking)
                             .ThenInclude(a=>a.KiyomoriSubject)
                             .OrderBy(a=>a.Deadline)
                         )
                {
                    assignments += "## 科目名："+schedule.KiyomoriWorking.KiyomoriSubject.SubjectName+"\n"+
                                  "### ワーク名："+schedule.KiyomoriWorking.WorkName+"\n"+
                                  "### 詳細："+schedule.Detail+ "\n";
                }

                if (assignments != "")
                {
                    await textch.SendMessageAsync(
                        "@everyone \n# 重要　本日の課題について \n本日" + JST.ToString("yyyy年MM月d日") + "は次の課題の締切日です．お忘れなきようにお願いします．\n"+assignments +"\n "+Supports.ApplicationPrefix+" "+Supports.ApplicationName);
                }
                Thread.Sleep(70*60*1000);
            }
            else if (JST.Hour == 20)
            {
                var compJST = new DateTime(JST.Year, JST.Month, JST.Day, 0, 0, 0,DateTimeKind.Utc);
                if (JST.DayOfWeek == DayOfWeek.Friday)
                {
                    
                    var weekAssignments = "";
                    foreach (var assignment in dbContext.KiyomoriAssignment.Where(a=>
                                     a.Deadline>=compJST&&
                                     a.Deadline<=compJST.AddDays(7)
                                  )
                                 .Include(a=>a.KiyomoriWorking)
                                 .ThenInclude(a=>a.KiyomoriSubject)
                                 .OrderBy(a=>a.Deadline)
                             )
                    {
                        weekAssignments += "## "+assignment.KiyomoriWorking.KiyomoriSubject.SubjectName+
                                           ", "+assignment.KiyomoriWorking.WorkName+
                                           ", "+assignment.Deadline.ToString("yyyy/MM/dd")+
                                           "まで\n"+ assignment.Detail+"\n";
                    }
                    if (weekAssignments != "")
                    {
                        await textch.SendMessageAsync(
                            "@everyone \n# 連絡　今週締め切りの課題について \n今週は次の課題の締切日です．お忘れなきようにお願いします．\n"+weekAssignments +"\n "+Supports.ApplicationPrefix+" "+Supports.ApplicationName);
                    }
                    else
                    {
                        
                        await textch.SendMessageAsync("今日より1週間以内提出の課題はありません. \n" + Supports.ApplicationPrefix + "　" +
                                                      Supports.ApplicationName);
                    }
                }
                else
                {
                    var assignments = "";
                    foreach (var schedule in 
                             dbContext.KiyomoriAssignment.Where(x => x.Deadline >= compJST.AddDays(1)&&x.Deadline<compJST.AddDays(2))
                                 .Include(a=>a.KiyomoriWorking)
                                 .ThenInclude(a=>a.KiyomoriSubject)
                                 .ToList()
                            )
                    {
                        assignments += "科目名："+schedule.KiyomoriWorking.KiyomoriSubject.SubjectName+"\n"+
                                      "ワーク名："+schedule.KiyomoriWorking.WorkName+"\n"+
                                      "詳細："+schedule.Detail+ "\n";
                    }

                    if (assignments != "")
                    {
                        await textch.SendMessageAsync(
                            "@everyone \n# 重要　明日の課題について \n明日" + JST.AddDays(1).ToString("yyyy年MM月d日") + "は次の課題の締切日です．お忘れなきようにお願いします．\n"+assignments +"\n "+Supports.ApplicationPrefix+" "+Supports.ApplicationName);
                    }
                }
                foreach (var schedule in dbContext.KiyomoriSchedule.Where(x => x.Date >= compJST.AddDays(1)&&x.Date < compJST.AddDays(2)).ToList())
                {
                    await textch.SendMessageAsync(
                        "@everyone \n# 重要　明日の授業変更について \n明日" + schedule.Date.ToString("yyyy年MM月d日") + "の" +
                        schedule.StartHour + "限～" + schedule.EndHour + "限は**" + schedule.SubjectName +
                        "**が実施されます．お忘れ物をないさいませぬよう，お気をつけ下さい．\n "+Supports.ApplicationPrefix+" "+Supports.ApplicationName);
                    dbContext.KiyomoriSchedule.Remove(schedule);
                    await dbContext.SaveChangesAsync();
                }
                Thread.Sleep(70*60*1000);
                
            }
            
            //Thread.Sleep(60*1000);
        }
    }
}