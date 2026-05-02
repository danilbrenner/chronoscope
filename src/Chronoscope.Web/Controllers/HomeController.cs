using Chronoscope.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Chronoscope.Web.Controllers;

public sealed class HomeController : Controller
{
    [HttpGet("/")]
    public IActionResult Index()
    {
        var viewModel = new HomeIndexViewModel(
            Title: "Timeline",
            Message: "Chronoscope shell is ready. Timeline, Map, Sync Status, and Setup pages will be connected in upcoming phases.");

        return View(viewModel);
    }
}
