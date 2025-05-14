using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.DbContexts;
using Shared.Model;
using Shared.Model.Dtos;

namespace Backend.Services;

public class GuideService
{
    private readonly SqlContext dbContext;
    private readonly ILogger<GuideService> logger;

    public GuideService(ILoggerFactory loggerFactory, SqlContext dbContext)
    {
        logger = loggerFactory.CreateLogger<GuideService>();
        this.dbContext = dbContext;
    }

    public async Task<OperationResult<List<TourDto>>> GetGuideTours(string accountId, CancellationToken stoppingToken)
    {
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

        return new OperationResult<List<TourDto>>(result);
    }

    public async Task<OperationResult<List<TourDescDto>>> GetGuideTourInstances(string accountId, CancellationToken stoppingToken)
    {
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
            CurrentParticipants = tour.Bookings.Count,
            GuideName = tour.Tour.Guide.Name,
            GuideSurname = tour.Tour.Guide.Surname,
            GuideAvatarUrl = tour.Tour.Guide.Account.AvatarUrl,
            IsCancelled = tour.IsCancelled,
            Classification = tour.Tour.Classification
        }).ToListAsync(stoppingToken);

        return new OperationResult<List<TourDescDto>>(result);
    }
}