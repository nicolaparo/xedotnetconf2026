#:package Microsoft.EntityFrameworkCore.SqlServer
#:property PublishAot=false

// AFTER: FullJoin is a new LINQ operator in .NET 11; EF Core 11 translates it to FULL JOIN.
// On the .NET 10 SDK this does not compile (no Queryable.FullJoin).
// Only prints the SQL (ToQueryString) - no database needed.

using System.Reflection;
using Microsoft.EntityFrameworkCore;

Console.WriteLine($"EF Core {typeof(DbContext).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion}");

using var db = new ShopContext();

var query = db.Customers.FullJoin(
    db.Orders,
    c => c.Id,
    o => o.CustomerId,
    (c, o) => new { Customer = c, Order = o });

Console.WriteLine(query.ToQueryString());

class ShopContext : DbContext
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlServer("Server=.;Database=Demo;Trusted_Connection=True;TrustServerCertificate=True");
}

class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
}
