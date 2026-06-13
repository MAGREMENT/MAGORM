using Base.Extensibility;

namespace Tests;

public class ExtensibilityTests
{
    [Test]
    public void CarTests()
    {
        var car = new Car();
        Assert.That(car.ChangeGear(2), Is.False);
        Assert.That(car.Gear, Is.EqualTo(1));
        
        var collection = new CarExtensionExtensionCollection();
        car.SetExtensionCollection(collection);
        
        Assert.That(car.ChangeGear(2), Is.False);
        Assert.That(car.Gear, Is.EqualTo(1));

        collection.Add(new FiveGearCar());
        Assert.That(car.ChangeGear(2), Is.True);
        Assert.That(car.Gear, Is.EqualTo(2));
    }
}

[Extensible(nameof(ICarExtension))]
public partial class Car
{
    public int Gear = 1;
    public int Speed = 0;
    
    [ExtensionBaseMethod(nameof(ICarExtension.ChangeGear))]   
    public bool BaseChangeGear(int newGear)
    {
        return false;
    }
}

public partial struct CarBase;

[ExtensionTemplate(nameof(Car), nameof(CarBase))]
public interface ICarExtension
{
    public bool ChangeGear(CarBase b, int newGear);

    public void Accelerate(CarBase b, double strength);
}

public class FiveGearCar : ICarExtension
{
    public bool ChangeGear(CarBase b, int newGear)
    {
        if (newGear <= 5)
        {
            b.Subject.Gear = newGear;
            return true;
        }

        return false;
    }

    public void Accelerate(CarBase b, double strength)
    {
        b.Accelerate(strength);
    }
}