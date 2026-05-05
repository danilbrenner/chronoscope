using Chronoscope.Application.Abstractions.SourceAuth;
using Chronoscope.Application.SourceAuth;
using Chronoscope.Web.Controllers;
using Chronoscope.Web.ViewModels.SourceAuth;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Chronoscope.Tests.Unit.Controllers;

public sealed class SourceAuthControllerTests
{
    [Fact]
    public void SignIn_Invoked_ReturnsChallengeForMicrosoftSignIn()
    {
        var sourceAuthService = new Mock<ISourceAuthService>(MockBehavior.Strict);
        var controller = new SourceAuthController(sourceAuthService.Object);

        var result = controller.SignIn();

        var challenge = Assert.IsType<ChallengeResult>(result);
        Assert.Empty(challenge.AuthenticationSchemes);
        Assert.Equal("/auth/onedrive/complete", challenge.Properties?.RedirectUri);
    }

    [Fact]
    public async Task Complete_AuthenticatedUser_ReturnsConnectionViewModel()
    {
        var session = new SourceAuthSession("Ada Lovelace", "ada@example.com", "drive-123");
        var sourceAuthService = new Mock<ISourceAuthService>(MockBehavior.Strict);
        sourceAuthService
            .Setup(service => service.GetCurrentAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var controller = new SourceAuthController(sourceAuthService.Object);

        var result = await controller.Complete(CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<SourceAuthCompleteViewModel>(view.Model);
        Assert.Equal("OneDrive connected", viewModel.Title);
        Assert.Equal("Microsoft sign-in succeeded and Chronoscope can call Microsoft Graph on your behalf.", viewModel.Message);
        Assert.Equal(session.DisplayName, viewModel.DisplayName);
        Assert.Equal(session.EmailAddress, viewModel.EmailAddress);
        Assert.Equal(session.DriveId, viewModel.DriveId);
    }
}
