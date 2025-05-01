namespace Shared.Model.Dtos;

public class CityLocationDto
{
    public int GeoId { get; set; }
    public float Latitude { get; set; }
    public float Longitude { get; set; }
    public string Name { get; set; }
    public string CountryCode { get; set; }
    public Dictionary<LocalizationCode, (string, string)> Names { get; set; } = new Dictionary<LocalizationCode, (string, string)>();
}