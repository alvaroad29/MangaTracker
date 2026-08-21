using System.Net;

namespace MangaTracker.Application.Exceptions;

public class AppValidationException(IEnumerable<string> errors)
    : AppException(string.Join(" ", errors), HttpStatusCode.BadRequest);