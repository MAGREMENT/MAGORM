namespace Base;

[AttributeUsage(AttributeTargets.Property)]
public class GeneratedPropertyAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Class)]
public class GeneratedPropertyCollectionAttribute : Attribute
{
}