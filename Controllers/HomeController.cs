namespace DigitalLibrarySystem.Controllers;



using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("UserID") == null)
        {
            return RedirectToAction("Login", "Account");
        }
        return View();
    }

    public IActionResult Error()
    {
        return View();
    }
}