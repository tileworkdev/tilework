using Microsoft.AspNetCore.Identity;

using Tilework.IdentityManagement.Services;
using Tilework.Persistence.IdentityManagement.Models;

namespace Tilework.Ui.Api;

public static class AuthEndpoints
{
    public static WebApplication MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/api/auth/login", async (HttpRequest httpRequest,
                                              UserService userService,
                                              SignInManager<User> signInManager) =>
            {
                if (!httpRequest.HasFormContentType)
                {
                    return Results.BadRequest();
                }

                var form = await httpRequest.ReadFormAsync();
                var userNameOrEmail = form["UserNameOrEmail"].ToString();
                var password = form["Password"].ToString();

                if (string.IsNullOrWhiteSpace(userNameOrEmail) ||
                    string.IsNullOrWhiteSpace(password))
                {
                    return Results.Redirect("/login?error=invalid");
                }

                var user = await userService.GetUserByLogin(userNameOrEmail);
                if (user == null)
                {
                    return Results.Redirect("/login?error=invalid");
                }

                var result = await signInManager.PasswordSignInAsync(user, password,
                    isPersistent: false, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    return Results.Redirect("/");
                }

                return Results.Redirect("/login?error=invalid");
            })
            .AllowAnonymous();

        app.MapPost("/api/auth/logout", async (SignInManager<User> signInManager) =>
            {
                await signInManager.SignOutAsync();
                return Results.Redirect("/login");
            });

        return app;
    }
}
