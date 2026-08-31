namespace Icms.Application.DTOs;

public record PagedRequest
{
    private const int MaxPageSize = 100;
    private int _pageSize = 20;

    public int Page { get; init; } = 1;

    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value < 1 ? 20 : value > MaxPageSize ? MaxPageSize : value;
    }

    public string? Search { get; init; }
    public string OrderBy { get; init; } = "Name";
    public bool Descending { get; init; }

    public int SafePage => Page < 1 ? 1 : Page;
}