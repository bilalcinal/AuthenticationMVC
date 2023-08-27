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
    //     modelBuilder.Ignore<City>();     
    // }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<City> Cities{ get; set; }
    public DbSet<RegisterToken> RegisterTokens{ get; set; }


}
 