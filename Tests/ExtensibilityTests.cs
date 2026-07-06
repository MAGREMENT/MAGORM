using Base.Extensibility;

namespace Tests;

/*TODO Improve extensibility
 - Add the possibility of adding new functions (?)
 - Add the possibility of adding new properties (?)
 */
public class ExtensibilityTests
{
    [Test]
    public void SimpleExtensionTest()
    {
        var car = new Car();
        Assert.That(car.ChangeGear(2), Is.True);
        Assert.That(car.Gear, Is.EqualTo(2));
        Assert.That(car.ChangeGear(6), Is.True);
        Assert.That(car.Gear, Is.EqualTo(6));
        
        var collection = new CarExtensionExtensionCollection();
        car.SetExtensionCollection(collection);
        
        Assert.That(car.ChangeGear(3), Is.True);
        Assert.That(car.Gear, Is.EqualTo(3));
        Assert.That(car.ChangeGear(-1), Is.True);
        Assert.That(car.Gear, Is.EqualTo(-1));

        collection.Add(new FiveGearCarExtension());
        Assert.That(car.ChangeGear(2), Is.True);
        Assert.That(car.Gear, Is.EqualTo(2));
        Assert.That(car.ChangeGear(7), Is.False);
        Assert.That(car.Gear, Is.EqualTo(2));
    }

    [Test]
    public void MissingBehaviorTest()
    {
        var car = new Car();
        Assert.Throws<MissingBehaviorException>(() => car.Fly(10));

        var collection = new CarExtensionExtensionCollection();
        car.SetExtensionCollection(collection);
        Assert.Throws<MissingBehaviorException>(() => car.Fly(10));
        
        collection.Add(new FiveGearCarExtension());
        Assert.Throws<MissingBehaviorException>(() => car.Fly(10));
        
        collection.Add(new FlyingCarExtension());
        Assert.DoesNotThrow(() => car.Fly(10));
    }
}

public partial class CarExtension;

public readonly partial struct CarBase;

[ExtensibleClass(nameof(CarExtension), nameof(CarBase))]
public partial class Car
{
    public int Gear { get; set; } = 1;
    public double Speed { get; set; }

    [ExtensibleMethod(nameof(BaseChangeGear))]
    public partial bool ChangeGear(int newGear);

    public bool BaseChangeGear(int newGear)
    {
        Gear = newGear;
        return true;
    }

    [ExtensibleMethod(nameof(BaseAccelerate))]
    public partial void Accelerate(double strength);

    public void BaseAccelerate(double strength)
    {
        Speed = Gear * strength;
    }

    [ExtensibleMethod]
    public partial void Fly(double strength);
}

public class FiveGearCarExtension : CarExtension
{
    public override bool ChangeGear(int newGear, CarBase previous)
    {
        if (newGear is < 1 or > 5) return false;
        return previous.ChangeGear(newGear);
    }
}

public class FlyingCarExtension : CarExtension
{
    public override void Fly(double strength, CarBase previous)
    {
        previous.Subject.Speed *= strength;
    }
}