using Chronoscope.Application.Abstractions.Data;
using Chronoscope.Infrastructure.Auth;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Moq;

namespace Chronoscope.Tests.Unit.Infrastructure.Auth;

public sealed class MicrosoftGraphSourceAuthProviderTests
{
    [Fact]
    public void CreateLinkUriAsync_Invoked_IncludesStateParameter()
    {
        var provider = CreateProvider();

        var linkUri = provider.CreateLinkUriAsync("https://chronoscope.local/link/onedrive/complete", CancellationToken.None);
        var query = QueryHelpers.ParseQuery(new Uri(linkUri).Query);

        Assert.True(query.ContainsKey("state"));
        Assert.False(string.IsNullOrWhiteSpace(query["state"]));
    }

    [Fact]
    public async Task CompleteLinkAsync_WithInvalidState_ThrowsInvalidOperationException()
    {
        var provider = CreateProvider();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.CompleteLinkAsync(
            "code-123",
            "invalid-state",
            "https://chronoscope.local/link/onedrive/complete",
            CancellationToken.None));

        Assert.Equal("OneDrive callback state is invalid.", exception.Message);
    }

    private static MicrosoftGraphSourceAuthProvider CreateProvider()
    {
        var httpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        var sourceAuthStateRepository = new Mock<ISourceAuthStateRepository>(MockBehavior.Strict);
        var dataProtectionProvider = DataProtectionProvider.Create("Chronoscope.Tests.Unit");
        var options = Options.Create(new OneDriveAuthOptions
        {
            Instance = "https://login.microsoftonline.com",
            TenantId = "common",
            ClientId = "client-id",
            ClientSecret = "client-secret",
            Scopes = ["Files.Read", "User.Read", "offline_access"]
        });

        return new MicrosoftGraphSourceAuthProvider(
            httpClientFactory.Object,
            dataProtectionProvider,
            sourceAuthStateRepository.Object,
            options);
    }
}
