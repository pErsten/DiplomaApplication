using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.DbContexts;
using Shared.Model;
using Shared.Model.Dtos;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Dipchik.Controllers;

public static class ToursController
{
    public static IEndpointRouteBuilder AddToursController(this IEndpointRouteBuilder builder, params AccountRolesEnum[] roleRequirements)
    {
        var group = builder.MapGroup("Tours");
        foreach (var role in roleRequirements)
        {
            group.RequireAuthorization(role.ToString());
        }

        group.MapPost("/GetAll", GetAllTours);
        group.MapGet("/GetById/{id}", GetTourById);
        group.MapGet("/GetRecommendations", GetTourRecommendations);

        return builder;
    }
    
    public static async Task<IResult> GetAllTours(
        SqlContext dbContext,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromBody] TourFiltersDto filters,
        CancellationToken stoppingToken)
    {
        if (page < 1)
        {
            return Results.BadRequest("Page number must be greater than 0");
        }

        if (pageSize < 1 || pageSize > Constants.MaxPageSize)
        {
            return Results.BadRequest($"Page size must be between 1 and {Constants.MaxPageSize}");
        }

        var query = dbContext.TourInstances
            .Where(x => x.Status == TourInstanceStatus.Scheduled)
            .Include(x => x.Tour)
            .ThenInclude(x => x.Guide)
            .ThenInclude(x => x.Account)
            .AsQueryable();
        
        if (filters.SelectedDestination.HasValue)
        {
            query = query.Where(t => t.Tour.Locations.Contains(filters.SelectedDestination.Value));
        }

        if (filters.FromPrice > Constants.DefaultMinPrice)
        {
            query = query.Where(t => t.Tour.Price >= filters.FromPrice);
        }

        if (filters.ToPrice < Constants.DefaultMaxPrice)
        {
            query = query.Where(t => t.Tour.Price <= filters.ToPrice);
        }

        if (filters.TourTypes != null && filters.TourTypes.Any())
        {
            query = query.Where(t => filters.TourTypes.Contains(t.Tour.TourType));
        }

        if (filters.WithGuide.HasValue)
        {
            query = query.Where(t => t.Tour.WithGuide == filters.WithGuide.Value);
        }

        if (filters.PrivateTour == true)
        {
            query = query.Where(t => (t.Tour.Classification == TourClassificationEnum.Private));
        }

        else if (filters.GroupTour == true)
        {
            query = query.Where(t => t.Tour.Classification == TourClassificationEnum.Group);
        }

        if (filters.OnSale.HasValue && filters.OnSale.Value)
        {
            query = query.Where(t => (t.Tour.SpecialOffers & SpecialOfferEnum.OnSale) > 0);
        }

        if (filters.SpecialDiscount.HasValue && filters.SpecialDiscount.Value)
        {
            query = query.Where(t => (t.Tour.SpecialOffers & SpecialOfferEnum.SpecialDiscount) == SpecialOfferEnum.SpecialDiscount);
        }

        if (filters.DestinationsCount.HasValue)
        {
            switch (filters.DestinationsCount.Value)
            {
                case DestinationCountEnum.Single:
                    query = query.Where(t => t.Tour.Locations.Count == 1);
                    break;
                case DestinationCountEnum.TwoToThree:
                    query = query.Where(t => t.Tour.Locations.Count == 2 || t.Tour.Locations.Count == 3);
                    break;
                case DestinationCountEnum.FourOrMore:
                    query = query.Where(t => t.Tour.Locations.Count >= 4);
                    break;
            }
        }

        // Apply instance-level filters
        if (filters.StartDate.HasValue)
        {
            query = query.Where(i => i.StartDate.Date == filters.StartDate.Value.Date);
        }

        query = query.Where(i => i.Tour.DurationDays >= filters.FromDurationDays);
        query = query.Where(i => i.Tour.DurationDays <= filters.ToDurationDays);

        if (filters.MinRating > 1.0d)
        {
            query = query.Where(i => i.Rating >= filters.MinRating);
        }

        if (filters.StartsSoon.HasValue && filters.StartsSoon.Value)
        {
            var now = DateTime.UtcNow;
            query = query.Where(i => (i.StartDate - now).TotalDays <= Constants.STARTS_SOON_DAYS);
        }

        var totalCount = await query.CountAsync(stoppingToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        // Apply sorting
        if (!string.IsNullOrEmpty(filters.SortBy))
        {
            query = filters.SortBy switch
            {
                "price" => query.OrderBy(t => t.Tour.Price),
                "rating" => query.OrderByDescending(i => i.Rating ?? 0),
                "duration" => query.OrderBy(t => t.Tour.DurationDays),
                "startdate" => query.OrderBy(i => i.StartDate),
                _ => query
            };
        }

        var tours = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(stoppingToken);

        var tourDtos = tours.Select(t => new TourDto
        {
            Id = t.Id,
            Title = t.Tour.Title,
            Description = t.Tour.Description,
            ImageUrl = t.Tour.ImageUrl,
            Locations = t.Tour.Locations,
            Price = t.Tour.Price,
            TourType = t.Tour.TourType,
            WithGuide = t.Tour.WithGuide,
            SpecialOffers = t.Tour.SpecialOffers,
            DurationDays = t.Tour.DurationDays,
            GuideName = t.Tour.Guide.Name,
            GuideSurname = t.Tour.Guide.Surname,
            GuideAvatarUrl = t.Tour.Guide.Account.AvatarUrl,
            Classification = t.Tour.Classification
        }).ToList();

        var absoluteData = await dbContext.Tours.GroupBy(x => 1).Select(x => new
        {
            MinDuration = x.Min(x => x.DurationDays),
            MaxDuration = x.Max(x => x.DurationDays),
            MinPrice = x.Min(x => x.Price),
            MaxPrice = x.Max(x => x.Price),
        }).FirstAsync(stoppingToken);

        var result = new PagedResult<TourDto>
        {
            Items = tourDtos,
            TotalCount = totalCount,
            TotalPages = totalPages,
            MinDuration = absoluteData.MinDuration,
            MaxDuration = absoluteData.MaxDuration,
            MinPrice = absoluteData.MinPrice,
            MaxPrice = absoluteData.MaxPrice,
            CurrentPage = page,
            PageSize = pageSize
        };

        return Results.Ok(result);
    }

    public static async Task<IResult> GetTourById(
        int id,
        SqlContext dbContext,
        CancellationToken stoppingToken)
    {
        var tour = await dbContext.TourInstances.Include(x => x.Rates).Where(t => t.Id == id).Select(tour => new TourDescDto
        {
            Id = tour.Id,
            TourId = tour.TourId,
            ImageUrl = tour.Tour.ImageUrl,
            Title = tour.Tour.Title,
            Description = tour.Tour.Description,
            Locations = tour.Tour.Locations,
            Price = tour.Tour.Price,
            TourType = tour.Tour.TourType,
            StartDate = tour.StartDate,
            EndDate = tour.EndDate,
            Rating = tour.Rating ?? 0d,
            MaxParticipants = tour.MaxParticipants,
            CurrentParticipants = tour.Bookings.Count,
            GuideName = tour.Tour.Guide.Name,
            GuideSurname = tour.Tour.Guide.Surname,
            GuideAvatarUrl = tour.Tour.Guide.Account.AvatarUrl,
            Status = tour.Status,
            Classification = tour.Tour.Classification
        }).FirstOrDefaultAsync(stoppingToken);

        if (tour == null)
        {
            return Results.NotFound($"Tour with ID {id} not found");
        }

        return Results.Ok(tour);
    }

    public static async Task<IResult> GetTourRecommendations(
        HttpContext context,
        SqlContext dbContext,
        IConfiguration configuration,
        CancellationToken stoppingToken)
    {
        var accountId = context.UserId();
        if (accountId == null)
        {
            return Results.Unauthorized();
        }

        var account = await dbContext.Accounts.FirstOrDefaultAsync(x => x.AccountId == accountId, stoppingToken);
        if (account is null)
        {
            return Results.BadRequest("Account not found");
        }

        // Get user's booking history
        var userBookings = await dbContext.TourBookings
            .Where(x => x.AccountId == account.Id && x.CancellationDate == null)
            .Include(x => x.TourInstance)
            .ThenInclude(x => x.Tour)
            .OrderByDescending(x => x.BookedUtc)
            .Take(10)
            .ToListAsync(stoppingToken);

        // Get user's ratings
        var userRatings = await dbContext.TourInstanceRates
            .Where(x => x.TouristAccountId == account.Id)
            .Include(x => x.TourInstance)
            .ThenInclude(x => x.Tour)
            .OrderByDescending(x => x.RatedTimeUtc)
            .Take(10)
            .ToListAsync(stoppingToken);

        // Get popular tours based on ratings
        var popularTours = await dbContext.TourInstances
            .Where(x => x.Status == TourInstanceStatus.Scheduled)
            .Include(x => x.Tour)
            .Include(x => x.Rates)
            .OrderByDescending(x => x.Rates.Average(r => r.Rate))
            .Take(20)
            .ToListAsync(stoppingToken);

        // Prepare data for ChatGPT
        var userPreferences = new
        {
            PreviouslyBookedTourTypes = userBookings.Select(x => x.TourInstance.Tour.TourType).ToList(),
            PreviouslyBookedLocations = userBookings.SelectMany(x => x.TourInstance.Tour.Locations).Distinct().ToList(),
            PreviouslyRatedTours = userRatings.Select(x => x.TourInstance.TourId).ToList(),
            PreviousRatings = userRatings.Select(x => x.Rate / 10.0).ToList(),
        };

        var availableTours = popularTours.Select(x => new
        {
            TourId = x.TourId,
            Title = x.Tour.Title,
            Description = x.Tour.Description,
            TourType = x.Tour.TourType,
            Locations = x.Tour.Locations,
            Price = x.Tour.Price,
            DurationDays = x.Tour.DurationDays,
            WithGuide = x.Tour.WithGuide,
            Rating = x.Rating ?? 0,
            SpecialOffers = x.Tour.SpecialOffers
        }).ToList();

        // Prepare ChatGPT prompt
        var prompt = new
        {
            model = "gpt-3.5-turbo",
            messages = new[]
            {
                new { role = "system", content = $"You are a tour recommendation expert. Analyze the user's preferences and available tours to recommend the best matches. Consider tour types, locations, price range, duration, and special offers. Your reason text must be made in {account.Locale.ToString()} language. Return a JSON array of tour IDs in order of recommendation, and a brief explanation of why these tours were recommended. Your response must be a JSON, that should follow this structure: private class ChatGptRecommendation{{ public List<int> recommendedTourIds {{ get; set; }} = new(); public string reason {{ get; set; }} = string.Empty; }}" },
                new { role = "user", content = JsonSerializer.Serialize(new { UserPreferences = userPreferences, AvailableTours = availableTours }) }
            },
            temperature = 0.7,
            max_tokens = 500
        };

        // Call ChatGPT API
        var openAiUrl = configuration.GetValue<string>("OpenAiUrl");
        var openAiKey = configuration.GetValue<string>("OpenAiKey");
        using var cli = new HttpClient();
        cli.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", openAiKey);
        var response = await cli.PostAsync(openAiUrl, 
            new StringContent(JsonSerializer.Serialize(prompt), Encoding.UTF8, "application/json"), 
            stoppingToken);

        if (!response.IsSuccessStatusCode)
        {
            return Results.Problem("Failed to get recommendations from AI service");
        }

        var responseContent = await response.Content.ReadAsStringAsync(stoppingToken);
        var chatGptResponse = JsonSerializer.Deserialize<ChatGptResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (chatGptResponse?.Choices == null || !chatGptResponse.Choices.Any())
        {
            return Results.Problem("Invalid response from AI service");
        }

        // Parse ChatGPT response
        var recommendation = JsonSerializer.Deserialize<ChatGptRecommendation>(
            chatGptResponse.Choices[0].Message.Content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (recommendation == null)
        {
            return Results.Problem("Failed to parse AI recommendations");
        }

        // Get recommended tours
        var recommendedTours = await dbContext.TourInstances
            .Where(x => recommendation.RecommendedTourIds.Contains(x.TourId) && x.Status == TourInstanceStatus.Scheduled)
            .Include(x => x.Tour)
            .ThenInclude(x => x.Guide)
            .ThenInclude(x => x.Account)
            .Select(x => new TourDto
            {
                Id = x.Id,
                Title = x.Tour.Title,
                Description = x.Tour.Description,
                ImageUrl = x.Tour.ImageUrl,
                Locations = x.Tour.Locations,
                Price = x.Tour.Price,
                TourType = x.Tour.TourType,
                WithGuide = x.Tour.WithGuide,
                SpecialOffers = x.Tour.SpecialOffers,
                DurationDays = x.Tour.DurationDays,
                GuideName = x.Tour.Guide.Name,
                GuideSurname = x.Tour.Guide.Surname,
                GuideAvatarUrl = x.Tour.Guide.Account.AvatarUrl,
                Classification = x.Tour.Classification
            })
            .ToListAsync(stoppingToken);

        // Order tours according to ChatGPT's recommendation
        recommendedTours = recommendedTours
            .OrderBy(x => recommendation.RecommendedTourIds.IndexOf(x.Id))
            .Take(3)
            .ToList();

        return Results.Ok(new TourRecommendationResponseDto
        {
            RecommendedTours = recommendedTours,
            RecommendationReason = recommendation.Reason
        });
    }

    private class ChatGptResponse
    {
        public List<ChatGptChoice> Choices { get; set; } = new();
    }

    private class ChatGptChoice
    {
        public ChatGptMessage Message { get; set; } = new();
    }

    private class ChatGptMessage
    {
        public string Content { get; set; } = string.Empty;
    }

    private class ChatGptRecommendation
    {
        public List<int> RecommendedTourIds { get; set; } = new();
        public string Reason { get; set; } = string.Empty;
    }
} 