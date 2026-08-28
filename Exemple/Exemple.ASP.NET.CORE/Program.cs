using API;
using API.ASP.NET.CORE;
using API.StaticFiles;
using Bridge.ORM.API;
using Bridge.WebServer.Scripts;
using ORM;
using ORM.Abstract;
using ORM.Languages;
using ORM.SQLite.Microsoft;
using Endpoint = API.Endpoint;

namespace Exemple.ASP.NET.CORE;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        var app = builder.Build();
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        var db = new Database(new SQLiteDatabaseEngine("Data Source=example.db"),
            new DictionaryModelRegistry());

        var author = new DictionaryApiModel("Author", Models.BasePrimaryKey, 
            Fields.String("Name"));
        
        author.AddEndpoint(new Endpoint(EndpointType.GET, "/someauthor", () => EndpointResult.Content("""
        <!DOCTYPE html>
        <html lang="en">
         <head>
           <meta charset="UTF-8" />
           <meta name="viewport" content="width=device-width, initial-scale=1.0" />
           <title>Testing</title>
         </head>
         <body>
           <p>Hello world !</p>
         </body>
        </html>                                
        """)));
        
        var book = new DictionaryApiModel("Book", Models.BasePrimaryKey, 
            Fields.String("Title"),
            Fields.Int("Price"),
            Fields.Reference("Author", "Author"));
        
        db.AddModels(author, book);
        db.Sync();

        var eDefiner = new WebApplicationApi(app);
        db.DefineModelsEndpoints(eDefiner);
        
        var staticFiles = new InMemoryStaticFileMapper();
        staticFiles.UseFramework();
        eDefiner.UseStaticFiles(staticFiles);
        
        app.Run();
    }
}