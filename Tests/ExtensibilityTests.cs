using Base.Extensibility;

namespace Tests;

/*TODO Improve extensibility
 - Have method specific extension chain
 - Add the possibility of adding new functions (?)
 - Add the possibility of adding new properties (?)
 */
public class ExtensibilityTests
{
    [Test]
    public void CarTests()
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
}

public partial class CarExtension;

public partial struct CarBase;

[ExtensibleClass(nameof(CarExtension), nameof(CarBase))]
public partial class Car
{
    private int _gear = 1;
    private double _speed;

    public int Gear => _gear;
    public double Speed => _speed;

    [ExtensibleMethod(nameof(BaseChangeGear))]
    public partial bool ChangeGear(int newGear);

    public bool BaseChangeGear(int newGear)
    {
        _gear = newGear;
        return true;
    }

    [ExtensibleMethod(nameof(BaseAccelerate))]
    public partial void Accelerate(double strength);

    public void BaseAccelerate(double strength)
    {
        _speed = _gear * strength;
    }
}

public class FiveGearCarExtension : CarExtension
{
    public override bool ChangeGear(int newGear, CarBase previous)
    {
        if (newGear is < 1 or > 5) return false;
        return previous.ChangeGear(newGear);
    }
}