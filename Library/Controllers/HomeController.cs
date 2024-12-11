using Helper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace Library.Controllers
{
    public class HomeController : Controller
    {
        ReportsService _reportsService = ReportsService.Instance;
        
        public IActionResult Index()
        {
            if(!IsLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }
            ViewBag.MaxFine = GetMaxFine();
            var books = _reportsService.GetBooks(DateTime.MinValue, DateTime.MaxValue);
            ViewBag.FirstBook = books.Count > 0 ? books[0] : "No data";
            ViewBag.SecondBook = books.Count > 1 ? books[1] : "No data";
            ViewBag.ThirdBook = books.Count > 2 ? books[2] : "No data";
            return View();
        }

        private decimal GetMaxFine()
        {
            decimal fine = 0;
            using (var con = new NpgsqlConnection(DbConfig.ConnectionString))
            {
                con.Open();
                using (var cmd = new NpgsqlCommand("CALL find_max_fine(@fine);", con))
                {
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
