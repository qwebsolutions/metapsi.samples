using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MetapsiCRM;

public class SignInInput
{
    public string Phone { get; set; }
    public string RequestCode { get; set; }
    public string AccessCode { get; set; }
}

public class SignInOutput
{
    public bool Success { get; set; }
    public string Message { get; set; }
}


public static class OtpSignInApi
{
    public static void MapOtpSignIn(this IEndpointRouteBuilder endpoint, Func<HttpContext, Task<Services>> getServices)
    {
        endpoint.MapPost("/sign-in", async (HttpContext httpContext, SignInInput signInInput) =>
        {
            if (string.IsNullOrWhiteSpace(signInInput.Phone))
            {
                return new SignInOutput()
                {
                    Success = false,
                    Message = "Failed to sign in!"
                };
            }

            var user = await (await getServices(httpContext)).Users.GetUserByPhone(signInInput.Phone);
            if (user == null)
            {
                return new SignInOutput()
                {
                    Success = false,
                    Message = "Failed to sign in!"
                };
            }

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(
                    new ClaimsIdentity(
                        [
                        new Claim(ClaimTypes.NameIdentifier, user.Id),
                        ],
                        CookieAuthenticationDefaults.AuthenticationScheme)));

            return new SignInOutput()
            {
                Success = true,
                Message = $"Welcome {signInInput.Phone}!"
            };
        });

        endpoint.MapPost("/sign-out", async (HttpContext httpContext) =>
        {
            await httpContext.SignOutAsync();
        });
    }
}