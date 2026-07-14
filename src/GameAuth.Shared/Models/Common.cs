namespace GameAuth.Shared.Models;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public string? ErrorMessage { get; }
    public string? ErrorCode { get; }

    private Result(bool isSuccess, T? data, string? errorMessage, string? errorCode)
    {
        IsSuccess = isSuccess;
        Data = data;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
    }

    public static Result<T> Success(T data) => new(true, data, null, null);
    public static Result<T> Failure(string errorMessage, string? errorCode = null) => new(false, default, errorMessage, errorCode);
}

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; init; } = Array.Empty<T>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
