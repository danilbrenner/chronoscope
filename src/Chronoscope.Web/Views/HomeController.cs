using Chronoscope.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Chronoscope.Web.Controllers;

public sealed class HomeController(ILogger<HomeController> logger) : Controller
{
    [HttpGet("/")]
    public IActionResult Index()
    {
        logger.LogInformation("Rendering timeline home page");

        var viewModel = new HomeIndexViewModel(
            Title: "Timeline",
            Message: "Chronoscope shell is ready. Timeline, Map, and Setup pages will be connected in upcoming phases.");

        return View(viewModel);
    }
}
