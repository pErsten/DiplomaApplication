using Common.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.DbContexts;
using Shared.Model;
using Shared.Model.Dtos;

namespace Dipchik.Controllers;

public static class BookingController
{
    public static IEndpointRouteBuilder AddBookingController(this IEndpointRouteBuilder builder, params AccountRolesEnum[] roleRequirements)
    {
        var group = builder.MapGroup("Booking");
        foreach (var role in roleRequirements)
        {
            group.RequireAuthorization(role.ToString());
        }

        group.MapGet("/BookTour", BookTour);
        group.MapGet("/GetMyBookings", GetMyBookings);
        group.MapGet("/CancelBooking", CancelBooking);

        return builder;
    }

    public static async Task<IResult> BookTour(int id, SqlContext dbContext, HttpContext context, CancellationToken stoppingToken)
    {
        var accountId = context.UserId();
        var account = await dbContext.Accounts.FirstOrDefaultAsync(x => x.AccountId == accountId, stoppingToken);
        if (account is null)
        {
            return Results.BadRequest("Account not found");
        }

        var tour = await dbContext.TourInstances
            .Where(x => x.Id == id)
            .Select(x => new { x.IsCancelled, x.MaxParticipants, x.Tour.Price, x.EndDate })
            .FirstOrDefaultAsync(stoppingToken);
        if (tour is null)
        {
            return Results.BadRequest("Tour not found");
        }
        if (TourInstance.GetStatus(tour.IsCancelled, tour.EndDate) != TourInstanceStatus.Scheduled)
        {
            return Results.BadRequest("Tour is not available for booking");
        }

        var bookings = await dbContext.TourBookings
            .Where(x => x.TourInstanceId == id && x.AccountId == account.Id && x.CancellationDate == null)
            .Select(x => new
            {
                x.Id, x.AccountId
            })
            .ToListAsync(stoppingToken);
        if (bookings.Count >= tour.MaxParticipants)
        {
            return Results.BadRequest("Tour is already fully booked");
        }
        if (bookings.Any(x => x.AccountId == account.Id))
        {
            return Results.BadRequest("You have already booked this tour");
        }

        var booking = new TourBooking
        {
            TourInstanceId = id,
            AccountId = account.Id,
            BookedUtc = DateTime.UtcNow,
            TotalPrice = tour.Price
        };

        await dbContext.TourBookings.AddAsync(booking, stoppingToken);
        await dbContext.SaveChangesAsync(stoppingToken);

        return Results.Ok();
    }

    public static async Task<IResult> GetMyBookings(
        SqlContext dbContext,
        HttpContext context,
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

        var bookings = await dbContext.TourBookings
            .Where(x => x.AccountId == account.Id)
            .Include(x => x.TourInstance)
            .ThenInclude(x => x.Tour)
            .Include(x => x.TourInstance)
            .ThenInclude(x => x.Rates)
            .OrderByDescending(x => x.BookedUtc)
            .Select(x => new BookingDto
            {
                Id = x.Id,
                TourInstanceId = x.TourInstanceId,
                TourTitle = x.TourInstance.Tour.Title,
                StartDate = x.TourInstance.StartDate,
                EndDate = x.TourInstance.EndDate,
                TotalPrice = x.TotalPrice,
                IsCancelled = x.IsCancelled,
                HasRated = x.TourInstance.Rates.Any(r => r.TouristAccountId == account.Id)
            })
            .ToListAsync(stoppingToken);

        return Results.Ok(bookings);
    }

    public static async Task<IResult> CancelBooking(
        int id,
        SqlContext dbContext,
        HttpContext context,
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

        var booking = await dbContext.TourBookings
            .Include(x => x.TourInstance)
            .FirstOrDefaultAsync(x => x.Id == id && x.AccountId == account.Id, stoppingToken);

        if (booking is null)
        {
            return Results.BadRequest("Booking not found");
        }

        if (booking.IsCancelled)
        {
            return Results.BadRequest("Booking is already cancelled");
        }

        if (booking.TourInstance.StartDate <= DateTime.UtcNow.AddDays(1))
        {
            return Results.BadRequest("Cannot cancel booking less than 24 hours before the tour");
        }

        booking.CancellationDate = DateTime.UtcNow;
        booking.CancellationReason = "Cancelled by user";

        await dbContext.SaveChangesAsync(stoppingToken);

        return Results.Ok();
    }
} 