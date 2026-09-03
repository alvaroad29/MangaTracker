using System.Net;

namespace MangaTracker.Application.Exceptions;


public class ForbiddenAppException(string message) : AppException(message, HttpStatusCode.Forbidden);