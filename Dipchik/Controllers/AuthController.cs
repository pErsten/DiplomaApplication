﻿using Dipchik.Services;
using Shared.Model;

namespace Dipchik.Controllers;

public static class AuthController
{
    public static IEndpointRouteBuilder AddAuthController(this IEndpointRouteBuilder builder, params AccountRolesEnum[] roleRequirements)
    {
        var group = builder.MapGroup("Auth");
        foreach (var role in roleRequirements)
        {
            group.RequireAuthorization(role.ToString());
        }

        group.MapGet("/register", Register);
        group.MapGet("/login", Login);

        return builder;
    }
    public static async Task<IResult> Register(string login, string password, AuthService authService, JwtTokenGenerator tokenGenerator)
    {
        var result = await authService.AddNewUser(login, AuthService.PasswordHasher(password));
        if (!result.TryGetValue(out var account))
        {
            return Results.BadRequest(result.ErrorMessageCode);
        }

        return Results.Ok(tokenGenerator.GenerateJwt(account));
    }
    public static async Task<IResult> Login(string login, string password, AuthService authService, JwtTokenGenerator tokenGenerator)
    {
        var result = await authService.ValidateUser(login, AuthService.PasswordHasher(password));
        if (!result.TryGetValue(out var account))
        {
            return Results.BadRequest(result.ErrorMessageCode);
        }

        return Results.Ok(tokenGenerator.GenerateJwt(account));
    }
}