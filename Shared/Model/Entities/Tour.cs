using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shared.Model;

namespace Common.Model.Entities;

public class Tour
{
    [Key]
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string ImageUrl { get; set; }
    public List<int> Locations { get; set; } = new();
    public decimal Price { get; set; }
    public TourTypeEnum TourType { get; set; }
    public bool WithGuide { get; set; }
    public SpecialOfferEnum SpecialOffers { get; set; }
    public int DurationDays { get; set; }
    public TourClassificationEnum Classification { get; set; }

    public int GuideId { get; set; }
    [ForeignKey("GuideId")]
    public Guide Guide { get; set; }

    // Template status
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }

    public List<TourInstance> Instances { get; set; }
}