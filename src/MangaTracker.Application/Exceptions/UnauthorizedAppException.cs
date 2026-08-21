using System.Net;

namespace MangaTracker.Application.Exceptions;

public class UnauthorizedAppException(string message = "Credenciales inválidas.")
    : AppException(message, HttpStatusCode.Unauthorized);