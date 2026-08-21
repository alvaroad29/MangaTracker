using System.Net;

namespace MangaTracker.Application.Exceptions;

public class ConflictAppException(string message) : AppException(message, HttpStatusCode.Conflict);