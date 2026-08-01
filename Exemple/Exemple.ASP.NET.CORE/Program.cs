using API.ASP.NET.CORE;
using Bridge.ORM.API;
using Bridge.ORM.WebServer;
using ORM;
using ORM.Abstract;
using ORM.Languages;
using ORM.Languages.SQLite;
using ORM.SQLite.Microsoft;
using WebServer;
using WebServer.ASP.NET.CORE;
using WebServer.StaticFiles;

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

        var db = new Database(new SqLiteLanguage(), new SQLiteDatabaseEngine("Data Source=example.db"),
            new DictionaryModelBank());

        var author = new DictionaryViewApiModel("Author", Models.BasePrimaryKey, 
            Fields.String("Name"));
        
        author.AddView(new View("someauthor", """
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
        """));
        
        var book = new DictionaryViewApiModel("Book", Models.BasePrimaryKey, 
            Fields.String("Title"),
            Fields.Int("Price"),
            Fields.Reference("Author", "Author"));
        
        db.AddModels(author, book);
        db.Sync();

        var eDefiner = new WebApplicationEndpointDefiner(app);
        var vDefiner = new ViewEndpointDefiner(eDefiner);
        db.DefineModelsEndpoints(eDefiner);
        db.DefineModelsViews(vDefiner);
        
        var staticFiles = new InMemoryStaticFileMapper();
        staticFiles.MapStaticFilesDirectory("./JS", ".");
        app.UseStaticFileMapper(staticFiles);
        
        app.Run();
    }
}