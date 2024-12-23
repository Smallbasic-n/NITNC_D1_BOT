using DiscordBotBasic;
using TairanoKiyomori;
using TairanoKiyomori.Modules;

Supports.Start(args, "平清盛","大政大臣", x =>
{
    x.AddHostedService<Worker>();
    x.AddSingleton<ScheduleCommand>();
});