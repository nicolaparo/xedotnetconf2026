#:property PublishAot=false

// AFTER (C# 15): indexers inside an extension block (C# 14 introduced extension blocks, not indexers).
// On the .NET 10 SDK this fails: extension blocks compile, the indexer inside them does not.

IEnumerable<int> numbers = Enumerable.Range(1, 10);
Console.WriteLine($"numbers[2] = {numbers[2]}");

var garage = new Garage { Cars = { new Car("Fiat Panda"), new Car("Alfa Romeo Giulia"), new Car("Lancia Delta") } };
Console.WriteLine($"garage[2] = {garage[2]}");

public static class Extensions
{
    extension(IEnumerable<int> sequence)
    {
        public int this[int index] => sequence.ElementAt(index);
    }

    extension(Garage garage)
    {
        public Car this[int slot] => garage.Cars[slot];
    }
}

public record Car(string Model);

public class Garage
{
    public List<Car> Cars { get; } = [];
}
