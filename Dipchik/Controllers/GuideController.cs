using Common.Model.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.DbContexts;
using Shared.Model;
using Shared.Model.Dtos;

namespace Dipchik.Controllers;

public static class GuideController
{
    public static IEndpointRouteBuilder AddGuideController(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("Guide");

        group.MapGet("GetTours", GetGuideTours);
        group.MapGet("GetTourInstances", GetGuideTourInstances);

        return builder;
    }

    public static async Task<IResult> GetGuideTours(
        SqlContext dbContext,
        HttpContext context,
        CancellationToken stoppingToken)
    {
        var accountId = context.UserId();
        if (accountId == null)
        {
            return Results.Unauthorized();
        }
        var guideId = await dbContext.Guides
            .Where(x => x.Account.AccountId == accountId)
            .Select(x => x.Id)
            .FirstOrDefaultAsync(stoppingToken);

        var tours = dbContext.TourInstances
            .Include(x => x.Tour)
            .ThenInclude(x => x.Guide)
            .ThenInclude(x => x.Account)
            .Where(x => x.Tour.GuideId == guideId)
            .AsQueryable();
        

        var result = await tours.Select(t => new TourDto
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
        }).ToListAsync(stoppingToken);

        return Results.Ok(result);
    }

    public static async Task<IResult> GetGuideTourInstances(
        SqlContext dbContext,
        HttpContext context,
        CancellationToken stoppingToken)
    {
        var accountId = context.UserId();
        if (accountId == null)
        {
            return Results.Unauthorized();
        }
        var guideId = await dbContext.Guides
            .Where(x => x.Account.AccountId == accountId)
            .Select(x => x.Id)
            .FirstOrDefaultAsync(stoppingToken);

        var tours = dbContext.TourInstances
            .Include(x => x.Tour)
            .ThenInclude(x => x.Guide)
            .ThenInclude(x => x.Account)
            .Where(x => x.Tour.GuideId == guideId)
            .AsQueryable();
        

        var result = await tours.Select(tour => new TourDescDto
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
            CurrentParticipants = tour.CurrentParticipants,
            GuideName = tour.Tour.Guide.Name,
            GuideSurname = tour.Tour.Guide.Surname,
            GuideAvatarUrl = tour.Tour.Guide.Account.AvatarUrl,
            Status = tour.Status,
            Classification = tour.Tour.Classification
        }).ToListAsync(stoppingToken);

        return Results.Ok(result);
    }
} 