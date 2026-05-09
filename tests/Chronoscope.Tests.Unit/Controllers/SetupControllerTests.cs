using Chronoscope.Application.Abstractions.SourceAuth;
using Chronoscope.Application.SourceAuth;
using Chronoscope.Web.Controllers;
using Chronoscope.Web.ViewModels.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Chronoscope.Tests.Unit.Controllers;

public sealed class SetupControllerTests
{
    [Fact]
    public async Task Index_NoLinkedSource_ReturnsSetupViewWithNotConnectedStep()
    {
        var sourceAuthProvider = new Mock<ISourceAuthProvider>(MockBehavior.Strict);
        sourceAuthProvider
            .Setup(provider => provider.GetCurrentAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("No linked source."));
        var controller = CreateController(sourceAuthProvider.Object);

        var result = await controller.Index(CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<SetupIndexViewModel>(view.Model);
        Assert.Equal("Setup", viewModel.Title);
        Assert.Equal("Not connected", viewModel.AuthStep.Status);
        Assert.False(viewModel.AuthStep.IsConnected);
    }

    [Fact]
    public async Task Index_LinkedSource_ReturnsSetupViewWithConnectedStep()
    {
        var sourceAuthProvider = new Mock<ISourceAuthProvider>(MockBehavior.Strict);
        sourceAuthProvider
            .Setup(provider => provider.GetCurrentAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SourceAuthSession("Ada Lovelace", "ada@example.com", "drive-123"));
        var controller = CreateController(sourceAuthProvider.Object);

        var result = await controller.Index(CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<SetupIndexViewModel>(view.Model);
        Assert.Equal("Connected", viewModel.AuthStep.Status);
        Assert.True(viewModel.AuthStep.IsConnected);
        Assert.Equal("Ada Lovelace", viewModel.AuthStep.DisplayName);
    }

    [Fact]
    public async Task Index_HtmxRequest_ReturnsAuthStepPartial()
    {
        var sourceAuthProvider = new Mock<ISourceAuthProvider>(MockBehavior.Strict);
        sourceAuthProvider
            .Setup(provider => provider.GetCurrentAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SourceAuthSession("Ada Lovelace", "ada@example.com", "drive-123"));
        var controller = CreateController(sourceAuthProvider.Object);
        controller.Request.Headers["HX-Request"] = "true";

        var result = await controller.Index(CancellationToken.None);

        var partial = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("_AuthStep", partial.ViewName);
        var viewModel = Assert.IsType<SetupAuthStepViewModel>(partial.Model);
        Assert.Equal("Connected", viewModel.Status);
        Assert.True(viewModel.IsConnected);
    }

    private static SetupController CreateController(ISourceAuthProvider sourceAuthProvider)
    {
        return new SetupController(sourceAuthProvider, NullLogger<SetupController>.Instance)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }
}
