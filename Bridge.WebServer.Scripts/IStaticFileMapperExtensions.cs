using API;
using WebServer;

namespace Bridge.WebServer.Scripts;

public static class IStaticFileMapperExtensions
{
    public static void UseFramework(this IStaticFileMapper mapper)
    {
        mapper.MapStaticFilesDirectory("./JS/src", ".");
    }
}