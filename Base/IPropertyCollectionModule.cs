namespace Base;

public interface IPropertyCollectionModule
{
    void BeforeSetValue(string name, object? value);

    void AfterSetValue(string name, object? value);

    void BeforeGetValue(string name);

    void AfterGetValue(string name, object? value);
}