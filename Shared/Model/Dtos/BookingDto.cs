namespace Shared.Model.Dtos;

public class BookingDto
{
    public int Id { get; set; }
    public int TourInstanceId { get; set; }
    public string TourTitle { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalPrice { get; set; }
    public bool IsCancelled { get; set; }
    public bool HasRated { get; set; }
} 