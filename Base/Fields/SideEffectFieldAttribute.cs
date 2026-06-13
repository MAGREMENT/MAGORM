namespace Base.Fields;

[AttributeUsage(AttributeTargets.Property)]
public class SideEffectFieldAttribute : Attribute;

[AttributeUsage(AttributeTargets.Class)]
public class SideEffectFieldCollectionAttribute : Attribute;