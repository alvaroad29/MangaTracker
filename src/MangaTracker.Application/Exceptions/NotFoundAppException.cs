using System.Net;

namespace MangaTracker.Application.Exceptions;

public class NotFoundAppException(string message) : AppException(message, HttpStatusCode.NotFound);