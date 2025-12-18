using Metapsi;
using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Ionic;
using Metapsi.Syntax;
using Metapsi.Web;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

namespace MetapsiCRM;

public static partial class Program
{
    static async Task Main(string[] args)
    {
        var identityService = new IdentityService();

        await Setup(async httpContext =>
        {
            return new Services()
            {
                Customers = new CookieDbService(httpContext),
                Users = new IdentityService(),
                User = await httpContext.GetUser(identityService),
                UiMode = httpContext.UiMode()
            };
        }).RunAsync();
    }

    public static WebApplication Setup(Func<HttpContext, Task<Services>> getServices)
    {
        var builder = WebApplication.CreateBuilder().AddMetapsi();
        builder.Services.AddJsModules(
            b =>
            {
                if (Debugger.IsAttached)
                {
                    b.AddModule(new EditCustomerModal().ModulePath, () => new EditCustomerModal().Module);
                    b.AddModule(new ViewCustomerPage().ModulePath, () => new ViewCustomerPage().Module);
                    b.AddModule(new ListCustomersPage().ModulePath, () => new ListCustomersPage().Module);
                    b.AddModule(new OtpPhonePage().ModulePath, () => new OtpPhonePage().Module);
                    b.AddModule(new OtpCodePage().ModulePath, () => new OtpCodePage().Module);
                }
                else
                {
                    b.AutoRegisterFrom(typeof(Program).Assembly);
                }
            });

        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();

        var webApp = builder.Build().UseMetapsi();
        webApp.UseAuthentication();

        webApp.MapHomepage(getServices);
        
        webApp.MapPost("/server-action", async (HttpContext context, ServerAction.Call serverCall) =>
        {
            try
            {
                return Results.Ok(
                    await serverCall.Run(
                        [
                        typeof(Homepage),
                        typeof(ListCustomersPage),
                        typeof(EditCustomerModal)
                        ],
                        await getServices(context)));
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }).AllowAnonymous();

        webApp.MapOtpSignIn(getServices);

        return webApp;
    }

}
