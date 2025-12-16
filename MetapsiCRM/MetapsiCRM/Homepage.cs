using Metapsi;
using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Ionic;
using Metapsi.Syntax;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetapsiCRM;

public static partial class Homepage
{
    public class Model
    {
        public User User { get; set; } = new User();
    }

    public static void MapHomepage(this IEndpointRouteBuilder endpoint, Func<HttpContext, Task<Services>> getServices)
    {
        endpoint.MapGet("/", async (HttpContext httpContext) =>
        {
            var services = await getServices(httpContext);

            var model = new Model()
            {
                User = services.User
            };

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
            User = services.User
        };

        return model;
    }

    private static void Render(this HtmlBuilder b, Model model)
    {
        b.HeadAppend(b.HtmlScriptModuleReference(new ListCustomersPage()));
        b.HeadAppend(b.HtmlScriptModuleReference(new ViewCustomerPage()));
        b.HeadAppend(b.HtmlScriptModuleReference(new EditCustomerModal()));
        b.HeadAppend(b.HtmlScriptModuleReference(new LoginPage()));
        b.HeadAppend(b.HtmlScriptModuleReference(new OtpCodePage()));
        b.HeadAppend(b.HtmlTitle("Metapsi CRM"));

        //b.HeadAppend(b.HtmlScriptModule(
        //    b =>
        //    {
        //        b.AddEventListener(b.Window(), b.Const("appload"), b.Def((SyntaxBuilder b) =>
        //        {
        //            b.Log("appload!");
        //        }));

        //        b.AddEventListener(b.Document(), b.Const("ionViewDidEnter"), b.Def((SyntaxBuilder b) =>
        //        {
        //            b.Log("ionViewDidEnter!");
        //        }));
        //    }));

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
                (b, model) => b.IonApp(b.IonNav(
                    b=>
                    {
                        //b.OnIonNavDidChange(
                        //    b.MakeAction((SyntaxBuilder b, Var<Model> model, Var<Event> e) =>
                        //    {
                        //        b.Log("OnIonNavDidChange", e);
                        //        return b.MakeStateWithEffects(
                        //            model,
                        //            b =>
                        //            {
                        //                var otpPhone = b.QuerySelector("#id-otp-phone");
                        //                b.Log(otpPhone);
                        //                b.CallOnObject(otpPhone, IonInput.Method.SetFocus);
                        //            });
                        //    }));
                    })),
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
                                    var ionNav = b.QuerySelector("ion-nav");
                                    b.If(
                                        b.HasObject(ionNav),
                                        b =>
                                        {
                                            var popPromise = b.CallOnObject<Promise>(ionNav, IonNav.Method.PopToRoot);
                                            b.PromiseThen(
                                                popPromise,
                                                (SyntaxBuilder b, Var<object> _) =>
                                                {
                                                    b.Dispatch(
                                                        dispatch, 
                                                        b.CallServerAction<Model, Services>(
                                                            b.Const("/server-action"),
                                                            async (Model model, Services services) =>
                                                            {
                                                                return await services.GetHomepageModel();
                                                            },
                                                            b.MakeAction((SyntaxBuilder b, Var<Model> oldModel, Var<Model> model) =>
                                                            {
                                                                return b.MakeStateWithEffects(
                                                                    model,
                                                                    b.SetNavRoot(model));
                                                            }),
                                                            b.AlertOnException<Model>()));
                                                });
                                        });
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
            b => b.IonNavSetRootEffect<LoginPage>());
    }
}
