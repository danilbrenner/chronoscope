namespace Chronoscope.Web.ViewModels.Setup;

public sealed record SetupIndexViewModel(
    string Title,
    string Message,
    SetupAuthStepViewModel AuthStep);
