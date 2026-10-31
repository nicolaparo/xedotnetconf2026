#:package Microsoft.EntityFrameworkCore.Sqlite
#:property PublishAot=false
#:property NoWarn=CA2266
#:include ../../shared/BlogDb.cs

// SAME code on EF Core 10 and EF Core 11 - only the SQL differs. There is no old/new variant.
// Compare the SECOND command (the Posts split query):
//   EF 10: joins BlogType for no reason and orders by [b].[Id], [b0].[Id]
//   EF 11: no BlogType join, ORDER BY [b].[Id] only
// Model + seed data: shared/BlogDb.cs. SQLite is used so the queries really execute; the pruning lives in shared relational code,
// so it should show up here too (check this - the docs show SQL Server output).

using System.Reflection;
using Microsoft.EntityFrameworkCore;

Console.WriteLine($"EF Core {typeof(DbContext).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion}");

using var connection = BlogDb.Seed();          // seeded without logging, so only the query under test is printed
using var db = BlogDb.CreateLoggedContext(connection);

var blogs = db.Blogs
    .Include(b => b.BlogType)
    .Include(b => b.Posts)
    .AsSplitQuery()
    .ToList();

Console.WriteLine($"{blogs.Count} blogs, {blogs.Sum(b => b.Posts.Count)} posts");
