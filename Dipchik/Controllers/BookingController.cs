using Backend.Services;
using Dipchik.Services;
using Shared.Model;

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

    public static async Task<IResult> BookTour(int id, AuthService authService, BookingService bookingService, CancellationToken stoppingToken)
    {
        var accountResult = await authService.GetAccount(stoppingToken);
        if (!accountResult.TryGetValue(out var account))
        {
            return Results.BadRequest(accountResult.ErrorMessageCode);
        }

        var result = await bookingService.BookTour(account, id, stoppingToken);
        if (!result.IsSuccess)
        {
            return Results.BadRequest(result.ErrorMessageCode);
        }
        return Results.Ok();
    }

    public static async Task<IResult> GetMyBookings(AuthService authService, BookingService bookingService, CancellationToken stoppingToken)
    {
        var accountResult = await authService.GetAccount(stoppingToken);
        if (!accountResult.TryGetValue(out var account))
        {
            return Results.BadRequest(accountResult.ErrorMessageCode);
        }

        var result = await bookingService.GetMyBookings(account, stoppingToken);
        if (!result.TryGetValue(out var bookings))
        {
            return Results.BadRequest(result.ErrorMessageCode);
        }

        return Results.Ok(bookings);
    }

    public static async Task<IResult> CancelBooking(int id, AuthService authService, BookingService bookingService, CancellationToken stoppingToken)
    {
        var accountResult = await authService.GetAccount(stoppingToken);
        if (!accountResult.TryGetValue(out var account))
        {
            return Results.BadRequest(accountResult.ErrorMessageCode);
        }

        var result = await bookingService.CancelBooking(account, id, stoppingToken);
        if (!result.IsSuccess)
        {
            return Results.BadRequest(result.ErrorMessageCode);
        }

        return Results.Ok();
    }
} 