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
            return await serverCall.Run(
                [
                typeof(ListCustomersPage),
                typeof(EditCustomerModal)
                ],
                new Services()
                {
                    Db = new CookieDbService(context)
                });
        }).AllowAnonymous();

        await webApp.RunAsync();
    }

    public static void Homepage(this HtmlBuilder b)
    {
        b.HeadAppend(b.HtmlScriptModuleReference(new ListCustomersPage()));
        b.HeadAppend(b.HtmlScriptModuleReference(new ViewCustomerPage()));
        b.HeadAppend(b.HtmlScriptModuleReference(new EditCustomerModal()));
        b.HeadAppend(b.HtmlTitle("Ionic quick-start sample"));
        //b.BodyAppend(
        //    b.Hyperapp(
        //        new Model(),
        //        (b, model) => b.App(model)));

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

    public static Var<HyperType.Effect> ModalControllerPresentEffect<T>(
        this SyntaxBuilder b,
        System.Action<PropsBuilder<ModalOptions>> setOptions,
        System.Action<SyntaxBuilder, Var<HyperType.Dispatcher>, Var<OverlayEventDetail>> then)
        where T : ICustomElement, new()
    {
        return b.MakeEffect(
            (b, dispatch) =>
            {
                var modalOptions = b.SetProps<ModalOptions>(
                    b.NewObj(),
                    b =>
                    {
                        b.Set(x => x.component, new T().Tag);
                        b.AddProps(setOptions);
                    });
                var createPromise = b.ModalControllerCreate(modalOptions);
                b.PromiseThen(createPromise, (SyntaxBuilder b, Var<object> modal) =>
                {
                    b.PromiseThen(
                        b.CallOnObject<Promise>(modal, IonModal.Method.OnDidDismiss),
                        (SyntaxBuilder b, Var<OverlayEventDetail> result) =>
                        {
                            b.Call(then, dispatch, result);
                        });
                    b.CallOnObject(modal, IonModal.Method.Present);
                });
            });
    }

    public static Var<HyperType.Effect> ModalControllerPresentEffect<T>(
        this SyntaxBuilder b,
        System.Action<PropsBuilder<ModalOptions>> setOptions)
        where T : ICustomElement, new()
    {
        return b.MakeEffect(
            b =>
            {
                b.ModalControllerPresent(
                    b =>
                    {
                        b.Set(x => x.component, new T().Tag);
                        b.AddProps(setOptions);
                    });
            });
    }

    public static void SetInitProps(this PropsBuilder<ModalOptions> b, IVariable initProps)
    {
        var existingProps = b.Get(x => x.componentProps);
        var propsReference = b.If(
            b.HasObject(existingProps),
            b => existingProps,
            b =>
            {
                b.Set(x => x.componentProps, b.NewObj<object>());
                return b.Get(x => x.componentProps);
            });

        b.SetProperty(propsReference, "props", initProps);
    }

    //public static Var<IVNode> App(this LayoutBuilder b, Var<Model> model)
    //{
    //    return b.IonApp(
            
    //        );
    //}
}