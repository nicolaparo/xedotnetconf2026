#:property PublishAot=false

// BEFORE: to control capacity or comparer you had to leave collection-expression syntax.

string[] values = ["one", "two", "three"];

List<string> names = new(capacity: values.Length * 2);
names.AddRange(values);
Console.WriteLine($"names: Count={names.Count}, Capacity={names.Capacity}");

var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Hello", "HELLO", "hello" };
Console.WriteLine($"set (constructor + initializer): Count={set.Count}");

// The trap: a plain collection expression gives you no say over the comparer.
HashSet<string> trap = ["Hello", "HELLO", "hello"];
Console.WriteLine($"set (plain collection expression): Count={trap.Count}  <- default comparer, 3 distinct strings");
