using Microsoft.EntityFrameworkCore;

namespace IctPromotie.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Activiteit> Activiteiten => Set<Activiteit>();
    public DbSet<Persoon> Personen => Set<Persoon>();
    public DbSet<Materiaal> Materialen => Set<Materiaal>();
    public DbSet<NieuwsItem> Nieuws => Set<NieuwsItem>();
    public DbSet<Idee> Ideeen => Set<Idee>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // Enums als tekst opslaan: leesbaar in de database en veilig als de volgorde verandert.
        b.Entity<Activiteit>().Property(a => a.Soort).HasConversion<string>();
        b.Entity<Activiteit>().HasIndex(a => a.Start);
        b.Entity<NieuwsItem>().Property(n => n.Bereik).HasConversion<string>();
        b.Entity<NieuwsItem>().HasIndex(n => n.Url).IsUnique();
        b.Entity<Idee>().Property(i => i.Inspanning).HasConversion<string>();
        b.Entity<Idee>().Property(i => i.Status).HasConversion<string>();
    }
}
