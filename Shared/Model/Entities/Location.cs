using System.ComponentModel.DataAnnotations;

namespace Common.Model.Entities;

public class Location
{
    [Key]
    public int GeoId { get; set; }
    public string Name { get; set; }
    public string Country { get; set; }
}