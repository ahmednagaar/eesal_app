namespace ReceiptSystem.Desktop.Services;

/// <summary>
/// Thrown when the API returns HTTP 401 Unauthorized.
/// Distinguishes auth failures from generic HttpRequestException.
/// </summary>
public class ApiUnauthorizedException : Exception
{
    public ApiUnauthorizedException()
        : base("Unauthorized (401)") { }

    public ApiUnauthorizedException(string message)
        : base(message) { }

    public ApiUnauthorizedException(string message, Exception inner)
        : base(message, inner) { }
}
