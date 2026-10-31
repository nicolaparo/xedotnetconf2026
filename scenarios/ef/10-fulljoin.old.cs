#:package Microsoft.EntityFrameworkCore.SqlServer
#:property PublishAot=false

// BEFORE: no FULL JOIN in LINQ. Simulate it: left join UNION the right-only rows.
// Only prints the SQL (ToQueryString) - no database needed.

using System.Reflection;
using Microsoft.EntityFrameworkCore;

Console.WriteLine($"EF Core {typeof(DbContext).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion}");

using var db = new ShopContext();

var left =
    from c in db.Customers
    join o in db.Orders on c.Id equals o.CustomerId into orders
    from o in orders.DefaultIfEmpty()
    select new { CustomerId = (int?)c.Id, CustomerName = (string?)c.Name, OrderId = o == null ? (int?)null : o.Id };

var rightOnly =
    from o in db.Orders
    where !db.Customers.Any(c => c.Id == o.CustomerId)
    select new { CustomerId = (int?)null, CustomerName = (string?)null, OrderId = (int?)o.Id };

Console.WriteLine(left.Union(rightOnly).ToQueryString());

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
