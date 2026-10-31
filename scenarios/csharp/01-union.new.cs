#:property PublishAot=false

// AFTER (C# 15, .NET 11 SDK): a closed set of case types, checked by the compiler.
// On the .NET 10 SDK this file does not compile: the 'union' keyword is unknown.

Console.WriteLine(Describe(new Dog("Rex", "Labrador")));   // implicit conversion Dog -> Pet
Console.WriteLine(Describe(new Cat("Whiskers")));

// default(Pet) holds nothing (Value is null). Without the 'null' arm the switch would throw.
Pet empty = default;
Console.WriteLine(Describe(empty));

// LIVE 1: delete the Bird arm below and re-run -> the compiler warns the switch is not exhaustive.
// LIVE 2: add Lizard (record + union member) -> every switch without a Lizard arm warns.

static string Describe(Pet pet) => pet switch
{
    Dog d  => $"Dog {d.Name} ({d.Breed})",
    Cat c  => $"Cat {c.Name}",
    Bird b => $"Bird {b.Name}",
    null   => "nothing (default value)",
};

public record class Cat(string Name);
public record class Dog(string Name, string Breed);
public record class Bird(string Name);

public union Pet(Cat, Dog, Bird);
