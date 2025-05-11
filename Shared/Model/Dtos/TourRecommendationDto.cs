using System.Text.Json.Serialization;

namespace Shared.Model.Dtos;

public class TourRecommendationResponseDto
{
    public List<TourDto> RecommendedTours { get; set; } = new();
    public string RecommendationReason { get; set; } = string.Empty;
} 