using System.Net.Mime;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace NITNC_D1_Server.DataContext;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    
    
    public DbSet<ChankQuestions> ChankQuestions { get; set; }
    public DbSet<FactbookQuestions> FactbookQuestions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<ChankQuestions>()
            .HasKey(x => new { x.Id });
        builder.Entity<FactbookQuestions>()
            .HasKey(x => new { x.Id });
    }
}