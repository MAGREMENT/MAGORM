using Base.Extensibility;

namespace Tests;

public class ExtensibilityTests
{
    [Test]
    public void CarTests()
    {
        
    }

    private IExtensionCollection<ICarExtension> _collection;
    
    public void SetExtensionCollection<T>(IExtensionCollection<T> collection)
    {
        var tType = typeof(T);
        if(typeof(T) == tType)
        {
            _collection = (IExtensionCollection<ICarExtension>)collection;
        }
    }
}

[Extensible(nameof(ICarExtension))]
public partial class Car
{
    [ExtensionBaseMethod(nameof(ICarExtension.ChangeGear))]   
    public bool BaseChangeGear(int newGear, int oldGear)
    {
        return true;
    }
}

public partial struct CarBase;

[ExtensionTemplate(nameof(Car), nameof(CarBase))]
public interface ICarExtension
{
    public bool ChangeGear(CarBase b, int newGear, int oldGear);

    public void Accelerate(CarBase b, double strength);
}