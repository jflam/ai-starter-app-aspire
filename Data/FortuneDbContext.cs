using Microsoft.EntityFrameworkCore;

namespace Data;

public class FortuneDbContext : DbContext
{
    public FortuneDbContext(DbContextOptions<FortuneDbContext> options)
        : base(options)
    {
    }

    public DbSet<Fortune> Fortunes { get; set; }
}

public class Fortune
{
    public int Id { get; set; }
    public string Text { get; set; }
}
