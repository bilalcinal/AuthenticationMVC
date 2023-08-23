using Microsoft.EntityFrameworkCore;

namespace MyProject.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext>options) : base(options)
    {
        
    }
    // protected override void OnModelCreating(ModelBuilder modelBuilder)
    // {
    //     base.OnModelCreating(modelBuilder);
    //     modelBuilder.Ignore<Sehirler>();     
    // }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Sehirler> Sehirlers{ get; set; }

}
 