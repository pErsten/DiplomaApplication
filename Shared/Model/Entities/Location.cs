using System.ComponentModel.DataAnnotations;

namespace Common.Model.Entities;

public class Location
{
    [Key]
    public int GeoId { get; set; }
    public float Latitude { get; set; }
    public float Longitude { get; set; }
    public string Name { get; set; }
    public string CountryCode { get; set; }
}