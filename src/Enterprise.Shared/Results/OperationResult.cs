namespace Enterprise.Shared.Results;

public sealed class OperationResult
{
    public bool Succeeded { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    public int StatusCode { get; init; }

    public static OperationResult Ok(int statusCode = 200) =>
        new() { Succeeded = true, StatusCode = statusCode };

    public static OperationResult Fail(string code, string message, int statusCode) =>
        new() { Succeeded = false, ErrorCode = code, ErrorMessage = message, StatusCode = statusCode };
}

public sealed class OperationResult<T>
{
    public bool Succeeded { get; init; }
    public T? Value { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    public int StatusCode { get; init; }

    public static OperationResult<T> Ok(T value, int statusCode = 200) =>
        new() { Succeeded = true, Value = value, StatusCode = statusCode };

    public static OperationResult<T> Fail(string code, string message, int statusCode) =>
        new() { Succeeded = false, ErrorCode = code, ErrorMessage = message, StatusCode = statusCode };
}
