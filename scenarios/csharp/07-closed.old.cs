#:property PublishAot=false

// BEFORE: abstract record + private protected constructor "closes" the hierarchy by convention,
// but the compiler still demands a default arm because it does not know the list is closed.

Console.WriteLine(Describe(new Running(40)));
Console.WriteLine(Describe(new Failed("disk full")));

static string Describe(JobStatus status) => status switch
{
    Queued           => "queued",
    Running(var p)   => $"{p}% done",
    Completed(var t) => $"finished in {t.TotalSeconds}s",
    Failed(var e)    => $"failed: {e}",
    _                => throw new InvalidOperationException("unreachable... we hope"),
};

abstract record JobStatus { private protected JobStatus() { } }
record Queued : JobStatus;
record Running(int PercentComplete) : JobStatus;
record Completed(TimeSpan Elapsed) : JobStatus;
record Failed(string Error) : JobStatus;
