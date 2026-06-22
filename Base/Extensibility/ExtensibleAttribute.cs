namespace Base.Extensibility;

[AttributeUsage(AttributeTargets.Class)]
public class ExtensibleClassAttribute(string extensionName, string baseName) : Attribute;

[AttributeUsage(AttributeTargets.Method)]
public class ExtensibleMethodAttribute(string? baseMethodName = null) : Attribute;