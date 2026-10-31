#:package Microsoft.EntityFrameworkCore.SqlServer
#:property PublishAot=false
#:property NoWarn=IL2026;IL3050;EF9105

// AFTER: full-text catalog and index in the model, FreeTextTable with a Rank, and hybrid search
// (full-text + vector) merged with FullJoin and a Reciprocal Rank Fusion score. EF Core 11 only.
// Needs SQL Server 2025 with the full-text component and an Ollama server with an embedding model:
//   XE_SQL    connection string  (default: the Docker container "xe-sql2025" on localhost,14335)
//   XE_OLLAMA Ollama base URL    (default: http://macmini:11434)

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

using var http = new HttpClient { BaseAddress = new Uri(Environment.GetEnvironmentVariable("XE_OLLAMA") ?? "http://macmini:11434") };
async Task<SqlVector<float>> Embed(string text)
{
    var r = await http.PostAsJsonAsync("/api/embed", new { model = "qwen3-embedding:0.6b", input = text });
    return new SqlVector<float>((await r.Content.ReadFromJsonAsync<EmbedResponse>())!.Embeddings[0]);
}

await using var db = new ArticleContext();
var cs = new SqlConnectionStringBuilder(db.Database.GetConnectionString()!);
var dbName = cs.InitialCatalog;
cs.InitialCatalog = "master";
await using (var admin = new SqlConnection(cs.ConnectionString))
{
    await admin.OpenAsync();
    foreach (var sql in new[] { $"DROP DATABASE IF EXISTS [{dbName}]", $"CREATE DATABASE [{dbName}]" })
        await new SqlCommand(sql, admin).ExecuteNonQueryAsync();
}
await db.Database.EnsureCreatedAsync();   // table, full-text catalog and full-text index come from the model
foreach (var t in titles) db.Articles.Add(new Article { Title = t, Embedding = await Embed(t) });
await db.SaveChangesAsync();
while (await db.Database.SqlQueryRaw<int>("SELECT FULLTEXTCATALOGPROPERTY('ftCatalog', 'PopulateStatus') AS Value").SingleAsync() != 0)
    await Task.Delay(200);                // wait until the full-text index has been populated

// 1. Full-text with a ranking: FreeTextTable returns the entity and the Rank.
var text = "cane";
Console.WriteLine($"\nFull-text: {text}");
var ranked = await db.Articles.FreeTextTable<Article, int>(text, a => a.Title, languageTerm: "Italian")
    .Join(db.Articles, f => f.Key, a => a.Id, (f, a) => new { a.Title, f.Rank })
    .OrderByDescending(r => r.Rank).ToListAsync();
foreach (var r in ranked) Console.WriteLine($"{r.Rank,4}  {r.Title}");

// 2. Hybrid: full-text and vector results joined with FullJoin, then a Reciprocal Rank Fusion score.
const int k = 5;
var semantic = await Embed("animali domestici");
var joined = await db.Articles
    .FreeTextTable<Article, int>(text, topN: k)
    .Join(db.Articles, f => f.Key, a => a.Id, (f, a) => new { Article = a, f.Rank })
    .FullJoin(
        db.Articles.Select(a => new { a.Id, a.Title, Distance = EF.Functions.VectorDistance("cosine", a.Embedding, semantic) })
            .OrderBy(v => v.Distance).Take(k),
        f => f.Article.Id, v => v.Id,
        (f, v) => new
        {
            TextTitle = f.Article.Title,
            VectorTitle = v.Title,
            FullTextRank = (int?)f.Rank,
            Distance = (double?)v.Distance,
        })
    .ToListAsync();
// Reciprocal Rank Fusion on the joined rows (the FULL JOIN itself runs in SQL).
var fused = joined
    .Select(x => (Title: x.TextTitle ?? x.VectorTitle, x.FullTextRank, x.Distance,
        Rrf: (x.FullTextRank is { } r ? 1.0 / (k + r) : 0.0) + (x.Distance is { } d ? 1.0 / (k + d) : 0.0)))
    .OrderByDescending(x => x.Rrf).Take(5).ToList();
Console.WriteLine($"\nHybrid: '{text}' + semantic 'animali domestici'");
foreach (var x in fused) Console.WriteLine($"{x.Rrf:F3}  rank={x.FullTextRank,-4} dist={x.Distance:F3}  {x.Title}");

record EmbedResponse(float[][] Embeddings);

class ArticleContext : DbContext
{
    public DbSet<Article> Articles => Set<Article>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlServer(Environment.GetEnvironmentVariable("XE_SQL")
            ?? "Server=localhost,14335;Database=XeFullText;User Id=sa;Password=Xe!Demo2026pass;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasFullTextCatalog("ftCatalog");
        modelBuilder.Entity<Article>().HasFullTextIndex(a => a.Title).UseKeyIndex("PK_Articles").UseCatalog("ftCatalog");
        modelBuilder.Entity<Article>().Property(a => a.Embedding).HasColumnType("vector(1024)");
    }
}

class Article
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public SqlVector<float> Embedding { get; set; }
}
