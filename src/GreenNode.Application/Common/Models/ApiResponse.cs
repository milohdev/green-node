namespace GreenNode.Application.Common.Models;

public class ApiResponse<T>
{
    public T? Data { get; init; }
    public string Message { get; init; } = string.Empty;

    public static ApiResponse<T> Success(T data, string message = "OK") =>
        new() { Data = data, Message = message };

    public static ApiResponse<T> Fail(string message) =>
        new() { Data = default, Message = message };
}

public class ApiErrorResponse
{
    public string Message { get; init; } = string.Empty;
    public IEnumerable<string> Errors { get; init; } = [];
}
