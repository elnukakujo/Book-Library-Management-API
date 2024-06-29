using Microsoft.EntityFrameworkCore; // Add this using directive
using NSwag.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Configure DbContext with SQLite Database
builder.Services.AddDbContext<BookDb>(opt => 
    opt.UseSqlite(builder.Configuration.GetConnectionString("BookLibraryConnection")));

// Add Developer Exception Page Middleware for database exceptions
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Add services for generating OpenAPI documents
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "BookAPI";
    config.Title = "BookAPI v1";
    config.Version = "v1";
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(config =>
    {
        config.DocumentTitle = "BookAPI";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });
}

app.MapGet("/books", async (BookDb db) => 
    await db.Books.ToListAsync());

app.MapGet("/books/{id}", async (int id, BookDb db) =>
    await db.Books.FindAsync(id)
        is Book book ? Results.Ok(book) : Results.NotFound());

app.MapPost("/books", async (Book book, BookDb db) =>
{
    db.Books.Add(book);
    await db.SaveChangesAsync();
    return Results.Created($"/books/{book.Id}", book);
});

app.MapPut("/books/{id}", async (int id, Book updatedBook, BookDb db) =>
{
    var book = await db.Books.FindAsync(id);
    if (book is null) return Results.NotFound();
    book.Title = updatedBook.Title;
    book.AuthorLastname = updatedBook.AuthorLastname;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/books/{id}", async (int id, BookDb db) =>
{
    var book = await db.Books.FindAsync(id);
    if (book is Book)
    {
        db.Books.Remove(book);
        await db.SaveChangesAsync();
        return Results.Ok();
    }
    return Results.NotFound();
});

app.Run();