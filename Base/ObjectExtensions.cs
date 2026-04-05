namespace Base;

public static class ObjectExtensions
{
    public static bool NullableEquals(this object? left, object? right)
    {
        if (left is null)
        {
            return right is null;
        }

        return left.Equals(right);
    }
}