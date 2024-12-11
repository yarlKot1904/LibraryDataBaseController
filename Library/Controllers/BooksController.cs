using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Library.Models;
using Helper;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Library.Controllers
{
    public class BooksController : Controller
    {
        public ActionResult Index()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            var books = GetAllBooks();
            return View(books);
        }

        public ActionResult Details(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            var book = GetBookById(id);
            if (book == null)
                return NotFound();
            return View(book);
        }

        public ActionResult Create()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            ViewBag.Types = new SelectList(GetAllBookTypes(), "Id", "Type");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Book model)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            if (ModelState.IsValid)
            {
                AddBook(model);
                return RedirectToAction("Index");
            }
            ViewBag.Types = new SelectList(GetAllBookTypes(), "Id", "Type", model.TypeId);
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            var book = GetBookById(id);
            if (book == null)
                return NotFound();
            ViewBag.Types = new SelectList(GetAllBookTypes(), "Id", "Type", book.TypeId);
            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Book model)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            if (ModelState.IsValid)
            {
                UpdateBook(model);
                return RedirectToAction("Index");
            }
            ViewBag.Types = new SelectList(GetAllBookTypes(), "Id", "Type", model.TypeId);
            return View(model);
        }

        public ActionResult Delete(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            var book = GetBookById(id);
            if (book == null)
                return NotFound();
            return View(book);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                DeleteBook(id);
            }
            catch(PostgresException e)
            {
                Console.WriteLine(e.SqlState);
                if (e.SqlState == "P0001")
                {
                    TempData["ErrorMessage"] = "Cannot delete books which havent returned!";
                    return RedirectToAction("Index");
                }
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }


        private List<Book> GetAllBooks()
        {
            var result = new List<Book>();
            using (var con = new NpgsqlConnection(DbConfig.ConnectionString))
            {
                con.Open();
                using (var cmd = new NpgsqlCommand(@"
                    SELECT b.id, b.name, b.amount, b.type_id, bt.type, bt.fine, bt.max_days
                    FROM books b
                    LEFT JOIN book_types bt ON b.type_id = bt.id
                    ORDER BY b.name;", con))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new Book
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Amount = reader.GetInt32(2),
                            TypeId = reader.GetInt32(3),
                            Type = new BookType
                            {
                                Id = reader.GetInt32(3),
                                Type = reader.GetString(4),
                                Fine = reader.GetInt32(5),
                                MaxDays = reader.GetInt32(6)
                            }
                        });
                    }
                }
            }
            return result;
        }

        private Book GetBookById(int id)
        {
            Book book = null;
            using (var con = new NpgsqlConnection(DbConfig.ConnectionString))
            {
                con.Open();
                using (var cmd = new NpgsqlCommand(@"
                    SELECT b.id, b.name, b.amount, b.type_id, bt.type, bt.fine, bt.max_days
                    FROM books b
                    LEFT JOIN book_types bt ON b.type_id = bt.id
                    WHERE b.id=@id;", con))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            book = new Book
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Amount = reader.GetInt32(2),
                                TypeId = reader.GetInt32(3),
                                Type = new BookType
                                {
                                    Id = reader.GetInt32(3),
                                    Type = reader.GetString(4),
                                    Fine = reader.GetInt32(5),
                                    MaxDays = reader.GetInt32(6)
                                }
                            };
                        }
                    }
                }
            }
            return book;
        }

        private void AddBook(Book book)
        {
            using (var con = new NpgsqlConnection(DbConfig.ConnectionString))
            {
                con.Open();
                using (var cmd = new NpgsqlCommand(
                    "INSERT INTO books(name, amount, type_id) VALUES(@n, @a, @t)", con))
                {
                    cmd.Parameters.AddWithValue("n", book.Name);
                    cmd.Parameters.AddWithValue("a", book.Amount);
                    cmd.Parameters.AddWithValue("t", book.TypeId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void UpdateBook(Book book)
        {
            using (var con = new NpgsqlConnection(DbConfig.ConnectionString))
            {
                con.Open();
                using (var cmd = new NpgsqlCommand(
                    "UPDATE books SET name=@n, amount=@a, type_id=@t WHERE id=@id", con))
                {
                    cmd.Parameters.AddWithValue("n", book.Name);
                    cmd.Parameters.AddWithValue("a", book.Amount);
                    cmd.Parameters.AddWithValue("t", book.TypeId);
                    cmd.Parameters.AddWithValue("id", book.Id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void DeleteBook(int id)
        {
            using (var con = new NpgsqlConnection(DbConfig.ConnectionString))
            {
                con.Open();
                using (var cmd = new NpgsqlCommand("DELETE FROM books WHERE id=@id", con))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private List<BookType> GetAllBookTypes()
        {
            var result = new List<BookType>();
            using (var con = new NpgsqlConnection(DbConfig.ConnectionString))
            {
                con.Open();
                using (var cmd = new NpgsqlCommand("SELECT id, type, fine, max_days FROM book_types ORDER BY type", con))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new BookType
                        {
                            Id = reader.GetInt32(0),
                            Type = reader.GetString(1),
                            Fine = reader.GetInt32(2),
                            MaxDays = reader.GetInt32(3)
                        });
                    }
                }
            }
            return result;
        }
        private bool IsLoggedIn()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("UserLogin"));
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == "admin";
        }

    }

}
