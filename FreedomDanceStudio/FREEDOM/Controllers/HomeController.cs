using Microsoft.AspNetCore.Mvc;

namespace FREEDOM.Controllers;

public class HomeController: Controller
{
    public IActionResult Index()
    {
        return View();
    }
    
    public IActionResult Rules()
    {
        return View("/Views/Rules/Rules.cshtml");
    }

    public IActionResult Offer()
    {
        return View("/Views/Offer/Offer.cshtml");
    }
}