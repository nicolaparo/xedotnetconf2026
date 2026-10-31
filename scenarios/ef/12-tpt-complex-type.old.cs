#:package Microsoft.EntityFrameworkCore.SqlServer
#:property PublishAot=false

// BEFORE: complex types worked, but not together with TPT/TPC. The workaround was TPH (the default).
// Prints the CREATE script (no database connection needed - verify).

using System.Reflection;
using Microsoft.EntityFrameworkCore;

Console.WriteLine($"EF Core {typeof(DbContext).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion}");

using var db = new ZooContext();
Console.WriteLine(db.Database.GenerateCreateScript());

class ZooContext : DbContext
{
    public DbSet<Animal> Animals => Set<Animal>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlServer("Server=.;Database=Demo;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // TPH is the default: one table, a discriminator column.
        modelBuilder.Entity<Animal>().ComplexProperty(a => a.Details);
        modelBuilder.Entity<Dog>();
        modelBuilder.Entity<Cat>();
    }
}

abstract class Animal
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public required AnimalDetails Details { get; set; }
}

class Dog : Animal { public string Breed { get; set; } = ""; }
class Cat : Animal { public bool IsIndoor { get; set; } }

class AnimalDetails
{
    public DateTime BirthDate { get; set; }
    public string? Veterinarian { get; set; }
}
