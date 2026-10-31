#:package Microsoft.EntityFrameworkCore.SqlServer
#:property PublishAot=false

// AFTER: complex type + JSON column on a TPT hierarchy.
// Compiles on both SDKs. EF Core 10 is expected to throw while building the model (verify the message),
// EF Core 11 prints the CREATE script.
// Prints the CREATE script (no database connection needed - verify).

using System.Reflection;
using Microsoft.EntityFrameworkCore;

Console.WriteLine($"EF Core {typeof(DbContext).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion}");

using var db = new ZooContext();

try
{
    Console.WriteLine(db.Database.GenerateCreateScript());
}
catch (Exception ex)
{
    Console.WriteLine($"{ex.GetType().Name}: {ex.Message}");
}

class ZooContext : DbContext
{
    public DbSet<Animal> Animals => Set<Animal>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlServer("Server=.;Database=Demo;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Animal>()
            .UseTptMappingStrategy()
            .ComplexProperty(a => a.Details, b => b.ToJson());
        modelBuilder.Entity<Dog>();
        modelBuilder.Entity<Cat>();

        // LIVE: remove ", b => b.ToJson()" to see the plain columns Details_BirthDate / Details_Veterinarian instead.
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
