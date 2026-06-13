using APIConnector.ASP.NET.CORE;
using Bridge.ORM.APIConnector;
using ORM;
using ORM.Abstract;
using ORM.Languages;
using ORM.Languages.SQLite;
using ORM.SQLite.Microsoft;

namespace API;

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
        app.Run();
    }
}