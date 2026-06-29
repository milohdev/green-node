namespace GreenNode.Application.Common.Exceptions;

public class AppValidationException : Exception
{
    public IEnumerable<string> Errors { get; }

    public AppValidationException(string message)
        : base(message)
    {
        Errors = [message];
    }

    public AppValidationException(IEnumerable<string> errors)
        : base("Se encontraron errores de validación.")
    {
        Errors = errors;
    }
}
