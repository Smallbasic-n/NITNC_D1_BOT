using MatsudairaSadanobu.Modules;
using static DiscordBotBasic.Supports;

namespace MatsudairaSadanobu;

public class Program
{
    public static void Main(string[] args)
    {
        Start(args, "松平定信","老中", 
            x => {
                x.AddHostedService<Worker>();
                x.AddSingleton<WhoisCommandModule>();
                x.AddSingleton<RoleCommandModule>();
                x.AddSingleton<TalkCommandModule>();
            }
        );
    }
    public static List<(ulong,int)> Speakers = new List<(ulong, int)>();
}