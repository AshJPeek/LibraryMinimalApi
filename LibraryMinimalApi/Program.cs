using LibraryMinimalApi.Data;
using LibraryMinimalApi.Models;
using LibraryMinimalApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
    new SqliteConnectionFactory(
        builder.Configuration.GetValue<string>("Database:ConnectionString")));
builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddSingleton<IBookService, BookService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("books", async (Book book, IBookService bookService) =>
{
    var created = await bookService.CreateAsync(book);
    if (!created)
    {
        return Results.BadRequest(new
        {
            errorMessage = "A book with this Isbn already exists."
        });
    }

    return Results.Created($"/books/{book.Isbn}", book);
});

var databaseInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await databaseInitializer.InitializeAsync();


app.UseHttpsRedirection();



app.Run();

