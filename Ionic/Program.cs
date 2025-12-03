using Metapsi;
using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Ionic;
using Metapsi.Syntax;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Metapsi.Web;
using System;

public class Customer
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

public class Services
{
    public IDbService Db { get; set; }
}

public static class Program
{

    public static async Task Main()
    {
        var builder = WebApplication.CreateSlimBuilder().AddMetapsi();
        builder.Services.AddJsModules(
            b =>
            {
                b.AddModule(new EditCustomerModal().ModulePath, () => new EditCustomerModal().Module);
                b.AddModule(new ViewCustomerPage().ModulePath, () => new ViewCustomerPage().Module);
                b.AddModule(new ListCustomersPage().ModulePath, () => new ListCustomersPage().Module);
            });
        var webApp = builder.Build().UseMetapsi();

        webApp.MapGet("/", async (HttpContext httpContext) =>
        {
            await httpContext.WriteHtmlDocumentResponse(HtmlBuilder.FromDefault(Homepage));
        });

        webApp.MapPost("/server-action", async (HttpContext context, ServerAction.Call serverCall) =>
        {
            try
            {
                return Results.Ok(
                    await serverCall.Run(
                        [
                        typeof(ListCustomersPage),
                        typeof(EditCustomerModal)
                        ],
                        new Services()
                        {
                            Db = new CookieDbService(context)
                        }));
            }
            catch(Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }).AllowAnonymous();

        await webApp.RunAsync();
    }

    public static void Homepage(this HtmlBuilder b)
    {
        b.HeadAppend(b.HtmlScriptModuleReference(new ListCustomersPage()));
        b.HeadAppend(b.HtmlScriptModuleReference(new ViewCustomerPage()));
        b.HeadAppend(b.HtmlScriptModuleReference(new EditCustomerModal()));
        b.HeadAppend(b.HtmlTitle("Metapsi CRM"));

        b.BodyAppend(
            b.Hyperapp<object>(
                b => b.MakeInit(
                    b.MakeStateWithEffects(
                        b.NewObj(),
                        b =>
                        {
                            b.RequestAnimationFrame(b =>
                            {
                                var nav = b.QuerySelector("ion-nav");
                                b.CallOnObject(
                                    nav,
                                    IonNav.Method.SetRoot,
                                    b.Const(new ListCustomersPage().Tag));
                            });
                        })),
                (b, model) => b.IonApp(b.IonNav())));
    }
}