using Microsoft.AspNetCore.Mvc;
using Shared.Model;
using Shared.Model.Dtos;
using Backend.Services;
using Dipchik.Services;

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
        group.MapPost("/RateTour", RateTour);

        return builder;
    }
    
    public static async Task<IResult> GetAllTours(
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromBody] TourFiltersDto filters,
        ToursService toursService,
        CancellationToken stoppingToken)
    {
        var result = await toursService.GetAllTours(page, pageSize, filters, stoppingToken);
        if (!result.TryGetValue(out var tours))
        {
            return Results.BadRequest(result.ErrorMessageCode);
        }

        return Results.Ok(tours);
    }

    public static async Task<IResult> GetTourById(
        int id,
        ToursService toursService,
        CancellationToken stoppingToken)
    {

        var result = await toursService.GetTourById(id, stoppingToken);
        if (!result.TryGetValue(out var tours))
        {
            return Results.BadRequest(result.ErrorMessageCode);
        }

        return Results.Ok(tours);
    }

    public static async Task<IResult> GetTourRecommendations(
        AuthService authService,
        ToursService toursService,
        CancellationToken stoppingToken)
    {
        var accountResult = await authService.GetAccount(stoppingToken);
        if (!accountResult.TryGetValue(out var account))
        {
            return Results.BadRequest(accountResult.ErrorMessageCode);
        }

        var result = await toursService.GetTourRecommendations(account, stoppingToken);
        if (!result.TryGetValue(out var recommendations))
        {
            return Results.BadRequest(result.ErrorMessageCode);
        }

        return Results.Ok(recommendations);
    }

    public static async Task<IResult> RateTour(
        AuthService authService,
        ToursService toursService,
        [FromBody] TourRateDto request,
        CancellationToken stoppingToken)
    {
        var accountResult = await authService.GetAccount(stoppingToken);
        if (!accountResult.TryGetValue(out var account))
        {
            return Results.BadRequest(accountResult.ErrorMessageCode);
        }

        var result = await toursService.RateTour(account, request, stoppingToken);
        if (!result.IsSuccess)
        {
            return Results.BadRequest(result.ErrorMessageCode);
        }

        return Results.Ok();
    }
} 