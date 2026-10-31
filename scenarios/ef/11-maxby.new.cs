#:package Microsoft.EntityFrameworkCore.Sqlite
#:property PublishAot=false
#:property NoWarn=CA2266
#:include ../../shared/BlogDb.cs

// AFTER: MaxBy is translated by EF Core 11. The code compiles on both SDKs (Queryable.MaxBy exists
// since .NET 6) but EF Core 10 is expected to throw a translation exception at runtime (verify).

using System.Reflection;
using Microsoft.EntityFrameworkCore;

Console.WriteLine($"EF Core {typeof(DbContext).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion}");

using var connection = BlogDb.Seed();
using var db = BlogDb.CreateLoggedContext(connection);

try
{
    var top = db.Blogs.MaxBy(b => b.Posts.Count());
    Console.WriteLine($"Blog with most posts: {top?.Name}");
}
catch (Exception ex)
{
    Console.WriteLine($"{ex.GetType().Name}: {ex.Message.Split('.')[0]}.");
}
