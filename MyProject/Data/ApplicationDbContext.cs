using Microsoft.EntityFrameworkCore;

namespace MyProject.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext>options) : base(options)
    {
        
    }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Ilceler> Ilceler { get; set; }
    public DbSet<Sehirler> Sehirler{ get; set; }
    public DbSet<SemtMah> SemtMah { get; set; }
    public DbSet<Ulkeler> Ulkeler { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Ignore<Ilceler>();
        modelBuilder.Ignore<Sehirler>();
        modelBuilder.Ignore<SemtMah>();
        modelBuilder.Ignore<Ulkeler>();
        
    }

}
 