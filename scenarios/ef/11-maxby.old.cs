#:package Microsoft.EntityFrameworkCore.Sqlite
#:property PublishAot=false
#:property NoWarn=CA2266
#:include ../../shared/BlogDb.cs

// BEFORE: "the element with the biggest X" = OrderByDescending + FirstOrDefault.

using System.Reflection;
using Microsoft.EntityFrameworkCore;

Console.WriteLine($"EF Core {typeof(DbContext).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion}");

using var connection = BlogDb.Seed();
using var db = BlogDb.CreateLoggedContext(connection);

var top = db.Blogs.OrderByDescending(b => b.Posts.Count()).FirstOrDefault();
Console.WriteLine($"Blog with most posts: {top?.Name}");
