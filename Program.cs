using Dapper;
using DapperIntro.Entities;
using Microsoft.Data.SqlClient;

namespace DapperIntro
{
    public class Program
    {
        private const string _connectionString = @"
                Data Source=(localdb)\MSSQLLocalDB;Integrated Security=True;Database=LibraryDB;TrustServerCertificate=True;";

        static void Main(string[] args)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            #region
            //Visitor visitor = new Visitor()
            //{
            //    Name = "Дмитро Корольов",
            //    PhoneNumber = "0000000000",
            //    BirthDate = new DateTime(2000, 12, 01)
            //};

            //VisitorPassport passport = new VisitorPassport()
            //{
            //    PassportNumber = "000000000"
            //};

            //AddVisitor(connection, visitor, passport);

            //var visitors = ReadVisitors(connection);

            //foreach (var visitor in visitors)
            //{
            //    Console.WriteLine($"{visitor.Name} -- {visitor.Passport.PassportNumber}");
            //}

            //Author author = new Author()
            //{
            //    Name = "Френк Герберт"
            //};

            //List<Book> books = new List<Book>()
            //{
            //    new Book()
            //    {
            //        Title = "Дюна"
            //    },
            //    new Book()
            //    {
            //        Title = "Мессія Дюни"
            //    }
            //};

            //AddAuthorWithBooks(connection, author, books);

            #endregion

            //var authors = ReadAuthorsWithBooks(connection);

            //foreach(var author in authors)
            //{
            //    Console.WriteLine($"{author.Name}");
            //    foreach (var book in author.Books)
            //        Console.WriteLine($"--{book.Title}");
            //}

            //AddLoan(connection, 1, 2);

            var loans = ReadLoans(connection);
            foreach(var loan in loans)
            {
                Console.WriteLine($"{loan.Book.Title} -- {loan.Visitor.Name} -- {loan.LoanDate}");
            }
        }

        public static List<BookLoan> ReadLoans(SqlConnection connection)
        {
            string query = @"SELECT bl.BookId, bl.VisitorId, bl.LoanDate, bl.ReturnDate,
                            b.Id, b.Title, b.AuthorId, v.Id, v.Name, v.PhoneNumber, v.BirthDate
                            FROM BookLoans bl
                            JOIN Books b ON bl.BookId = b.Id
                            JOIN Visitors v ON v.Id = bl.VisitorId";

            var loans = connection.Query<BookLoan, Book, Visitor, BookLoan>(
                    query,
                    (bookLoan, book, visitor) =>
                    {
                        bookLoan.Book = book;
                        bookLoan.Visitor = visitor;
                        return bookLoan;
                    },
                    splitOn: "Id, Id").ToList();

            return loans;
        }

        public static void AddLoan(SqlConnection connection, int visitorId, int bookId)
        {
            string query = @"INSERT INTO BookLoans (BookId, VisitorId, LoanDate, ReturnDate)
                VALUES (@BookId, @VisitorId, @LoanDate, @ReturnDate);";

            connection.Execute(
                query,
                new BookLoan()
                {
                    BookId = bookId,
                    VisitorId = visitorId,
                    LoanDate = DateTime.Now,
                    ReturnDate = null
                });
        }

        public static List<Author> ReadAuthorsWithBooks(SqlConnection connection)
        {
            string query = @"
                SELECT A.Id, A.Name, B.Id, B.Title, B.AuthorId
                FROM Authors A
                LEFT JOIN Books B ON A.Id = B.AuthorId
            ";

            var authorsDictionary = new Dictionary<long, Author>();

            var authors = connection.Query<Author, Book, Author>(
                    query,
                    (a, b) =>
                    {
                        if (!authorsDictionary.TryGetValue(a.Id, out var author))
                        {
                            author = a;
                            author.Books = new List<Book>();
                            authorsDictionary.Add(author.Id, author);
                        }

                        if (b != null)
                        {
                            author.Books.Add(b);
                        }

                        return author;
                    },
                    splitOn: "Id").Distinct().ToList();

            return authors;
        }

        public static void AddAuthorWithBooks(SqlConnection connection, Author author, List<Book> books)
        {
            using var transaction = connection.BeginTransaction();

            string insertAuthor = @"
                INSERT INTO Authors (Name)
                OUTPUT INSERTED.Id VALUES (@Name);
            ";

            string insertBook = @"
                INSERT INTO Books (Title, AuthorId)
                VALUES(@Title, @AuthorId)
            ";

            var authorId = connection.ExecuteScalar<long>(
                insertAuthor, author, transaction);

            foreach (var book in books)
            {
                book.AuthorId = authorId;
            }

            connection.Execute(
                    insertBook,
                    books,
                    transaction
                );

            transaction.Commit();
        }

        public static List<Visitor> ReadVisitors(SqlConnection connection)
        {
            string query = @"SELECT V.Id, V.Name, V.BirthDate, VP.Id, VP.PassportNumber, VP.VisitorId
                            FROM Visitors V
                            JOIN VisitorPassports VP ON V.Id = VP.VisitorId;";

            var visitors = connection.Query<Visitor, VisitorPassport, Visitor>(
                query,
                (visitor, passport) =>
                {
                    visitor.Passport = passport;
                    return visitor;
                },
                splitOn: "Id"
                ).ToList();

            return visitors;
        }

        public static void AddVisitor(SqlConnection connection, Visitor visitor, VisitorPassport passport)
        {
            using var transaction = connection.BeginTransaction();

            string insertVisitor = @"INSERT INTO Visitors (Name, PhoneNumber, BirthDate)
                                    OUTPUT INSERTED.Id VALUES (@Name, @PhoneNumber, @BirthDate);";

            string insertPassport = @"INSERT INTO VisitorPassports (PassportNumber, VisitorId)
                                      VALUES (@PassportNumber, @VisitorId)";

            long visitorId = connection.ExecuteScalar<long>(insertVisitor, visitor, transaction);
            passport.VisitorId = visitorId;
            connection.Execute(insertPassport, passport, transaction);
            transaction.Commit();
        }

        public static void UpdateBookAuthorById(SqlConnection connection, int id, string author)
        {
            string query = @"
                UPDATE Books
                SET Author = @Author
                WHERE Id = @Id;
            ";

            connection.Execute(query, new { Author = author, Id = id });
        }

        public static void DeleteBookById(SqlConnection connection, int id)
        {
            string query = @"
                DELETE FROM Books WHERE Id = @Id;
            ";
            connection.Execute(query, new { Id = id });
        }

        public static List<Book> ReadBooksFromDb(SqlConnection connection)
        {
            string query = "SELECT Id, Title, Author FROM Books;";

            return connection.Query<Book>(query).ToList();
        }

        public static void AddBookToDb(SqlConnection connection, Book book)
        {
            string query = @"
                INSERT INTO Books (Title, Author)
                VALUES (@Title, @Author);
            ";

            connection.Query(query, book);
        }

        public static void CreateBooksTable(SqlConnection connection)
        {
            string query = @"
                CREATE TABLE Books (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    Title NVARCHAR(MAX) NOT NULL,
                    Author NVARCHAR(MAX) NOT NULL
                );
            ";

            connection.Execute(query);
        }
    }
}
