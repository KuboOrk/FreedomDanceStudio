using Microsoft.AspNetCore.Mvc;

namespace FreedomDanceStudio.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}