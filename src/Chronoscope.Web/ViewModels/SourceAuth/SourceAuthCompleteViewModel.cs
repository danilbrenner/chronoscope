namespace Chronoscope.Web.ViewModels.SourceAuth;

public sealed record SourceAuthCompleteViewModel(
    string Title,
    string Message,
    string DisplayName,
    string? EmailAddress,
    string DriveId);
