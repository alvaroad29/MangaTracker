using System.Net;

namespace MangaTracker.Application.Exceptions;

public abstract class AppException(string message, HttpStatusCode statusCode) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}