using Chronoscope.Application.Abstractions.SourceAuth;
using Chronoscope.Web.Extensions;
using Chronoscope.Web.ViewModels.Setup;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Chronoscope.Web.Controllers;

public sealed class SetupController(
    ISourceAuthProvider sourceAuthProvider,
    ILogger<SetupController> logger) : Controller
{
    [HttpGet("/setup")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        logger.LogInformation("Rendering setup page. IsHtmxRequest: {IsHtmxRequest}", Request.IsHtmx());
        var authStep = await CreateAuthStepAsync(cancellationToken);
        if (Request.IsHtmx())
        {
            logger.LogInformation("Returning setup auth step partial view for HTMX request");
            return PartialView("_AuthStep", authStep);
        }

        var viewModel = new SetupIndexViewModel(
            Title: "Setup",
            Message: "Connect your OneDrive Source to start setup. Folder selection is the next step.",
            AuthStep: authStep);

        logger.LogInformation("Returning setup page");
        return View(viewModel);
    }

    private async Task<SetupAuthStepViewModel> CreateAuthStepAsync(CancellationToken cancellationToken)
    {
        try
        {
            var sourceSession = await sourceAuthProvider.GetCurrentAsync(cancellationToken);
            logger.LogInformation("OneDrive source is connected");
            return new SetupAuthStepViewModel(
                Status: "Connected",
                Title: "OneDrive sign-in",
                Message: "Your OneDrive Source is linked. You can remove the link and connect a different account.",
                IsConnected: true,
                DisplayName: sourceSession.DisplayName,
                EmailAddress: sourceSession.EmailAddress,
                DriveId: sourceSession.DriveId);
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "OneDrive source is not connected due to HTTP request error");
            return NotConnectedStep();
        }
        catch (InvalidOperationException exception)
        {
            logger.LogWarning(exception, "OneDrive source is not connected");
            return NotConnectedStep();
        }
    }

    private static SetupAuthStepViewModel NotConnectedStep()
    {
        return new SetupAuthStepViewModel(
            Status: "Not connected",
            Title: "OneDrive sign-in",
            Message: "No OneDrive Source is linked. Create a new link to continue setup.",
            IsConnected: false,
            DisplayName: null,
            EmailAddress: null,
            DriveId: null);
    }
}
