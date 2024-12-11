using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System;
using System.Security.Cryptography;
using System.Text;
using Library.Models;
using Helper;

namespace Library.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Логин и пароль не могут быть пустыми.";
                return View();
            }
            Console.WriteLine(HashPassword(password));

            var user = GetUserByLogin(login);
            if (user == null)
            {
                ViewBag.Error = "Неверный логин или пароль.";
                return View();
            }

            var hash = HashPassword(password);
            if (user.PasswordHash != hash)
            {
                ViewBag.Error = "Неверный логин или пароль.";
                return View();
            }

            HttpContext.Session.SetString("UserLogin", user.Login);
            HttpContext.Session.SetString("UserRole", user.Role);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        private User GetUserByLogin(string login)
        {
            User user = null;
            using (var con = new NpgsqlConnection(DbConfig.ConnectionString))
            {
                con.Open();
                using (var cmd = new NpgsqlCommand("SELECT login, password_hash, role FROM users WHERE login=@login", con))
                {
                    cmd.Parameters.AddWithValue("login", login);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new User
                            {
                                Login = reader.GetString(0),
                                PasswordHash = reader.GetString(1),
                                Role = reader.GetString(2)
                            };
                        }
                    }
                }
            }
            return user;
        }

        private string HashPassword(string password)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hashBytes = sha.ComputeHash(bytes);
                Console.WriteLine("XD");
                Console.WriteLine("Password " + Convert.ToBase64String(hashBytes));
                return Convert.ToBase64String(hashBytes);
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
