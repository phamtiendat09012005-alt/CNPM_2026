namespace SportsStoreAI.API.DTOs;

public sealed record ApiResponse<T>(bool Success, string Message, T? Data)
{
    public static ApiResponse<T> Ok(T data, string message = "Thành công.")
        => new(true, message, data);

    public static ApiResponse<T> Fail(string message)
        => new(false, message, default);
}

public sealed record PagedResult<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);
