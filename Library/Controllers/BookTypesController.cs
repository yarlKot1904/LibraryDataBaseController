using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Library.Models;
using Helper;

namespace Library.Controllers
{
    public class BookTypesController : Controller
    {
        public ActionResult Index()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            var types = GetAllBookTypes();
            return View(types);
        }

        public ActionResult Details(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            var bt = GetBookTypeById(id);
            if (bt == null)
                return NotFound();
            return View(bt);
        }

        public ActionResult Create()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(BookType model)
        {

            if (ModelState.IsValid)
            {
                AddBookType(model);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            var bt = GetBookTypeById(id);
            if (bt == null)
                return NotFound();
            return View(bt);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(BookType model)
        {
            if (ModelState.IsValid)
            {
                UpdateBookType(model);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public ActionResult Delete(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            var bt = GetBookTypeById(id);
            if (bt == null)
                return NotFound();
            return View(bt);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            DeleteBookType(id);
            return RedirectToAction("Index");
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

        private BookType? GetBookTypeById(int id)
        {
            BookType? bt = null;
            using (var con = new NpgsqlConnection(DbConfig.ConnectionString))
            {
                con.Open();
                using (var cmd = new NpgsqlCommand("SELECT id, type, fine, max_days FROM book_types WHERE id=@id", con))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bt = new BookType
                            {
                                Id = reader.GetInt32(0),
                                Type = reader.GetString(1),
                                Fine = reader.GetInt32(2),
                                MaxDays = reader.GetInt32(3)
                            };
                        }
                    }
                }
            }
            return bt;
        }

        private void AddBookType(BookType bt)
        {
            using (var con = new NpgsqlConnection(DbConfig.ConnectionString))
            {
                con.Open();
                using (var cmd = new NpgsqlCommand("INSERT INTO book_types(type, fine, max_days) VALUES(@t, @f, @md)", con))
                {
                    cmd.Parameters.AddWithValue("t", (object?)bt.Type ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("f", (object?)bt.Fine ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("md", (object?)bt.MaxDays ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void UpdateBookType(BookType bt)
        {
            using (var con = new NpgsqlConnection(DbConfig.ConnectionString))
            {
                con.Open();
                using (var cmd = new NpgsqlCommand(
                    "UPDATE book_types SET type=@t, fine=@f, max_days=@md WHERE id=@id", con))
                {
                    cmd.Parameters.AddWithValue("t", (object?)bt.Type ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("f", (object?)bt.Fine ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("md", (object?)bt.MaxDays ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("id", bt.Id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void DeleteBookType(int id)
        {
            using (var con = new NpgsqlConnection(DbConfig.ConnectionString))
            {
                con.Open();
                using (var cmd = new NpgsqlCommand("DELETE FROM book_types WHERE id=@id", con))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.ExecuteNonQuery();
                }
            }
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
