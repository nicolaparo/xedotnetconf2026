// Shared by the Blog/Post EF Core demos (09, 11). Included with: #:include ../../shared/BlogDb.cs
// Needs the Microsoft.EntityFrameworkCore.Sqlite package in the including file.
// Seed data: "small" has 1 post, "big" has 3 posts; both blogs have the BlogType "Tech".
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

public class BlogContext(DbContextOptions<BlogContext> options) : DbContext(options)
{
    public DbSet<Blog> Blogs => Set<Blog>();
}

public class BlogType
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public class Blog
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int BlogTypeId { get; set; }
    public BlogType BlogType { get; set; } = null!;
    public List<Post> Posts { get; set; } = [];
}

public class Post
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public int BlogId { get; set; }
}

public static class BlogDb
{
    /// <summary>Opens an in-memory SQLite database, creates the schema and inserts the seed data (without logging).</summary>
    public static SqliteConnection Seed()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        using var setup = new BlogContext(new DbContextOptionsBuilder<BlogContext>().UseSqlite(connection).Options);
        setup.Database.EnsureCreated();
        var tech = new BlogType { Name = "Tech" };
        setup.Blogs.AddRange(
            new Blog { Name = "small", BlogType = tech, Posts = { new Post { Title = "a" } } },
            new Blog { Name = "big", BlogType = tech, Posts = { new Post { Title = "a" }, new Post { Title = "b" }, new Post { Title = "c" } } });
        setup.SaveChanges();
        return connection;
    }

    /// <summary>A context on the seeded connection that prints every executed SQL command.</summary>
    public static BlogContext CreateLoggedContext(SqliteConnection connection)
        => new(new DbContextOptionsBuilder<BlogContext>()
            .UseSqlite(connection)
            .LogTo(Console.WriteLine, [RelationalEventId.CommandExecuted], LogLevel.Information, DbContextLoggerOptions.None)
            .Options);
}
