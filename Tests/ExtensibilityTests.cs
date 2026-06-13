using Base.Extensibility;

namespace Tests;

public class ExtensibilityTests
{
    
}

[Extensible]
public partial class Car
{
    
}

public partial struct CarBase
{
    
}

[ExtensionTemplate(nameof(Car), nameof(CarBase))]
public interface ICarExtension
{
    public bool ChangeGear(CarBase b, int newGear, int oldGear);

    public void Accelerate(CarBase b, double strength);
}