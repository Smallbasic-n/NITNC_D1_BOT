using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using DiscordBotBasic;
using Google.Protobuf.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using NITNC_D1_Server.DataContext;
using Npgsql;

namespace MatsudairaSadanobu;

public class Worker(ILogger<DiscordWorkerService> logger, DiscordSocketClient client, IConfiguration configuration, InteractionHandler handler)
    : DiscordWorkerService(logger, client, configuration, handler)
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        client.UserJoined += ClientOnUserJoined;
        client.ButtonExecuted += ClientOnButtonExecuted;
        await base.ExecuteAsync(stoppingToken);
    }

    private Task ClientOnButtonExecuted(SocketMessageComponent arg)
    {
        var customId = arg.Data.CustomId;
        if (string.IsNullOrWhiteSpace(customId) || customId[..5] != "role-") return Task.CompletedTask;
        var role = Convert.ToUInt64(customId[5..]);
        var user = client.GetGuild(GuildId).GetUser(arg.User.Id);
        var isDel = true;
        if (user.Roles.Any(x => x.Id == role)) user.RemoveRoleAsync(role);
        else
        {
            user.AddRoleAsync(role);
            isDel = false;
        }
        arg.RespondAsync(ephemeral: true, text: "あなたをロール" + client.GetGuild(GuildId).GetRole(role).Name + (isDel?"から除外":"に追加")+"しました．");
        return Task.CompletedTask;
    }

    private Task ClientOnUserJoined(SocketGuildUser arg)
    {
        if (arg.IsBot || arg.IsWebhook) return Task.CompletedTask;
        arg.Guild.GetTextChannel(Convert.ToUInt64(configuration["JoinChId"]))
            .SendMessageAsync("<@" + arg.Id + ">さん，こんにちは．D1サーバで，人事担当をしております，松平定信と申します．" +
                                    arg.Guild.Name +"サーバに参加を歓迎します．まずは，次のWebサイトを参考に，私にあなたの名前と学籍番号を登録して下さい． " +
                                    "https://github.com/Smallbasic-n/NITNC_D1_BOT?tab=readme-ov-file#%E6%9D%BE%E5%B9%B3%E5%AE%9A%E4%BF%A1 " +
                                    "みんなでよいサーバを作っていきましょう！");
        return Task.CompletedTask;
    }
}
