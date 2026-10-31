#:package Microsoft.EntityFrameworkCore.SqlServer
#:property PublishAot=false

// BEFORE: to configure a property of a complex type you first had to get its builder.

using System.Reflection;
using Microsoft.EntityFrameworkCore;

Console.WriteLine($"EF Core {typeof(DbContext).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion}");

using var db = new ShopContext();
Console.WriteLine(db.Database.GenerateCreateScript());

class ShopContext : DbContext
{
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlServer("Server=.;Database=Demo;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.Entity<Customer>()
            .ComplexProperty(c => c.Address)
            .Property(a => a.PostalCode)
            .HasMaxLength(10);
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
