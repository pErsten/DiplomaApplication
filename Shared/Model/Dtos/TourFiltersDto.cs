namespace Shared.Model.Dtos;

public class TourFiltersDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal FromPrice { get; set; }
    public decimal ToPrice { get; set; }
    public int FromDurationDays { get; set; }
    public int ToDurationDays { get; set; }
    public bool? WithGuide { get; set; }
    public bool? PrivateTour { get; set; }
    public bool? GroupTour { get; set; }
    public IEnumerable<TourTypeEnum>? TourTypes { get; set; }
    public double MinRating { get; set; }
    public DestinationCountEnum? DestinationsCount { get; set; }
    public int? SelectedDestination { get; set; }
    public bool? OnSale { get; set; }
    public bool? StartsSoon { get; set; }
    public bool? SpecialDiscount { get; set; }
    public string? SortBy { get; set; }
} 