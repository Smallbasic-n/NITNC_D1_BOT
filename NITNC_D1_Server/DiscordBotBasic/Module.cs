using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NITNC_D1_Server.DataContext;

namespace DiscordBotBasic;

public class Module(IDbContextFactory<ApplicationDbContext> context, IConfiguration configuration)
    : InteractionModuleBase<SocketInteractionContext>

{
    protected readonly ApplicationDbContext DbContext = context.CreateDbContext();
    
}