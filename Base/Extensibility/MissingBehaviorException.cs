namespace Base.Extensibility;

public class MissingBehaviorException(string functionName) 
    : Exception(functionName + " does not have any extension nor any default behavior");