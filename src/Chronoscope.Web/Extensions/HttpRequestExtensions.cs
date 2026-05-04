using Microsoft.AspNetCore.Http;

namespace Chronoscope.Web.Extensions;

public static class HttpRequestExtensions
{
    public static bool IsHtmx(this HttpRequest request)
    {
        return string.Equals(request.Headers["HX-Request"], "true", StringComparison.OrdinalIgnoreCase);
    }
}
