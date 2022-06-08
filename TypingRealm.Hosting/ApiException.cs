using System;

namespace TypingRealm.Hosting;

public abstract class ApiException : Exception
{
    protected ApiException(string message, int statusCode) : base(message)
        => StatusCode = statusCode;

    public int StatusCode { get; set; }
}

public sealed class NotFoundApiException : ApiException
{
    public NotFoundApiException(string message) : base(message, 404) { }
}

public sealed class BadRequestApiException : ApiException
{
    public BadRequestApiException(string message) : base(message, 400) { }
}
