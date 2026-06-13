namespace Base.Extensibility;

[AttributeUsage(AttributeTargets.Class)]
public class ExtensibleAttribute(params string[] templateNames) : Attribute;

[AttributeUsage(AttributeTargets.Interface)]
public class ExtensionTemplateAttribute(string extensionName, string baseName) : Attribute;

[AttributeUsage(AttributeTargets.Method)]
public class ExtensionBaseMethodAttribute(string methodName) : Attribute;