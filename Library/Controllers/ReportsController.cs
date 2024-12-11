using Helper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;

public class ReportsController : Controller
{
    private readonly ReportsService _reportsService;

    public ReportsController()
    {
        _reportsService = ReportsService.Instance;
    }

    public IActionResult Index()
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
        return View();
    }

    [HttpGet]
    public IActionResult TopThreeBooksTxt(DateTime? start_date, DateTime? end_date)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

        if (!start_date.HasValue || !end_date.HasValue)
        {
            TempData["ErrorMessage"] = "Необходимо указать даты начала и конца периода.";
            return RedirectToAction("Index");
        }

        var report = _reportsService.GetTopThreeBooksReport(start_date.Value, end_date.Value);
        var bytes = Encoding.UTF8.GetBytes(report);
        return File(bytes, "text/plain", "TopThreeBooks.txt");
    }

    [HttpGet]
    public IActionResult CalculateFinesTxt(DateTime? start_date, DateTime? end_date)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

        if (!start_date.HasValue || !end_date.HasValue)
        {
            TempData["ErrorMessage"] = "Необходимо указать даты начала и конца периода.";
            return RedirectToAction("Index");
        }

        var report = _reportsService.GetCalculateFinesReport(start_date.Value, end_date.Value);
        var bytes = Encoding.UTF8.GetBytes(report);
        return File(bytes, "text/plain", "CalculateFines.txt");
    }

    private bool IsLoggedIn()
    {
        return !string.IsNullOrEmpty(HttpContext.Session.GetString("UserLogin"));
    }
}
