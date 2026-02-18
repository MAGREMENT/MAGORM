using Core.Abstract;

namespace Core.Util;

public static class DependencyResolutionAlgorithms
{
    public static T[] Best<T>(IReadOnlyList<T> dependencies) where T : IDependencies
        => ResolveMultiPass(dependencies);

    public static T[] ResolveMultiPass<T>(IReadOnlyList<T> dependencies) where T : IDependencies
    {
        var result = new T[dependencies.Count];
        var resultIndex = 0;
        var done = new HashSet<string>();
        var buffer = new T[dependencies.Count - 1];
        var bufferIndex = 0;

        foreach (var d in dependencies)
        {
            if (d.DependsOn.Count == 0)
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
                foreach (var name in buffer[i].DependsOn)
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