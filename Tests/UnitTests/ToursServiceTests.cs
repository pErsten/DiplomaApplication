using Backend.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shared.Model;
using Shared.Model.Dtos;
using System.Text.Json;
using Xunit;

namespace Tests.UnitTests;

public class ToursServiceTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly ToursService _service;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IConfiguration _configuration;

    public ToursServiceTests(TestFixture fixture)
    {
        _fixture = fixture;
        _loggerFactory = Substitute.For<ILoggerFactory>();
        var logger = Substitute.For<ILogger<ToursService>>();
        _loggerFactory.CreateLogger(Arg.Any<string>()).Returns(logger);
        
        _configuration = Substitute.For<IConfiguration>();
        _configuration.GetValue<string>("ServerUrl").Returns("http://test-server");

        _service = new ToursService(_loggerFactory, _fixture.DbContext, _configuration);
    }

    [Fact]
    public async Task GetAllTours_WithNoFilters_ReturnsAllActiveTours()
    {
        // Arrange
        var filters = new TourFiltersDto
        {
            ToPrice = int.MaxValue,
            ToDurationDays = int.MaxValue
        };
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _service.GetAllTours(page, pageSize, filters, CancellationToken.None);

        // Assert
        Assert.True(result.TryGetValue(out var data));
        Assert.NotNull(data);
        Assert.Equal(3, data.TotalCount); // We have 3 active tour instances
        Assert.Equal(3, data.Items.Count);
        Assert.Equal(1, data.TotalPages);
        Assert.Equal(100.00m, data.MinPrice);
        Assert.Equal(150.00m, data.MaxPrice);
        Assert.Equal(3, data.MinDuration);
        Assert.Equal(5, data.MaxDuration);
    }

    [Fact]
    public async Task GetAllTours_WithPriceFilter_ReturnsFilteredTours()
    {
        // Arrange
        var filters = new TourFiltersDto
        {
            FromPrice = 120.00m,
            ToPrice = 200.00m,
            ToDurationDays = int.MaxValue
        };
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _service.GetAllTours(page, pageSize, filters, CancellationToken.None);

        // Assert
        Assert.True(result.TryGetValue(out var data));
        Assert.NotNull(data);
        Assert.Equal(1, data.TotalCount); // Only the Nature Adventure tour
        Assert.Single(data.Items);
        Assert.Equal("Nature Adventure", data.Items[0].Title);
    }

    [Fact]
    public async Task GetAllTours_WithTourTypeFilter_ReturnsFilteredTours()
    {
        // Arrange
        var filters = new TourFiltersDto
        {
            TourTypes = new List<TourTypeEnum> { TourTypeEnum.Hiking },
            ToPrice = int.MaxValue,
            ToDurationDays = int.MaxValue
        };
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _service.GetAllTours(page, pageSize, filters, CancellationToken.None);
        var success = result.TryGetValue(out var data);

        // Assert
        Assert.True(success);
        Assert.NotNull(data);
        Assert.Equal(2, data.TotalCount); // Both instances of Historical City Tour
        Assert.Equal(2, data.Items.Count);
        Assert.All(data.Items, tour => Assert.Equal(TourTypeEnum.Hiking, tour.TourType));
    }

    [Fact]
    public async Task GetTourById_WithValidId_ReturnsTourDetails()
    {
        // Act
        var result = await _service.GetTourById(1, CancellationToken.None);

        // Assert
        Assert.True(result.TryGetValue(out var data));
        Assert.NotNull(data);
        Assert.Equal(1, data.Id);
        Assert.Equal("Historical City Tour", data.Title);
        Assert.Equal(100.00m, data.Price);
        Assert.Equal(5, data.Rating);
        Assert.Equal(20, data.MaxParticipants);
        Assert.Equal(1, data.CurrentParticipants); // One booking exists
        Assert.False(data.IsCancelled);
    }

    [Fact]
    public async Task GetTourById_WithInvalidId_ReturnsError()
    {
        // Act
        var result = await _service.GetTourById(999, CancellationToken.None);

        // Assert
        Assert.False(result.TryGetValue(out var data));
        Assert.Null(data);
    }

    [Fact]
    public async Task GetAllTours_WithSpecialOffersFilter_ReturnsFilteredTours()
    {
        // Arrange
        var filters = new TourFiltersDto
        {
            OnSale = true,
            ToPrice = int.MaxValue,
            ToDurationDays = int.MaxValue
        };
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _service.GetAllTours(page, pageSize, filters, CancellationToken.None);

        // Assert
        Assert.True(result.TryGetValue(out var data));
        Assert.NotNull(data);
        Assert.Equal(2, data.TotalCount); // Both instances of Historical City Tour
        Assert.Equal(2, data.Items.Count);
        Assert.All(data.Items, tour => Assert.True((tour.SpecialOffers & SpecialOfferEnum.OnSale) > 0));
    }

    [Fact]
    public async Task GetAllTours_WithClassificationFilter_ReturnsFilteredTours()
    {
        // Arrange
        var filters = new TourFiltersDto
        {
            PrivateTour = true,
            ToPrice = int.MaxValue,
            ToDurationDays = int.MaxValue
        };
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _service.GetAllTours(page, pageSize, filters, CancellationToken.None);

        // Assert
        Assert.True(result.TryGetValue(out var data));
        Assert.NotNull(data);
        Assert.Equal(1, data.TotalCount); // Only the Nature Adventure tour
        Assert.Single(data.Items);
        Assert.Equal(TourClassificationEnum.Private, data.Items[0].Classification);
    }
} 