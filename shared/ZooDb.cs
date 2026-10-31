// Shared by the union + EF Core demos. Included with: #:include ../../shared/ZooDb.cs
// (the path is relative to <tfm>/work/, where run.ps1 copies the scenario).
// Needs the Microsoft.EntityFrameworkCore.Sqlite package in the including file.
using System.Text.Json.Serialization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

public record class Cat(string Name, int Lives);
public record class Dog(string Name, string Breed);

[JsonUnion(TypeClassifier = typeof(JsonUnionTypeStructuralClassifier))]
public union Pet(Cat, Dog);

public class Owner
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public Pet Pet { get; set; }
}

public class ZooContext(DbContextOptions<ZooContext> options, Action<ModelBuilder>? configure = null) : DbContext(options)
{
    public DbSet<Owner> Owners => Set<Owner>();
    protected override void OnModelCreating(ModelBuilder mb) => configure?.Invoke(mb);
}

public static class ZooDb
{
    /// <summary>Opens an in-memory SQLite database, creates the schema and inserts two owners.</summary>
    public static (SqliteConnection Connection, DbContextOptions<ZooContext> Options) Seed(Action<ModelBuilder>? configure = null)
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<ZooContext>().UseSqlite(connection).Options;
        using var db = new ZooContext(options, configure);
        db.Database.EnsureCreated();
        db.Owners.AddRange(
            new Owner { Name = "Ann", Pet = new Dog("Rex", "Labrador") },
            new Owner { Name = "Bob", Pet = new Cat("Tom", 9) });
        db.SaveChanges();
        return (connection, options);
    }
}
