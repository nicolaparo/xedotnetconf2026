#:property PublishAot=false

// System.Text.Json on .NET 11 understands C# 15 unions: it writes the active case, no wrapper, no discriminator.
// Reading back needs a way to tell the cases apart. On the .NET 10 SDK this file does not compile.
using System.Text.Json;
using System.Text.Json.Serialization;

Pet pet = new Dog("Rex", "Labrador");
Console.WriteLine($"serialize:   {JsonSerializer.Serialize(pet)}");
Console.WriteLine($"default:     {JsonSerializer.Serialize(default(Pet))}");
Console.WriteLine($"in a record: {JsonSerializer.Serialize(new Owner("Ann", new Cat("Tom", 9)))}");

try
{
    var back = JsonSerializer.Deserialize<Pet>("""{"Name":"Tom","Lives":9}""");
    Console.WriteLine($"deserialize: {back.Value}");
}
catch (JsonException ex)
{
    Console.WriteLine($"deserialize: {ex.Message.Split(" Path:")[0]}");
}

// LIVE: remove the [JsonUnion] attribute below -> both cases are JSON objects -> "ambiguous" error.
// The structural classifier picks Cat because the payload has "Lives".

public record class Cat(string Name, int Lives);
public record class Dog(string Name, string Breed);
public record Owner(string Name, Pet Pet);

[JsonUnion(TypeClassifier = typeof(JsonUnionTypeStructuralClassifier))]
public union Pet(Cat, Dog);
