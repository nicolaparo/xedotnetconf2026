#:property PublishAot=false

// AFTER (C# 15): with(...) forwards arguments to the constructor / factory method.
// On the .NET 10 SDK (C# 14) this does not compile.

string[] values = ["one", "two", "three"];

List<string> names = [with(capacity: values.Length * 2), .. values];
Console.WriteLine($"names: Count={names.Count}, Capacity={names.Capacity}");

HashSet<string> set = [with(StringComparer.OrdinalIgnoreCase), "Hello", "HELLO", "hello"];
Console.WriteLine($"set: Count={set.Count}  <- one element, comparer applied");

// LIVE (check the exact error first): move with(...) away from the first position and compile again.
