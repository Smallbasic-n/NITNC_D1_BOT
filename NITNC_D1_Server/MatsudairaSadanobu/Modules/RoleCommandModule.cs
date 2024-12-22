using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using NITNC_D1_Server.DataContext;
using static DiscordBotBasic.Supports;

namespace MatsudairaSadanobu.Modules;

public class RoleCommandModule : InteractionModuleBase<SocketInteractionContext>

{
    private readonly ApplicationDbContext DbContext;
    private ulong JoinChId;
    
    public InteractionService Commands { get; set; }

    private DiscordBotBasic.InteractionHandler _handler;

    public RoleCommandModule(DiscordBotBasic.InteractionHandler handler, IDbContextFactory<ApplicationDbContext> context,
        IConfiguration configuration)
    {
        _handler = handler;
        DbContext = context.CreateDbContext();
        JoinChId=Convert.ToUInt64(configuration["JoinChId"]);
    }


    [SlashCommand(
        "rolecreate",
        "松平定信公にロールを作成してもらいます．"
    )]
    [RequireUserPermission(GuildPermission.Administrator| GuildPermission.ManageRoles)]
    public async Task CreateRoleSlashCommand(
        [Summary("name","ロール名")] string name,
        [Summary("color","ロールの色を指定します．")] string color
    )
    {
        if (Context.Guild.Roles.Count(x => x.Name==name) > 0)
        {
            await RespondAsync(ephemeral: true,text:"このロールは既に存在します．このロールを松平定信に登録する場合はaddコマンドを使用してください．");
            return;
        }
                
        var createdRole=await Context.Guild.CreateRoleAsync(name, color: Color.Parse(color));
        DbContext.MatsudairaROles.Add(new MatsudairaRoles() { RoleId = createdRole.Id });
        await DbContext.SaveChangesAsync();
        await RespondAsync(ephemeral: true, embed: EmbedInstanceCreator("ロール"+name+"を作成しました",""));
    }
    
    [SlashCommand(
        "roleadd",
        "ロールを松平定信公の管理下に置きます．"
    )]
    [RequireUserPermission(GuildPermission.Administrator| GuildPermission.ManageRoles)]
    public async Task AddRoleSlashCommand(
        [Summary("role","松平定信公の管理下に置きたいロール")] IRole role
    )
    {
        if (Context.Guild.Roles.All(x => x.Id != role.Id))
        {
            await RespondAsync(ephemeral: true,text:"このロールは存在しません．このロールを松平定信に登録する場合はcreateコマンドを使用してください．");
            return;
        }
        var dbrole=Context.Guild.Roles.Single(x=>x.Id==role.Id);
        if (DbContext.MatsudairaROles.Any(x => x.RoleId == dbrole.Id))
        {
            await RespondAsync(ephemeral: true,text:"このロールは既に松平定信に登録されています．");
            return;
        }
        DbContext.MatsudairaROles.Add(new MatsudairaRoles() { RoleId = dbrole.Id });
        await DbContext.SaveChangesAsync();
        await RespondAsync(ephemeral: true, embed: EmbedInstanceCreator("ロール"+role.Name+"を登録しました",""));
    }
    
    [SlashCommand(
        "rolelist",
        "松平定信公の管理下に置かれているロールを表示します．"
    )]
    [RequireUserPermission(GuildPermission.Administrator| GuildPermission.ManageRoles)]
    public async Task ListRoleSlashCommand(
    )
    {
        var roleList = "";
        foreach (var role1 in DbContext.MatsudairaROles)
        {
            roleList += "@"+Context.Guild.GetRole(role1.RoleId).Name + ", ";
        }
        if (string.IsNullOrWhiteSpace(roleList))
        {
            await RespondAsync(
                ephemeral: true,
                embed: EmbedInstanceCreator(
                    "管理しているロールはありません．",
                    ""
                )
            );
            return;
        }
        await RespondAsync(
            ephemeral: true,
            embed: EmbedInstanceCreator(
                "次のロールを管理しています．",
                "",
                new EmbedFieldBuilder()
                    .WithName("ロール")
                    .WithValue(roleList[..^1])
            )
        );
    }
    
    [SlashCommand(
        "roledelete",
        "ロールを松平定信公の管理下からはずします．"
    )]
    [RequireUserPermission(GuildPermission.Administrator| GuildPermission.ManageRoles)]
    public async Task DeleteRoleSlashCommand(
        [Summary("role","松平定信公の管理下より外したいロール")] IRole role=null,
        [Summary("roleid","松平定信公の管理下より外したいロールId．(すでに削除されたロールに使用)")] ulong roleId=0,
        [Summary("purge","ロールをDiscord上からも削除するかどうか(デフォルトでFalse，パラメータrole指定時のみ有効)．")] bool purge=false
    )
    {
        if (!Equals(role, null))
        {
            if (DbContext.MatsudairaROles.All(x => x.RoleId != role.Id))
            {
                await RespondAsync(ephemeral: true, text: "このロールは松平定信に登録されていません．");
                return;
            }

            DbContext.MatsudairaROles.Remove(DbContext.MatsudairaROles.Single(x => x.RoleId == role.Id));
            if (purge) await Context.Guild.GetRole(role.Id).DeleteAsync(new RequestOptions{ AuditLogReason = "From Matsudaira Sadanobu", RetryMode = RetryMode.AlwaysRetry});
            await DbContext.SaveChangesAsync();
            await RespondAsync(
                ephemeral: true,
                embed: EmbedInstanceCreator(
                    "ロール" + role.Name + "の登録の解除" + (purge ? "とロールのDiscord上からの削除" : "") + "をしました．",
                    "")
            );
            return;
        }
        var roleData = Context.Guild.GetRole(roleId);
        if (!Equals(roleData, null))
        {
            DbContext.MatsudairaROles.Remove(DbContext.MatsudairaROles.Single(x => x.RoleId == roleId));
            await DbContext.SaveChangesAsync();
            await RespondAsync(ephemeral: true, embed: EmbedInstanceCreator("ロール" + roleId + "の登録を解除しました．", ""));
            return;
        }
        await RespondAsync(ephemeral: true,text:"ロールもしくは正しいロールIdを指定してください．");
    }
    
    [SlashCommand(
        "survey",
        "松平定信公によるロール付与令を発行します．"
    )]
    [RequireUserPermission(GuildPermission.Administrator| GuildPermission.ManageRoles)]
    public async Task SurveyRoleSlashCommand(
    )
    {
        var buttons = new ComponentBuilder();
        foreach (var roleDt1 in DbContext.MatsudairaROles)
        {
            buttons.WithButton(Context.Guild.GetRole(roleDt1.RoleId).Name,"role-"+roleDt1.RoleId.ToString());
        }
        await RespondAsync(ephemeral:true, text:"ロール付与令を発行します．");
        var message= await Context.Guild.GetTextChannel(JoinChId).SendMessageAsync(
            embed: EmbedInstanceCreator(
                "松平定信公によるロール付与の令",
                "自分が該当するロールにリアクションをつけてください．"
            ),
            text: "@everyone",
            components: buttons.Build()
        );
    }
}