#:package Microsoft.EntityFrameworkCore.Sqlite
#:property PublishAot=false
#:property NoWarn=CA2266
#:include ../../shared/ZooDb.cs

// A C# 15 union as an entity property. Model + seed data: shared/ZooDb.cs (Owner.Pet is a union).
// EF Core 11 does not map unions (efcore #36375); a value converter through System.Text.Json works.
// On the .NET 10 SDK this file does not compile ('union' is unknown).

using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

Console.WriteLine($"EF Core {typeof(DbContext).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion}");

// 1. Let EF map it by convention.
Try("plain property", () => ZooDb.Seed());

// 2. Store the union as JSON text. STJ (.NET 11) writes the active case, the classifier reads it back.
// LIVE: delete the [JsonUnion] attribute in shared/ZooDb.cs -> reading back fails (ambiguous union).
Try("value converter", () =>
{
    void Configure(ModelBuilder mb) => mb.Entity<Owner>().Property(o => o.Pet).HasConversion(
        new ValueConverter<Pet, string>(p => JsonSerializer.Serialize(p, (JsonSerializerOptions?)null), s => JsonSerializer.Deserialize<Pet>(s, (JsonSerializerOptions?)null)));

    var (conn, options) = ZooDb.Seed(Configure);
    using var _ = conn;
    using var db = new ZooContext(options, Configure);
    foreach (var o in db.Owners.OrderBy(o => o.Id))
        Console.WriteLine($"  {o.Name} -> {o.Pet.Value}");
    Console.WriteLine($"  stored: {db.Database.SqlQueryRaw<string>("SELECT Pet AS Value FROM Owners").ToList()[0]}");
});

static void Try(string label, Action action)
{
    Console.WriteLine($"--- {label}");
    try { action(); }
    catch (Exception ex) { Console.WriteLine($"  {ex.GetType().Name}: {ex.Message.Split('\n')[0]}"); }
}
