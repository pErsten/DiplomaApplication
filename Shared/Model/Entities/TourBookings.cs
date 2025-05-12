using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Model.Entities;

public class TourBooking
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int AccountId { get; set; }
    [ForeignKey(nameof(AccountId))]
    public Account Account { get; set; } = null!;

    [Required]
    public int TourInstanceId { get; set; }
    [ForeignKey(nameof(TourInstanceId))]
    public TourInstance TourInstance { get; set; } = null!;

    [Required]
    public DateTime BookedUtc { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }

    public bool IsCancelled => CancellationDate is not null;
    public string? CancellationReason { get; set; }
    public DateTime? CancellationDate { get; set; }
}