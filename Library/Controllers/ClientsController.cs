using System;

using Library.Models;
using Npgsql;
using Helper;
using Microsoft.AspNetCore.Mvc;

namespace Library.Controllers
{
    public class ClientsController : Controller
    {
        public ActionResult Index()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            var clients = GetAllClients();
            return View(clients);
        }

        public ActionResult Details(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            var client = GetClientById(id);
            if (client == null)
                return NotFound();

            int booksOnHand = GetBooksOnHandCount(id);
            ViewBag.BooksOnHand = booksOnHand;
            ViewBag.Fine = GetFine(id);
            return View(client);
        }

        public ActionResult Create()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Client model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    AddClient(model);
                }
                catch (PostgresException e) { Console.WriteLine(e.SqlState); if (e.SqlState == "P0001") { TempData["ErrorMessage"] = "This passport is already used!"; return RedirectToAction("Index"); } return RedirectToAction("Index"); }
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            var client = GetClientById(id);
            if (client == null)
                return NotFound();
            return View(client);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Client model)
        {
            if (ModelState.IsValid)
            {
                UpdateClient(model);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public ActionResult Delete(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            var client = GetClientById(id);
            if (client == null)
                return NotFound();
            return View(client);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            DeleteClient(id);
            return RedirectToAction("Index");
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
                            PassportSeries = reader.GetString(4),
                            PassportNumber = reader.GetString(5)
                        });
                    }
                }
            }
            return result;
        }

        private Client? GetClientById(int id)
        {
            Client client = null;
            using (var con = new NpgsqlConnection(DbConfig.ConnectionString))
            {
                con.Open();
                using (var cmd = new NpgsqlCommand("SELECT id, last_name, first_name, middle_name, passport_series, passport_number FROM clients WHERE id=@id", con))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    Console.WriteLine(cmd.CommandText);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            client = new Client
                            {
                                Id = reader.GetInt32(0),
                                LastName = reader.GetString(1),
                                FirstName = reader.GetString(2),
                                MiddleName = reader.IsDBNull(3) ? null : reader.GetString(3),
                                PassportSeries = reader.IsDBNull(4) ? null : reader.GetString(4),
                                PassportNumber = reader.IsDBNull(5) ? null : reader.GetString(5)
                            };
                        }
                    }
                }
            }
            return client;
        }

        private void AddClient(Client client)
        {
            using (var con = new NpgsqlConnection(DbConfig.ConnectionString))
            {
                con.Open();
                using (var cmd = new NpgsqlCommand(
                    "INSERT INTO clients(last_name, first_name, middle_name, passport_series, passport_number) VALUES(@ln, @fn, @mn, @ps, @pn)", con))
                {
                    cmd.Parameters.AddWithValue("ln", client.LastName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("fn", client.FirstName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("mn", (object)client.MiddleName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("ps", (object)client.PassportSeries ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("pn", (object)client.PassportNumber ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void UpdateClient(Client client)
        {
            using (var con = new NpgsqlConnection(DbConfig.ConnectionString))
            {
                con.Open();
                using (var cmd = new NpgsqlCommand(
                    "UPDATE clients SET last_name=@ln, first_name=@fn, middle_name=@mn, passport_series=@ps, passport_number=@pn WHERE id=@id", con))
                {
                    cmd.Parameters.AddWithValue("ln", (object)client.LastName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("fn", (object)client.FirstName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("mn", (object)client.MiddleName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("ps", (object)client.PassportSeries ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("pn", (object)client.PassportNumber ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("id", client.Id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void DeleteClient(int id)
        {
            using (var con = new NpgsqlConnection(DbConfig.ConnectionString))
            {
                con.Open();
                using (var cmd = new NpgsqlCommand("DELETE FROM clients WHERE id=@id", con))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private int GetBooksOnHandCount(int clientId)
        {
            int booksOnHand = 0;
            using (var con = new Npgsql.NpgsqlConnection(DbConfig.ConnectionString))
            {
                con.Open();
                using (var cmd = new Npgsql.NpgsqlCommand("CALL books_on_hand(@clientId, @books_count);", con))
                {
                    cmd.Parameters.AddWithValue("clientId", clientId);
                    var outParam = cmd.Parameters.AddWithValue("books_count", DBNull.Value);
                    outParam.Direction = System.Data.ParameterDirection.InputOutput;
                    cmd.ExecuteNonQuery();
                    booksOnHand = (int)outParam.Value;
                }
            }
            return booksOnHand;
        }

        private decimal GetFine(int clientId)
        {
            decimal fine = 0;
            using (var con = new NpgsqlConnection(DbConfig.ConnectionString))
            {
                con.Open();
                using (var cmd = new NpgsqlCommand("CALL calculate_client_fine(@clientId, @fine);", con))
                {
                    cmd.Parameters.AddWithValue("clientId", clientId);
                    var outParam = cmd.Parameters.AddWithValue("fine", DBNull.Value);
                    outParam.Direction = System.Data.ParameterDirection.InputOutput;
                    cmd.ExecuteNonQuery();
                    fine = (decimal)outParam.Value;
                }
            }
            return fine;
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
