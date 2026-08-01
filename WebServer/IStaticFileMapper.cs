using API;

namespace WebServer;

public interface IStaticFileMapper
{
    void MapStaticFilesDirectory(string directoryPath, string mappedPath, bool recursive = true);

    string? GetStaticFile(string filePath);
}

public static class EndpointDefinerExtensions {

    public static void UseStaticFiles(this IEndpointDefiner definer, IStaticFileMapper mapper)
    {
        definer.AddDefaultHandler(path =>
        {
            var filePath = mapper.GetStaticFile('.' + path);
            if (filePath is null) return new EndpointResult(EndpointResultType.NotFound);
            return new EndpointResult(EndpointResultType.File, filePath);
        });
    }
}