#:property PublishAot=false

// AFTER (C# 15): 'closed' fixes the set of direct descendants at compile time.
// The switch is exhaustive with no default arm.
// On the .NET 10 SDK (C# 14) this does not compile.
//
// NOTE: in .NET 11 preview 5 the runtime did not ship System.Runtime.CompilerServices.ClosedAttribute
// yet and each project had to declare it. If the compiler complains about a missing ClosedAttribute,
// check the "closed modifier" docs page for the exact shim and add it at the bottom of this file.

Console.WriteLine(Describe(new Running(40)));
Console.WriteLine(Describe(new Failed("disk full")));

// LIVE: delete the Failed arm -> exhaustiveness warning. Add a new record deriving from JobStatus -> every switch warns.

static string Describe(JobStatus status) => status switch
{
    Queued           => "queued",
    Running(var p)   => $"{p}% done",
    Completed(var t) => $"finished in {t.TotalSeconds}s",
    Failed(var e)    => $"failed: {e}",
};

public closed record class JobStatus;
public record class Queued : JobStatus;
public record class Running(int PercentComplete) : JobStatus;
public record class Completed(TimeSpan Elapsed) : JobStatus;
public record class Failed(string Error) : JobStatus;
