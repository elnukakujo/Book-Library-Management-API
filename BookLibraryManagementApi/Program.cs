using Microsoft.AspNetCore.Rewrite;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseRewriter(new RewriteOptions().AddRedirect("book/(.*)", "books/$1"));
app.Use(async (context, next) =>
{
    Console.WriteLine($"[{context.Request.Method} {context.Request.Path} {DateTime.UtcNow}] Started.");
    await next(context);
    Console.WriteLine($"[{context.Request.Method} {context.Request.Path} {DateTime.UtcNow}] Finished.");
});

var books = new List<Book>();
app.MapGet("/books", () => books);
app.MapGet("/books/{title}", (string title) =>
{
    var targetBook = books.FirstOrDefault(b => title == b.Title);
    return targetBook is null ? Results.NotFound() : Results.Ok(targetBook);
});
app.MapPost("/books", (Book book) =>
{
    books.Add(book);
    return Results.Created($"/books/{book.Title}", book);
});
app.MapPut("/books/{title}", (string title, Book updatedBook) =>
{
    var existingBookIndex = books.FindIndex(b => b.Title == title);
    books[existingBookIndex] = updatedBook;
    return Results.Ok(updatedBook);
});
app.MapDelete("/books/{title}", (string title) =>
{
    books.RemoveAll(b => title == b.Title);
    return Results.NoContent();
});
app.Run();

public record Book(string Title, string AuthorLastName);