using Chronoscope.Application.Abstractions.SourceAuth;
using Chronoscope.Web.ViewModels.SourceAuth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chronoscope.Web.Controllers;

[AllowAnonymous]
public sealed class SourceAuthController(ISourceAuthService sourceAuthService) : Controller
{
    [HttpGet("/auth/onedrive/sign-in")]
    public IActionResult SignIn()
    {
        return Challenge(new AuthenticationProperties
        {
            RedirectUri = "/auth/onedrive/complete"
        });
    }

    [HttpGet("/auth/onedrive/complete")]
    public async Task<IActionResult> Complete(CancellationToken cancellationToken)
    {
        var session = await sourceAuthService.GetCurrentAsync(cancellationToken);
        var viewModel = new SourceAuthCompleteViewModel(
            Title: "OneDrive connected",
            Message: "Microsoft sign-in succeeded and Chronoscope can call Microsoft Graph on your behalf.",
            DisplayName: session.DisplayName,
            EmailAddress: session.EmailAddress,
            DriveId: session.DriveId);

        return View(viewModel);
    }
}
