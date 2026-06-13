namespace Base.Dependency;

public static class DependencyResolutionAlgorithms
{
    public static T[] Best<T>(IDependencyCollection<T> collection) where T : INamed
        => ResolveMultiPass(collection);

    public static T[] ResolveMultiPass<T>(IDependencyCollection<T> collection) where T : INamed
    {
        var result = new T[collection.Count];
        var resultIndex = 0;
        var done = new HashSet<string>();
        var buffer = new T[collection.Count - 1];
        var bufferIndex = 0;

        foreach (var d in collection.Enumerate())
        {
            if (collection.GetDependsOnCount(d) == 0)
            {
                result[resultIndex++] = d;
                done.Add(d.Name);
            }
            else buffer[bufferIndex++] = d;
        }

        if (resultIndex == 0) throw new UnresolvableDependenciesException();

        while (bufferIndex > 0)
        {
            var currIndex = 0;
            for (int i = 0; i < bufferIndex; i++)
            {
                var ok = true;
                foreach (var name in collection.GetDependsOn(buffer[i]))
                {
                    if (!done.Contains(name))
                    {
                        ok = false;
                        break;
                    }
                }

                if (ok) result[resultIndex++] = buffer[i];
                else buffer[currIndex++] = buffer[i];
            }

            if(bufferIndex == currIndex) throw new UnresolvableDependenciesException();
            
            bufferIndex = currIndex;
        }

        return result;
    }
}

public class UnresolvableDependenciesException : Exception
{
    public UnresolvableDependenciesException() : base("The dependencies could not be resolved") {}
}