#:property PublishAot=false

// BEFORE (C# 14 and earlier): "one of these types" is modelled with object + a default arm.
// Compiles on every SDK. The compiler cannot tell you when a case is missing.

Console.WriteLine(Describe(new Dog("Rex", "Labrador")));

// A new kind of pet shows up... nothing warns us. It fails at runtime.
try
{
    Console.WriteLine(Describe(new Lizard("Liz")));
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Runtime failure: {ex.Message}");
}

static string Describe(object pet) => pet switch
{
    Dog d  => $"Dog {d.Name} ({d.Breed})",
    Cat c  => $"Cat {c.Name}",
    Bird b => $"Bird {b.Name}",
    _      => throw new InvalidOperationException($"Unhandled pet type: {pet.GetType().Name}"),
};

record Cat(string Name);
record Dog(string Name, string Breed);
record Bird(string Name);
record Lizard(string Name);
