using System.Net.Mime;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SoftFluent.EntityFrameworkCore.DataEncryption;
using SoftFluent.EntityFrameworkCore.DataEncryption.Providers;

namespace NITNC_D1_Server.DataContext;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    
    
    public DbSet<ChankQuestions> ChankQuestions { get; set; }
    public DbSet<FactbookQuestions> FactbookQuestions { get; set; }
    public DbSet<LincolnConfiguration> LincolnConfiguration { get; set; }
    public DbSet<MatsudairaDatas> MatsudairaDatas { get; set; }
    public DbSet<MatsudairaRoles> MatsudairaROles { get; set; }

    public static byte[] _encryptionKey; 
    public static byte[] _encryptionIV;
    private IEncryptionProvider _provider;
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        _provider = new AesProvider(_encryptionKey, _encryptionIV);
        base.OnModelCreating(builder);
        builder.Entity<ChankQuestions>()
            .HasKey(x => new { x.Id });
        builder.Entity<FactbookQuestions>()
            .HasKey(x => new { x.Id });
        builder.Entity<LincolnConfiguration>().HasNoKey();
        builder.Entity<MatsudairaDatas>()
            .HasKey(x=>new {x.Id});
        builder.Entity<MatsudairaRoles>()
            .HasKey(x=>new {x.Id});
        builder.UseEncryption(_provider);
    }
    
	
}