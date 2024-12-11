using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Library.Models;
using Helper;

namespace Library.Controllers
{
    public class ViewsController : Controller
    {
        public ActionResult ClientCount()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            var data = GetClientCountData();
            return View(data);
        }

        public ActionResult BooksAndReaders()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            var data = GetBooksAndReadersData();
            return View(data);
        }

        private List<ClientCountRecord> GetClientCountData()
        {
            var result = new List<ClientCountRecord>();
            using (var con = new NpgsqlConnection(DbConfig.ConnectionString))
            {
                con.Open();
                using (var cmd = new NpgsqlCommand("SELECT first_name, last_name, books_on_hand FROM clients_count", con))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new ClientCountRecord
                        {
                            FirstName = reader.GetString(0),
                            LastName = reader.GetString(1),
                            BooksOnHand = reader.GetInt32(2)
                        });
                    }
                }
            }
            return result;
        }

        private List<BooksAndReadersRecord> GetBooksAndReadersData()
        {
            var result = new List<BooksAndReadersRecord>();
            using (var con = new NpgsqlConnection(DbConfig.ConnectionString))
            {
                con.Open();
                using (var cmd = new NpgsqlCommand(@"
                    SELECT book_name, first_name, last_name, middle_name, data_beg, date_end, date_ret
                    FROM books_and_readers
                    ORDER BY data_beg", con))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new BooksAndReadersRecord
                        {
                            BookName = reader.GetString(0),
                            FirstName = reader.GetString(1),
                            LastName = reader.GetString(2),
                            MiddleName = reader.IsDBNull(3) ? null : reader.GetString(3),
                            DataBeg = reader.GetDateTime(4),
                            DateEnd = reader.GetDateTime(5),
                            DateRet = reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6)
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
    }
}
