using Chronoscope.Application.Abstractions.SourceAuth;
using Chronoscope.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Chronoscope.Web.Controllers;

public sealed class LinkSourceController(
    ISourceAuthProvider sourceAuthProvider,
    ILogger<LinkSourceController> logger) : Controller
{
    private string CallbackUri
    {
        get
        {
            if (!Request.Host.HasValue)
            {
                logger.LogError("Cannot build OneDrive callback URI because request host is not available");
                throw new InvalidOperationException(
                    "Cannot build OneDrive callback URI because request host is not available.");
            }

            return $"{Request.Scheme}://{Request.Host}{Request.PathBase}/link/onedrive/complete";
        }
    }

    [HttpGet("/link/onedrive/start")]
    public IActionResult SignIn(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting OneDrive link flow");
        var linkUri = sourceAuthProvider.CreateLinkUriAsync(CallbackUri, cancellationToken);
        logger.LogInformation("Redirecting to OneDrive sign-in");
        return Redirect(linkUri);
    }

    [HttpGet("/link/onedrive/complete")]
    public async Task<IActionResult> Complete(
        [FromQuery(Name = "code")] string? authorizationCode,
        [FromQuery(Name = "state")] string? state,
        [FromQuery(Name = "error")] string? error,
        [FromQuery(Name = "error_description")]
        string? errorDescription,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(error))
        {
            logger.LogWarning(
                "OneDrive callback returned an error. Error: {Error}, HasErrorDescription: {HasErrorDescription}",
                error,
                !string.IsNullOrWhiteSpace(errorDescription));
            return RedirectToAction(nameof(SetupController.Index), "Setup", new { message = errorDescription });
        }

        if (string.IsNullOrWhiteSpace(authorizationCode) || string.IsNullOrWhiteSpace(state))
        {
            logger.LogWarning(
                "OneDrive callback is missing required query parameters. HasAuthorizationCode: {HasAuthorizationCode}, HasState: {HasState}",
                !string.IsNullOrWhiteSpace(authorizationCode),
                !string.IsNullOrWhiteSpace(state));
            return BadRequest("OneDrive callback is missing required query parameters.");
        }

        logger.LogInformation("Completing OneDrive link flow");
        await sourceAuthProvider.CompleteLinkAsync(authorizationCode, state, CallbackUri, cancellationToken);
        logger.LogInformation("Completed OneDrive link flow");

        return RedirectToAction(nameof(SetupController.Index), "Setup");
    }

    [AcceptVerbs("DELETE", "POST", Route = "/link/onedrive")]
    public async Task<IActionResult> Unlink(CancellationToken cancellationToken)
    {
        logger.LogInformation("Unlinking OneDrive source");
        await sourceAuthProvider.UnlinkCurrentAsync(cancellationToken);
        logger.LogInformation("Unlinked OneDrive source");

        if (Request.IsHtmx())
        {
            Response.Headers["HX-Redirect"] = "/setup";
            return Ok();
        }

        return RedirectToAction(nameof(SetupController.Index), "Setup");
    }
}
