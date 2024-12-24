using Microsoft.Extensions.DependencyInjection;
using static DiscordBotBasic.Supports;

namespace AbrahamLincoln;

class Program
{
    static void Main(string[] args)
    {
        Start(args, "エイブラハム・リンカン","大統領", x =>
        {
            x.AddHostedService<Worker>();
        });
    }
}