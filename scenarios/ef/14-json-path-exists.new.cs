#:package Microsoft.EntityFrameworkCore.SqlServer
#:property PublishAot=false

// AFTER: EF.Functions.JsonPathExists translates to JSON_PATH_EXISTS (SQL Server 2022+).
// On EF Core 10 this does not compile (no such function). Prints the SQL (no database needed).

using System.Reflection;
using Microsoft.EntityFrameworkCore;

Console.WriteLine($"EF Core {typeof(DbContext).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion}");

using var db = new BlogContext();

var query = db.Blogs.Where(b => EF.Functions.JsonPathExists(b.JsonData, "$.OptionalInt"));
Console.WriteLine(query.ToQueryString());

class BlogContext : DbContext
{
    public DbSet<Blog> Blogs => Set<Blog>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlServer("Server=.;Database=Demo;Trusted_Connection=True;TrustServerCertificate=True");
}

class Blog
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string JsonData { get; set; } = "{}";
}
