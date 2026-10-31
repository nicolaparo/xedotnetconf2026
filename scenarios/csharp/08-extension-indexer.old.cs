#:property PublishAot=false

// BEFORE: indexed access to a type you do not own meant a helper method.

IEnumerable<int> numbers = Enumerable.Range(1, 10);
Console.WriteLine($"numbers.ElementAtIndex(2) = {numbers.ElementAtIndex(2)}");

var garage = new Garage { Cars = { new Car("Fiat Panda"), new Car("Alfa Romeo Giulia"), new Car("Lancia Delta") } };
Console.WriteLine($"garage.GetSlot(2) = {garage.GetSlot(2)}");

public static class Extensions
{
    public static int ElementAtIndex(this IEnumerable<int> sequence, int index) => sequence.ElementAt(index);
    public static Car GetSlot(this Garage garage, int slot) => garage.Cars[slot];
}

public record Car(string Model);

public class Garage
{
    public List<Car> Cars { get; } = [];
}
