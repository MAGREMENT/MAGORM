namespace Base.Extensibility;

[AttributeUsage(AttributeTargets.Class)]
public class ExtensibleAttribute : Attribute;

[AttributeUsage(AttributeTargets.Interface)]
public class ExtensionTemplateAttribute(string extensionName, string baseName) : Attribute;

[AttributeUsage(AttributeTargets.Method)]
public class ExtensionBaseMethod : Attribute;