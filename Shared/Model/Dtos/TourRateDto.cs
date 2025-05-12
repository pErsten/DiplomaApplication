namespace Shared.Model.Dtos;

public class TourRateDto
{
    public int TourInstanceId { get; set; }
    public byte Rate { get; set; }
    public string Commentary { get; set; } = string.Empty;
} 