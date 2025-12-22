using Metapsi;
using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Ionic;
using Metapsi.Syntax;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Threading.Tasks;

namespace MetapsiCRM;

public static partial class Homepage
{
    public class Model
    {
        public User User { get; set; } = new User();
        public string IonicMode { get; set; } = "md";
    }

    public static void MapHomepage(this IEndpointRouteBuilder endpoint, Func<HttpContext, Task<Services>> getServices)
    {
        endpoint.MapGet("/", async (HttpContext httpContext) =>
        {
            var model = await GetHomepageModel(await getServices(httpContext));

            var html = HtmlBuilder.FromDefault(
                b =>
                {
                    b.Render(model);
                });
            await httpContext.WriteHtmlDocumentResponse(html);
        });

        endpoint.MapGet("/get-model", async (HttpContext httpContext) =>
        {
            return await GetHomepageModel(await getServices(httpContext));
        });
    }

    public static async Task<Model> GetHomepageModel(this Services services)
    {
        var model = new Model()
        {
            User = services.User,
            IonicMode = services.UiMode
        };

        return model;
    }

    private static void Render(this HtmlBuilder b, Model model)
    {
        b.HeadAppend(b.HtmlScriptModuleReference(new ListCustomersPage()));
        b.HeadAppend(b.HtmlScriptModuleReference(new ViewCustomerPage()));
        b.HeadAppend(b.HtmlScriptModuleReference(new EditCustomerModal()));
        b.HeadAppend(b.HtmlScriptModuleReference(new OtpPhonePage()));
        b.HeadAppend(b.HtmlScriptModuleReference(new OtpCodePage()));
        b.HeadAppend(b.HtmlTitle("Metapsi CRM"));
        b.Document.Body.SetAttribute("mode", model.IonicMode);

        b.BodyAppend(
            b.Hyperapp<Model>(
                b =>
                {
                    var initModel = b.Const(model);
                    return b.MakeInit(
                        b.MakeStateWithEffects(
                            initModel,
                            b.SetNavRoot(initModel)));
                },
                (b, model) => b.IonApp(b.IonNav()),
                (b, model) =>
                {
                    return b.Listen(
                        b.Window(),
                        b.Const("reload-app"),
                        b.MakeAction((SyntaxBuilder b, Var<Model> model) =>
                        {
                            return b.MakeStateWithEffects(
                                model,
                                (b, dispatch) =>
                                {
                                    b.LocationReload(b.Get(b.Window(), x => x.location));
                                });
                        }));
                })
            );
    }

    private static Var<HyperType.Effect> SetNavRoot(this SyntaxBuilder b, Var<Model> model)
    {
        return b.If(
            b.HasObject(b.Get(model, x => x.User.Id)),
            b => b.IonNavSetRootEffect<ListCustomersPage>(),
            b => b.IonNavSetRootEffect<OtpPhonePage>());
    }
}
