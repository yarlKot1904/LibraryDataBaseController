using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Library.Models;
using Helper;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Library.Controllers
{
    public class JournalController : Controller
    {
        public ActionResult Index()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            var records = GetAllJournalRecords();
            return View(records);
        }

        public ActionResult Details(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            var record = GetJournalById(id);
            if (record == null)
                return NotFound();
            return View(record);
        }

        public ActionResult Create()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            ViewBag.Books = new SelectList(GetAllBooks(), "Id", "Name");
            ViewBag.Clients = new SelectList(GetAllClients(), "Id", "LastName");
            ViewBag.DateBeg = DateTime.Now;
            ViewBag.DateEnd = DateTime.Now.AddDays(30);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Journal model)
        {

            if (ModelState.IsValid)
            {
                AddJournalRecord(model);
                return RedirectToAction("Index");
            }

            ViewBag.Books = new SelectList(GetAllBooks(), "Id", "Name", model.BookId);
            ViewBag.Clients = new SelectList(GetAllClients(), "Id", "LastName", model.ClientId);
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            var record = GetJournalById(id);
            if (record == null)
                return NotFound();

            ViewBag.Books = new SelectList(GetAllBooks(), "Id", "Name", record.BookId);
            ViewBag.Clients = new SelectList(GetAllClients(), "Id", "LastName", record.ClientId);
            return View(record);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Journal model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    UpdateJournalRecord(model);
                }
                catch (PostgresException e)
                {
                    Console.WriteLine(e.SqlState);
                    if (e.SqlState == "P0001")
                    {
                        TempData["ErrorMessage"] = "Cannot set date before begin date!";
                        return RedirectToAction("Index");
                    }
                    return RedirectToAction("Index");
                }
                Console.WriteLine("model bad" + model.ToString());
                return RedirectToAction("Index");
            }


            ViewBag.Books = new SelectList(GetAllBooks(), "Id", "Name", model.BookId);
            ViewBag.Clients = new SelectList(GetAllClients(), "Id", "LastName", model.ClientId);
            return View(model);
        }


        public ActionResult Delete(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            var record = GetJournalById(id);
            if (record == null)
                return NotFound();
            return View(record);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                DeleteJournalRecord(id);
            }
            catch (PostgresException e)
            {
                Console.WriteLine(e.SqlState);
                if (e.SqlState == "P0001")
                {
                    TempData["ErrorMessage"] = "Cannot delete journal which havent returned!";
                    return RedirectToAction("Index");
                }
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }
        private List<Journal> GetAllJournalRecords()
        {
            var result = new List<Journal>();
            using (var con = new NpgsqlConnection(DbConfig.ConnectionString))
            {
                con.Open();
                using (var cmd = new NpgsqlCommand(@"
                    SELECT j.id, j.book_id, b.name AS book_name,
                           j.client_id, c.last_name, c.first_name, c.middle_name,
                           j.data_beg, j.date_end, j.date_ret
                    FROM journal j
                    LEFT JOIN books b ON j.book_id = b.id
                    LEFT JOIN clients c ON j.client_id = c.id
                    ORDER BY j.data_beg;", con))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new Journal
                        {
                            Id = reader.GetInt32(0),
                            BookId = reader.GetInt32(1),
                            BookName = reader.IsDBNull(2) ? null : reader.GetString(2),
                            ClientId = reader.GetInt32(3),
                            ClientLastName = reader.IsDBNull(4) ? null : reader.GetString(4),
                            ClientFirstName = reader.IsDBNull(5) ? null : reader.GetString(5),
                            ClientMiddleName = reader.IsDBNull(6) ? null : reader.GetString(6),
                            DateBeg = reader.GetDateTime(7),
                            DateEnd = reader.GetDateTime(8),
                            DateRet = reader.IsDBNull(9) ? (DateTime?)null : reader.GetDateTime(9)
                        });
                    }
                }
            }
            return result;
        }

        private Journal GetJournalById(int id)
        {
            Journal record = null;
            using (var con = new NpgsqlConnection(DbConfig.ConnectionString))
            {
                con.Open();
                using (var cmd = new NpgsqlCommand(@"
                    SELECT j.id, j.book_id, b.name AS book_name,
                           j.client_id, c.last_name, c.first_name, c.middle_name,
                           j.data_beg, j.date_end, j.date_ret
                    FROM journal j
                    LEFT JOIN books b ON j.book_id = b.id
                    LEFT JOIN clients c ON j.client_id = c.id
                    WHERE j.id=@id;", con))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            record = new Journal
                            {
                                Id = reader.GetInt32(0),
                                BookId = reader.GetInt32(1),
                                BookName = reader.IsDBNull(2) ? null : reader.GetString(2),
                                ClientId = reader.GetInt32(3),
                                ClientLastName = reader.IsDBNull(4) ? null : reader.GetString(4),
                                ClientFirstName = reader.IsDBNull(5) ? null : reader.GetString(5),
                                ClientMiddleName = reader.IsDBNull(6) ? null : reader.GetString(6),
                                DateBeg = reader.GetDateTime(7),
                                DateEnd = reader.GetDateTime(8),
                                DateRet = reader.IsDBNull(9) ? (DateTime?)null : reader.GetDateTime(9)
                            };
                        }
                    }
                }
            }
            return record;
        }

        private void AddJournalRecord(Journal record)
        {
            using (var con = new NpgsqlConnection(DbConfig.ConnectionString))
            {
                con.Open();
                using (var cmd = new NpgsqlCommand(
                    "INSERT INTO journal(book_id, client_id, data_beg, date_end, date_ret) VALUES(@b, @c, @db, @de, @dr)", con))
                {
                    cmd.Parameters.AddWithValue("b", record.BookId);
                    cmd.Parameters.AddWithValue("c", record.ClientId);
                    cmd.Parameters.AddWithValue("db", record.DateBeg);
                    cmd.Parameters.AddWithValue("de", record.DateEnd);
                    cmd.Parameters.AddWithValue("dr", (object)record.DateRet ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void UpdateJournalRecord(Journal record)
        {
            using (var con = new NpgsqlConnection(DbConfig.ConnectionString))
            {
                con.Open();
                using (var cmd = new NpgsqlCommand(
                    "UPDATE journal SET book_id=@b, client_id=@c, data_beg=@db, date_end=@de, date_ret=@dr WHERE id=@id", con))
                {
                    cmd.Parameters.AddWithValue("b", record.BookId);
                    cmd.Parameters.AddWithValue("c", record.ClientId);
                    cmd.Parameters.AddWithValue("db", record.DateBeg);
                    cmd.Parameters.AddWithValue("de", record.DateEnd);
                    cmd.Parameters.AddWithValue("dr", (object)record.DateRet ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("id", record.Id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void DeleteJournalRecord(int id)
        {
            using (var con = new NpgsqlConnection(DbConfig.ConnectionString))
            {
                con.Open();
                using (var cmd = new NpgsqlCommand("DELETE FROM journal WHERE id=@id", con))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private List<Book> GetAllBooks()
        {
            var result = new List<Book>();
            using (var con = new NpgsqlConnection(DbConfig.ConnectionString))
            {
                con.Open();
                using (var cmd = new NpgsqlCommand("SELECT id, name, amount, type_id FROM books ORDER BY name", con))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new Book
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Amount = reader.GetInt32(2),
                            TypeId = reader.GetInt32(3)
                        });
                    }
                }
            }
            return result;
        }

        private List<Client> GetAllClients()
        {
            var result = new List<Client>();
            using (var con = new NpgsqlConnection(DbConfig.ConnectionString))
            {
                con.Open();
                using (var cmd = new NpgsqlCommand("SELECT id, last_name, first_name, middle_name, passport_series, passport_number FROM clients ORDER BY last_name, first_name", con))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new Client
                        {
                            Id = reader.GetInt32(0),
                            LastName = reader.GetString(1),
                            FirstName = reader.GetString(2),
                            MiddleName = reader.IsDBNull(3) ? null : reader.GetString(3),
                            PassportSeries = reader.IsDBNull(4) ? null : reader.GetString(4),
                            PassportNumber = reader.IsDBNull(5) ? null : reader.GetString(5)
                        });
                    }
                }
            }
            return result;
        }

        public ActionResult GiveBook()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            ViewBag.Books = new SelectList(GetAllBooks(), "Id", "Name");
            ViewBag.Clients = new SelectList(GetAllClients(), "Id", "LastName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GiveBook(Journal model)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            if (ModelState.IsValid)
            {
                AddJournalRecord(model);
                return RedirectToAction("Index");
            }

            ViewBag.Books = new SelectList(GetAllBooks(), "Id", "Name", model.BookId);
            ViewBag.Clients = new SelectList(GetAllClients(), "Id", "LastName", model.ClientId);
            return View(model);
        }

        public ActionResult ReturnBook(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

            var record = GetJournalById(id);
            if (record == null) return NotFound();
            if (record.DateRet.HasValue)
            {
                TempData["ErrorMessage"] = "Книга уже возвращена.";
                return RedirectToAction("Index");
            }

            using (var con = new NpgsqlConnection(DbConfig.ConnectionString))
            {
                con.Open();
                using (var cmd = new NpgsqlCommand("UPDATE journal SET date_ret=@dr WHERE id=@id", con))
                {
                    cmd.Parameters.AddWithValue("dr", DateTime.Now);
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index");
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
