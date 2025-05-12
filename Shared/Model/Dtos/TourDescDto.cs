using Common.Model.Entities;

namespace Shared.Model.Dtos;

public class TourDescDto
{
    public int Id { get; set; }
    public int TourId { get; set; }
    public string ImageUrl { get; set; }
    public string Title { get; set; }
    public string GuideAvatarUrl { get; set; }
    public string GuideName { get; set; }
    public string GuideSurname { get; set; }
    public string Description { get; set; }
    public List<int> Locations { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public TourTypeEnum TourType { get; set; }
    public decimal Price { get; set; }
    public double Rating { get; set; }
    public int MaxParticipants { get; set; }
    public int CurrentParticipants { get; set; }
    public bool IsCancelled { get; set; }

    public TourInstanceStatus Status => TourInstance.GetStatus(IsCancelled, EndDate ?? DateTime.MinValue);
    public TourClassificationEnum Classification { get; set; }
}