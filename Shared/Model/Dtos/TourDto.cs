using Common.Model.Entities;

namespace Shared.Model.Dtos;

public class TourDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string ImageUrl { get; set; }
    public List<int> Locations { get; set; } = new();
    public decimal Price { get; set; }
    public TourTypeEnum TourType { get; set; }
    public bool WithGuide { get; set; }
    public bool PrivateTour { get; set; }
    public bool GroupTour { get; set; }
    public SpecialOfferEnum SpecialOffers { get; set; }
    public int DurationDays { get; set; }
    public string GuideName { get; set; }
    public string GuideSurname { get; set; }
    public string GuideAvatarUrl { get; set; }
    public bool IsActive { get; set; }
    public TourInstanceStatus Status { get; set; }
} 