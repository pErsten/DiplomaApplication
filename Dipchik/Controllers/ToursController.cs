using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.DbContexts;
using Shared.Model;
using Shared.Model.Dtos;

namespace Dipchik.Controllers;

public static class ToursController
{
    public static IEndpointRouteBuilder AddToursController(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("Tours");

        group.MapPost("/GetAll", GetAllTours);
        group.MapGet("/GetById/{id}", GetTourById);

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

        if (filters.PrivateTour.HasValue)
        {
            query = query.Where(t => t.Tour.PrivateTour == filters.PrivateTour.Value);
        }

        if (filters.GroupTour.HasValue)
        {
            query = query.Where(t => t.Tour.GroupTour == filters.GroupTour.Value);
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
        if (filters.Status != TourInstanceStatus.None)
        {
            query = query.Where(i => i.Status == filters.Status);
        }

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
            PrivateTour = t.Tour.PrivateTour,
            GroupTour = t.Tour.GroupTour,
            SpecialOffers = t.Tour.SpecialOffers,
            DurationDays = t.Tour.DurationDays,
            GuideName = t.Tour.Guide.Name,
            GuideSurname = t.Tour.Guide.Surname,
            GuideAvatarUrl = t.Tour.Guide.Account.AvatarUrl,
            IsActive = t.Status == TourInstanceStatus.Scheduled,
            Status = t.Status
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
            CurrentParticipants = tour.CurrentParticipants,
            GuideName = tour.Tour.Guide.Name,
            GuideSurname = tour.Tour.Guide.Surname,
            GuideAvatarUrl = tour.Tour.Guide.Account.AvatarUrl
        }).FirstOrDefaultAsync(stoppingToken);

        if (tour == null)
        {
            return Results.NotFound($"Tour with ID {id} not found");
        }

        return Results.Ok(tour);
    }
} 