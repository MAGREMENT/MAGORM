namespace Base;

[AttributeUsage(AttributeTargets.Property)]
public class TrackedPropertyAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Class)]
public class TrackedPropertyCollectionAttribute : Attribute
{
}