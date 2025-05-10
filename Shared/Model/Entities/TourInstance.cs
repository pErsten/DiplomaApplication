using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shared.Model;

namespace Common.Model.Entities;

public class TourInstance
{
    [Key]
    public int Id { get; set; }

    public int TourId { get; set; }
    [ForeignKey("TourId")]
    public Tour Tour { get; set; }

    // Instance-specific dates
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // Instance status
    public TourInstanceStatus Status { get; set; }
    public int MaxParticipants { get; set; }
    public int CurrentParticipants { get; set; }

    // Navigation property for rates
    public List<TourInstanceRate> Rates { get; set; } = new();

    // Computed property for average rating
    public double? Rating => Rates.Any() ? Rates.Average(r => r.Rate / 10.0) : null;
} 