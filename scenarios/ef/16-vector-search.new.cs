#:package Microsoft.EntityFrameworkCore.SqlServer
#:property PublishAot=false
#:property NoWarn=IL2026;IL3050;EF9105

// AFTER: vector index in the model (HasVectorIndex) and VectorSearch(...).WithApproximate() in EF Core 11.
// Needs SQL Server 2025 and an Ollama server with an embedding model. Both are read from environment variables:
//   XE_SQL    connection string  (default: the Docker container "xe-sql2025" on localhost,14335)
//   XE_OLLAMA Ollama base URL    (default: http://localhost:11434)
// On EF Core 10 this does not compile (no HasVectorIndex, no VectorSearch).

using System.Net.Http.Json;
using System.Reflection;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlTypes;
using Microsoft.EntityFrameworkCore;

Console.WriteLine($"EF Core {typeof(DbContext).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion}");

string[] titles =
[
    "Il mio gatto dorme tutto il giorno", "Come addestrare un cucciolo di cane", "Ricetta della carbonara",
    "Pasta fresca fatta in casa", "Guida a C# 15 e union types", "Novità di Entity Framework Core 11",
    "Allenamento per la maratona", "Correre la mattina presto", "Il pappagallo che parla",
];

using var http = new HttpClient { BaseAddress = new Uri(Environment.GetEnvironmentVariable("XE_OLLAMA") ?? "http://localhost:11434") };
async Task<SqlVector<float>> Embed(string text)
{
    var r = await http.PostAsJsonAsync("/api/embed", new { model = "qwen3-embedding:0.6b", input = text });
    var json = await r.Content.ReadFromJsonAsync<EmbedResponse>();
    return new SqlVector<float>(json!.Embeddings[0]);
}

await using var db = new SeedContext();   // same model without the index (EF caches one model per context type)
// Setup: vector indexes are a preview feature in SQL Server 2025, so they must be switched on per database.
var cs = new SqlConnectionStringBuilder(db.Database.GetConnectionString()!);
var dbName = cs.InitialCatalog;
cs.InitialCatalog = "master";
await using (var admin = new SqlConnection(cs.ConnectionString))
{
    await admin.OpenAsync();
    foreach (var sql in new[] { $"DROP DATABASE IF EXISTS [{dbName}]", $"CREATE DATABASE [{dbName}]" })
        await new SqlCommand(sql, admin).ExecuteNonQueryAsync();
    admin.ChangeDatabase(dbName);
    await new SqlCommand("ALTER DATABASE SCOPED CONFIGURATION SET PREVIEW_FEATURES = ON", admin).ExecuteNonQueryAsync();
}
// The table is read-only once it has a vector index (SQL Server 2025 preview), so: table, data, then index.
await db.Database.EnsureCreatedAsync();
foreach (var t in titles) db.Blogs.Add(new Blog { Name = t, Embedding = await Embed(t) });
await db.SaveChangesAsync();
await using (var withIndex = new BlogContext())   // the model has HasVectorIndex: take the CREATE VECTOR INDEX from its script
{
    var createIndex = withIndex.Database.GenerateCreateScript().Split((char)10).First(l => l.Contains("CREATE VECTOR INDEX"));
    Console.WriteLine(createIndex);
    await db.Database.ExecuteSqlRawAsync(createIndex);
}

await using var search = new BlogContext();
var query = "animali domestici";
var embedding = await Embed(query);
Console.WriteLine($"\nSearch: {query}");

// 1. EF Core 10 API, exact distance: runs on SQL Server 2025.
var exact = await search.Blogs
    .Select(b => new { b.Name, Distance = EF.Functions.VectorDistance("cosine", b.Embedding, embedding) })
    .OrderBy(r => r.Distance).Take(3).ToListAsync();
foreach (var r in exact) Console.WriteLine($"{r.Distance:F3}  {r.Name}");

// 2. EF Core 11 API: VectorSearch + WithApproximate. Only the SQL is printed, see the note below.
var approx = search.Blogs
    .VectorSearch(b => b.Embedding, embedding, "cosine")
    .OrderBy(r => r.Distance)
    .Take(3)
    .WithApproximate();                   // LIVE: remove this line for an exact (kNN) search
Console.WriteLine("\n" + approx.ToQueryString());
// Executing it on the SQL Server 2025 container (17.0.5035, CU10) fails: "Incorrect syntax near 'TOP_N'".
// EF 11 emits SELECT TOP(n) [WITH APPROXIMATE] ... FROM VECTOR_SEARCH(...), the new syntax; this build still wants the TOP_N parameter.

record EmbedResponse(float[][] Embeddings);

class SeedContext : BlogContext { protected override bool WithIndex => false; }

class BlogContext : DbContext
{
    protected virtual bool WithIndex => true;
    public DbSet<Blog> Blogs => Set<Blog>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlServer(Environment.GetEnvironmentVariable("XE_SQL")
            ?? "Server=localhost,14335;Database=XeVector;User Id=sa;Password=Xe!Demo2026pass;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Blog>().Property(b => b.Embedding).HasColumnType("vector(1024)");
        if (WithIndex) modelBuilder.Entity<Blog>().HasVectorIndex(b => b.Embedding).HasMetric("cosine");
    }
}

class Blog
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public SqlVector<float> Embedding { get; set; }
}
