
using Microsoft.EntityFrameworkCore;

namespace MyProject.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext>options) : base(options)
    {
        
    }

}
 