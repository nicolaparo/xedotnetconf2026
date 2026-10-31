#:package Microsoft.EntityFrameworkCore.SqlServer
#:property PublishAot=false

// AFTER: Property(c => c.Address.PostalCode) reaches into the complex type directly.
// Compiles on both SDKs; EF Core 10 is expected to throw at runtime (verify the message).

using System.Reflection;
using Microsoft.EntityFrameworkCore;

Console.WriteLine($"EF Core {typeof(DbContext).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion}");

using var db = new ShopContext();

try
{
    Console.WriteLine(db.Database.GenerateCreateScript());
}
catch (Exception ex)
{
    Console.WriteLine($"{ex.GetType().Name}: {ex.Message}");
}

class ShopContext : DbContext
{
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlServer("Server=.;Database=Demo;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>().ComplexProperty(c => c.Address);
        modelBuilder.Entity<Customer>().Property(c => c.Address.PostalCode).HasMaxLength(10);
    }
}

class Customer
{
    public int Id { get; set; }
    public required Address Address { get; set; }
}

class Address
{
    public string Street { get; set; } = "";
    public string PostalCode { get; set; } = "";
}
