namespace GreenNode.Application.Common.Exceptions;

public class ForbiddenException : Exception
{
    public ForbiddenException(string message = "No tienes acceso a este recurso.")
        : base(message)
    {
    }
}
