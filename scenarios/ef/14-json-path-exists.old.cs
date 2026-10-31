#:package Microsoft.EntityFrameworkCore.SqlServer
#:property PublishAot=false

// BEFORE: JSON_PATH_EXISTS meant raw SQL. Prints the SQL (no database needed).

using System.Reflection;
using Microsoft.EntityFrameworkCore;

Console.WriteLine($"EF Core {typeof(DbContext).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion}");

using var db = new BlogContext();

var query = db.Blogs.FromSqlRaw("SELECT * FROM Blogs WHERE JSON_PATH_EXISTS(JsonData, N'$.OptionalInt') = 1");
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
