namespace Chronoscope.Web.ViewModels.Setup;

public sealed record SetupAuthStepViewModel(
    string Status,
    string Title,
    string Message,
    bool IsConnected,
    string? DisplayName,
    string? EmailAddress,
    string? DriveId);
