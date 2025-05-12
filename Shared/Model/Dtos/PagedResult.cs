namespace Common.Model.Dtos;

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int MinDuration { get; set; }
    public int MaxDuration { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
} 