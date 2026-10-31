#:property PublishAot=false

// The 'union' keyword is sugar: the compiler emits a struct marked [Union] with one constructor per case type
// and a Value property. Write that by hand (Shape) and you get the same conversions and exhaustive switch.
// On the .NET 10 SDK this does not compile: 'union' is unknown (Pet), and per the docs UnionAttribute / IUnion
// only ship with the .NET 11 runtime (Shape).
using System.Reflection;
using System.Runtime.CompilerServices;

Shape shape = new Circle(5);                       // implicit union conversion -> Shape(Circle) constructor
Console.WriteLine($"{Area(shape):F2}");
Console.WriteLine($"{Area(new Rectangle(2, 3)):F2}");

// LIVE 1: delete the Shape(Rectangle) constructor -> 'new Rectangle(2, 3)' no longer converts, and the Rectangle arm is an error.
// LIVE 2: delete [Union] -> the type is an ordinary struct and the switch below is no longer exhaustive.
static double Area(Shape s) => s switch
{
    Circle c    => Math.PI * c.Radius * c.Radius,
    Rectangle r => r.Width * r.Height,
};

// Same shape, two declarations: what the compiler generated for the keyword form vs. what we wrote.
Inspect(typeof(Pet));     // declared with the 'union' keyword
Inspect(typeof(Shape));   // declared by hand

static void Inspect(Type t)
{
    var cases = string.Join(", ", t.GetConstructors().Select(c => c.GetParameters()[0].ParameterType.Name));
    Console.WriteLine($"{t.Name,-6} struct={t.IsValueType}  [Union]={t.GetCustomAttribute<UnionAttribute>() is not null}  " +
                      $"IUnion={t.IsAssignableTo(typeof(IUnion))}  ctors=({cases})  Value={t.GetProperty("Value")?.PropertyType.Name}");
}

public union Pet(Cat, Dog);

[Union]
public struct Shape : IUnion
{
    private readonly object? _value;

    public Shape(Circle value) { _value = value; }
    public Shape(Rectangle value) { _value = value; }

    public object? Value => _value;
}

public record class Cat(string Name);
public record class Dog(string Name);
public record class Circle(double Radius);
public record class Rectangle(double Width, double Height);
