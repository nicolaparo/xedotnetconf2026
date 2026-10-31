#:property LangVersion=preview
#:property Features=updated-memory-safety-rules
#:property AllowUnsafeBlocks=true
#:property PublishAot=false

// AFTER (C# 15 preview): `unsafe` is tied to the operations that can really corrupt memory.
// Opt-in: LangVersion=preview (+ the feature flag, per the docs) in the #:property lines above.
//
// WHAT IT DOES
//   Same sum as the old version, but the method is no longer unsafe. Declaring the pointer and
//   `fixed` need no unsafe any more; only the pointer element access is marked, with unsafe(...).
//
// SDK vs FLAGS: this is NOT a .NET 10 vs .NET 11 demo. Ran on 10.0.401 and 11.0.100-rc.3: both
// compile it, because the preview language version carries the feature. What matters is the flags.
//
// EXPECTED OUTPUT: sum=60
// LIVE 1: delete the `#:property LangVersion=preview` line -> CS0214 (pointer outside unsafe)
//         and CS8652 (feature is Preview), on either SDK.
// LIVE 2: remove unsafe(...) around p[i] -> CS9360 "may only be used in an unsafe context",
//         pointing at that single line (the old rules would have flagged the whole method).

Console.WriteLine($"sum={Sum([10, 20, 30])}");

static int Sum(int[] values)
{
    int total = 0;
    fixed (int* p = values)            // no unsafe needed with the new rules
    {
        for (int i = 0; i < values.Length; i++)
            total += unsafe(p[i]);     // element access is the real risk: marked here, and only here
    }
    return total;
}
