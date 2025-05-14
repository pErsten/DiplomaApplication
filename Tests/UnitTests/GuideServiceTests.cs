using Backend.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shared.Model;
using Xunit;

namespace Tests.UnitTests;

public class GuideServiceTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly GuideService _service;
    private readonly ILoggerFactory _loggerFactory;

    public GuideServiceTests(TestFixture fixture)
    {
        _fixture = fixture;
        _loggerFactory = Substitute.For<ILoggerFactory>();
        var logger = Substitute.For<ILogger<GuideService>>();
        _loggerFactory.CreateLogger(Arg.Any<string>()).Returns(logger);

        _service = new GuideService(_loggerFactory, _fixture.DbContext);
    }

    [Fact]
    public async Task GetGuideTours_WithValidGuideId_ReturnsGuideTours()
    {
        // Arrange
        var guideAccountId = "test_guide1"; // John Doe's account

        // Act
        var result = await _service.GetGuideTours(guideAccountId, CancellationToken.None);

        // Assert
        Assert.True(result.TryGetValue(out var data));
        Assert.NotNull(data);
        Assert.Equal(2, data.Count); // Both instances of Historical City Tour
        Assert.All(data, tour => 
        {
            Assert.Equal("Historical City Tour", tour.Title);
            Assert.Equal("John", tour.GuideName);
            Assert.Equal("Doe", tour.GuideSurname);
            Assert.Equal("https://test.com/avatar1.jpg", tour.GuideAvatarUrl);
        });
    }

    [Fact]
    public async Task GetGuideTours_WithInvalidGuideId_ReturnsEmptyList()
    {
        // Arrange
        var invalidGuideId = "non_existent_guide";

        // Act
        var result = await _service.GetGuideTours(invalidGuideId, CancellationToken.None);

        // Assert
        Assert.True(result.TryGetValue(out var data));
        Assert.NotNull(data);
        Assert.Empty(data);
    }

    [Fact]
    public async Task GetGuideTourInstances_WithValidGuideId_ReturnsGuideTourInstances()
    {
        // Arrange
        var guideAccountId = "test_guide1"; // John Doe's account

        // Act
        var result = await _service.GetGuideTourInstances(guideAccountId, CancellationToken.None);

        // Assert
        Assert.True(result.TryGetValue(out var data));
        Assert.NotNull(data);
        Assert.Equal(2, data.Count); // Both instances of Historical City Tour
        Assert.All(data, tour => 
        {
            Assert.Equal("Historical City Tour", tour.Title);
            Assert.Equal("John", tour.GuideName);
            Assert.Equal("Doe", tour.GuideSurname);
            Assert.Equal("https://test.com/avatar1.jpg", tour.GuideAvatarUrl);
            Assert.False(tour.IsCancelled);
        });

        // Verify specific instance details
        var firstInstance = data.First();
        Assert.Equal(1, firstInstance.Id);
        Assert.Equal(5, firstInstance.Rating);
        Assert.Equal(20, firstInstance.MaxParticipants);
        Assert.Equal(1, firstInstance.CurrentParticipants); // One booking exists
        Assert.True(firstInstance.StartDate > DateTime.UtcNow);
        Assert.True(firstInstance.EndDate > firstInstance.StartDate);
    }

    [Fact]
    public async Task GetGuideTourInstances_WithDifferentGuideId_ReturnsDifferentGuideTours()
    {
        // Arrange
        var guideAccountId = "test_guide2"; // Jane Smith's account

        // Act
        var result = await _service.GetGuideTourInstances(guideAccountId, CancellationToken.None);

        // Assert
        Assert.True(result.TryGetValue(out var data));
        Assert.NotNull(data);
        Assert.Single(data); // Only the Nature Adventure tour
        var tour = data.First();
        Assert.Equal("Nature Adventure", tour.Title);
        Assert.Equal("Jane", tour.GuideName);
        Assert.Equal("Smith", tour.GuideSurname);
        Assert.Equal("https://test.com/avatar2.jpg", tour.GuideAvatarUrl);
        Assert.Equal(150.00m, tour.Price);
        Assert.Equal(10, tour.MaxParticipants);
        Assert.Equal(0, tour.CurrentParticipants); // No bookings for this tour
    }

    [Fact]
    public async Task GetGuideTourInstances_WithInvalidGuideId_ReturnsEmptyList()
    {
        // Arrange
        var invalidGuideId = "non_existent_guide";

        // Act
        var result = await _service.GetGuideTourInstances(invalidGuideId, CancellationToken.None);

        // Assert
        Assert.True(result.TryGetValue(out var data));
        Assert.NotNull(data);
        Assert.Empty(data);
    }
} 