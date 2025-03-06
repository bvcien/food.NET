using System.Data;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

using NETCORE.Models;

namespace NETCORE.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
  
    public HomeController(ILogger<HomeController> logger )
    {
        
        _logger = logger;
    }

    public IActionResult Index()
    {
       
    return View();  // Trả dữ liệu vào View
    }

    public IActionResult Privacy()
    {
        return View();
    }
    public IActionResult Tintuc()
    {
        return View();
    }
       public IActionResult Ngucoc()
    {
        return View();
    }
   public IActionResult Thucphamsong()
    {
        return View();
    }


 public IActionResult Lienhe()
    {
        return View();
    }
    public IActionResult Raucu()
    {
        return View();
    }
 
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

internal class DataContext
{
}