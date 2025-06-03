using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using kulunvalvonta.Data;
using Kulunvalvonta.Shared;

namespace kulunvalvonta.Endpoints
{

    public static class AdminEndpoints
    {
        public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
        {
            // Vaadi Admin rooli kaikille reiteille
            var admin = app
                .MapGroup("/api/admin")
                .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

            // Hae käyttäjät
            admin.MapGet("/users",
                async (UserManager<ApplicationUser> userManager) =>
                {
                    var allUsers = await userManager.Users.ToListAsync();
                    var list = new List<UserDto>(allUsers.Count);
                    foreach (var u in allUsers)
                    {
                        var roles = await userManager.GetRolesAsync(u);
                        list.Add(new UserDto
                        {
                            Id = u.Id,
                            Email = u.Email ?? string.Empty,
                            Roles = roles
                        });
                    }
                    return Results.Ok(list);
                });

            // Poista käyttäjä id:n perusteella
            admin.MapDelete("/users/{id}",
                async (string id, UserManager<ApplicationUser> userManager) =>
                {
                    var user = await userManager.FindByIdAsync(id);
                    if (user is null)
                        return Results.NotFound();

                    var result = await userManager.DeleteAsync(user);
                    if (!result.Succeeded)
                        return Results.BadRequest(result.Errors);

                    return Results.NoContent();
                });

            return app;
        }
    }
}
