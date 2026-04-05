namespace ApiConnector;

public interface IEndpointProperty;

[AttributeUsage(AttributeTargets.Property)]
public class EndpointPropertyAttribute : Attribute, IEndpointProperty;