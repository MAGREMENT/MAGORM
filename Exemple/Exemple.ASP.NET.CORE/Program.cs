using API.ASP.NET.CORE;
using Bridge.ORM.API;
using ORM;
using ORM.Abstract;
using ORM.Languages;
using ORM.Languages.SQLite;
using ORM.SQLite.Microsoft;
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

        var author = ApiModels.DefineBase("Author",
            Fields.String("Name"));
        
        var book = ApiModels.DefineBase("Book",
            Fields.String("Title"),
            Fields.Int("Price"),
            Fields.Reference("Author", "Author"));
        
        db.AddModels(author, book);
        app.MapModels(db);
        
        var staticFiles = new InMemoryStaticFileMapper();
        staticFiles.MapStaticFilesDirectory("./JS", ".");
        app.UseStaticFileMapper(staticFiles);
        
        app.Run();
    }
}