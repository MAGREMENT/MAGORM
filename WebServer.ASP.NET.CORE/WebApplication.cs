using API.ASP.NET.CORE;
using Microsoft.AspNetCore.Builder;

namespace WebServer.ASP.NET.CORE;

public static class WebApplicationExtensions
{
    public static void UseStaticFileMapper(this WebApplication app, IStaticFileMapper mapper)
    {
        var definer = new WebApplicationApi(app);
        definer.UseStaticFiles(mapper);
    }
}