using Microsoft.AspNetCore.Rewrite;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IBookService, BookService>();
var app = builder.Build();

app.UseRewriter(new RewriteOptions().AddRewrite("book/(.*)", "books/$1", skipRemainingRules: true));
app.Use(async (context, next) =>
{
    Console.WriteLine($"[{context.Request.Method} {context.Request.Path} {DateTime.UtcNow}] Started.");
    await next(context);
    Console.WriteLine($"[{context.Request.Method} {context.Request.Path} {DateTime.UtcNow}] Finished.");
});

var books = new List<Book>();
app.MapGet("/books", (IBookService service) => service.GetBooks());
app.MapGet("/books/{title}", (string title, IBookService service) =>
{
    var targetBook = service.GetBook(title);
    return targetBook is null ? Results.NotFound() : Results.Ok(targetBook);
});
app.MapPost("/books", (Book book, IBookService service) =>
{
    service.AddBook(book);
    return Results.Created($"/books/{book.Title}", book);
})
.AddEndpointFilter(async (context, next) =>
{
    var bookArgument= context.GetArgument<Book>(0);
    var errors=new Dictionary<string,string[]>();
    if(bookArgument.Title.Length<5)
    {
        errors.Add(nameof(Book.Title),["Title must be at least 5 characters long."]);
    }
    if(errors.Count>0){
        return Results.ValidationProblem(errors);
    }
    return await next(context);
});
app.MapPut("/books/{title}", (string title, Book updatedBook, IBookService service) =>
{
    service.UpdateBook(title, updatedBook);
    return Results.Ok(updatedBook);
});
app.MapDelete("/books/{title}", (string title, IBookService service) =>
{
    service.DeleteBook(title);
    return Results.NoContent();
});
app.Run();

public record Book(string Title, string AuthorLastName);

interface IBookService
{
    List<Book> GetBooks();
    Book? GetBook(string title);
    Book AddBook(Book book);
    void UpdateBook(string title, Book updatedBook);
    void DeleteBook(string title);
}

public class BookService : IBookService
{
    private readonly List<Book> _books = [];
    public List<Book> GetBooks() => _books;
    public Book? GetBook(string title) {
        return _books.SingleOrDefault(b => title == b.Title);
    }
    public Book AddBook(Book book) {
        _books.Add(book);
        return book;
    }
    public void UpdateBook(string title, Book updatedBook)
    {
        var existingBookIndex = _books.FindIndex(b => b.Title == title);
        _books[existingBookIndex] = updatedBook;
    }
    public void DeleteBook(string title) {
        _books.RemoveAll(b => title == b.Title);
    }
}