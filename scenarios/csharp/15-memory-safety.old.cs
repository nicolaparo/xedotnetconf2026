#:property AllowUnsafeBlocks=true
#:property PublishAot=false

// BEFORE (C# 14 and earlier): `unsafe` is required as soon as a pointer type appears,
// even for operations that cannot corrupt memory (declaring a pointer, taking an address, fixed).
// Works on both SDKs.
//
// WHAT IT DOES
//   Pin an array, walk it through a pointer, sum the elements.
//   The whole method has to be unsafe, so the reader cannot tell which line is the risky one.
//
// EXPECTED OUTPUT: sum=60

Console.WriteLine($"sum={Sum([10, 20, 30])}");

static unsafe int Sum(int[] values)
{
    int total = 0;
    fixed (int* p = values)            // needs unsafe in C# 14
    {
        for (int i = 0; i < values.Length; i++)
            total += p[i];             // the only line that can really go wrong
    }
    return total;
}
