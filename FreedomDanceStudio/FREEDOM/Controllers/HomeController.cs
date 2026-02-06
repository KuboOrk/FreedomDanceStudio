using Microsoft.AspNetCore.Mvc;

namespace FREEDOM.Controllers;

public class HomeController: Controller
{
    public IActionResult Index()
    {
        return View();
    }
}