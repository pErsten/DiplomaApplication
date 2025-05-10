using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Model.Entities;

public class TourInstanceRate
{
    [Key]
    public int Id { get; set; }
    public byte Rate { get; set; } // from 10 to 50, to be represented as scale from 1.0 to 5.0
    public string TouristCommentary { get; set; }
    public DateTime RatedTimeUtc { get; set; }

    public int TouristAccountId { get; set; }
    [ForeignKey("TouristAccountId")]
    public Account TouristAccount { get; set; }

    public int TourInstanceId { get; set; }
    [ForeignKey("TourInstanceId")]
    public TourInstance TourInstance { get; set; }
} 