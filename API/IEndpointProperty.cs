namespace API;

public interface IEndpointProperty;

[AttributeUsage(AttributeTargets.Property)]
public class EndpointFieldAttribute : Attribute, IEndpointProperty;