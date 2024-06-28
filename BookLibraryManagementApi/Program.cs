var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var books = new List<Book>();
app.MapGet("/books", () => books);
app.MapGet("/books/{id}", (int id) =>
{
    var targetBook = books.FirstOrDefault(b => id == b.BookID);
    return targetBook is null ? Results.NotFound() : Results.Ok(targetBook);
});
app.MapPost("/books", (Book book) =>
{
    books.Add(book);
    return Results.Created($"/books/{book.BookID}", book);
});
app.MapPut("/books/{id}", (int id, Book updatedBook) =>
{
    var existingBookIndex = books.FindIndex(b => b.BookID == id);
    books[existingBookIndex] = updatedBook;
    return Results.Ok(updatedBook);
});
app.MapDelete("/books/{id}", (int id) =>
{
    books.RemoveAll(b => id == b.BookID);
    return Results.NoContent();
});
app.Run();

public record Book(string Title, string AuthorLastName, int BookID);