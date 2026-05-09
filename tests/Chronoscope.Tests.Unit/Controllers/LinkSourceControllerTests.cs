using Chronoscope.Application.Abstractions.SourceAuth;
using Chronoscope.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Chronoscope.Tests.Unit.Controllers;

public sealed class LinkSourceControllerTests
{
    [Fact]
    public Task SignIn_Invoked_RedirectsToMicrosoftAuthorizationEndpoint()
    {
        try
        {
            var sourceAuthProvider = new Mock<ISourceAuthProvider>(MockBehavior.Strict);
            sourceAuthProvider
                .Setup(provider => provider.CreateLinkUriAsync(
                    "https://chronoscope.local/link/onedrive/complete",
                    It.IsAny<CancellationToken>()))
                .Returns("https://login.microsoftonline.com/common/oauth2/v2.0/authorize?foo=bar");

            var controller = CreateController(sourceAuthProvider.Object);

            var result = controller.SignIn(CancellationToken.None);

            var redirect = Assert.IsType<RedirectResult>(result);
            Assert.Equal("https://login.microsoftonline.com/common/oauth2/v2.0/authorize?foo=bar", redirect.Url);
            sourceAuthProvider.VerifyAll();
            return Task.CompletedTask;
        }
        catch (Exception exception)
        {
            return Task.FromException(exception);
        }
    }

    [Fact]
    public async Task Unlink_Invoked_RemovesSavedLinkAndRedirectsToSetup()
    {
        var sourceAuthProvider = new Mock<ISourceAuthProvider>(MockBehavior.Strict);
        sourceAuthProvider
            .Setup(provider => provider.UnlinkCurrentAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var controller = CreateController(sourceAuthProvider.Object);

        var result = await controller.Unlink(CancellationToken.None);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Setup", redirect.ControllerName);
        sourceAuthProvider.Verify(provider => provider.UnlinkCurrentAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Unlink_HtmxRequest_ReturnsOkAndHxRedirectHeader()
    {
        var sourceAuthProvider = new Mock<ISourceAuthProvider>(MockBehavior.Strict);
        sourceAuthProvider
            .Setup(provider => provider.UnlinkCurrentAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var controller = CreateController(sourceAuthProvider.Object);
        controller.Request.Headers["HX-Request"] = "true";

        var result = await controller.Unlink(CancellationToken.None);

        Assert.IsType<OkResult>(result);
        Assert.Equal("/setup", controller.Response.Headers["HX-Redirect"].ToString());
        sourceAuthProvider.Verify(provider => provider.UnlinkCurrentAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Complete_WithAuthorizationCode_CompletesLinkAndRedirectsLocally()
    {
        var sourceAuthProvider = new Mock<ISourceAuthProvider>(MockBehavior.Strict);
        sourceAuthProvider
            .Setup(provider => provider.CompleteLinkAsync(
                "code-123",
                "state-abc",
                "https://chronoscope.local/link/onedrive/complete",
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var controller = CreateController(sourceAuthProvider.Object);

        var result = await controller.Complete("code-123", "state-abc", null, null, CancellationToken.None);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Setup", redirect.ControllerName);
        sourceAuthProvider.VerifyAll();
    }

    private static LinkSourceController CreateController(ISourceAuthProvider sourceAuthProvider)
    {
        var controller = new LinkSourceController(sourceAuthProvider, NullLogger<LinkSourceController>.Instance)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        controller.Request.Scheme = "https";
        controller.Request.Host = new HostString("chronoscope.local");
        return controller;
    }
}
