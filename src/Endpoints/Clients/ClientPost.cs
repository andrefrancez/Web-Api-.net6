﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using WebApiUdemy.Domain.Users;
using WebApiUdemy.Infra.Data;

namespace WebApiUdemy.Endpoints.Clients;

public static class ClientPost {
    public static string Template => "/clients";
    public static string[] Methods => new string[] {HttpMethod.Post.ToString()};
    public static Delegate Handle => Action;

    [AllowAnonymous]
    public static async Task<IResult> Action(ClientRequest clientRequest, UserCreator userCreator) {
        var userClaims = new List<Claim> {
             new Claim("Cpf", clientRequest.Cpf),
             new Claim("Name", clientRequest.Name)
        };

        (IdentityResult identity , string userId) result = await userCreator.Create(clientRequest.Email, clientRequest.Password, userClaims);

        if(!result.identity.Succeeded)
            return Results.ValidationProblem(result.identity.Errors.ConvertToProblemDetails());

        return Results.Created($"/clients/{result.userId}", result.userId);
    }
}
