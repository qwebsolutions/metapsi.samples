using Metapsi;
using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Ionic;
using Metapsi.Syntax;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

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


public class OtpCodePage: CustomElement<OtpCodePage.Model>
{
    public class Model
    {
        public string Phone { get; set; } = string.Empty;
        public string Code { get; set; } = "1234";
    }

    public override Var<HyperType.StateWithEffects> OnInit(SyntaxBuilder b, Var<Element> element)
    {
        b.Log("element", element);
        var phone = b.GetProperty<string>(element, b.Const("phone"));
        b.Log("phone", phone);
        return b.MakeStateWithEffects(b.NewObj<Model>(
            b =>
            {
                b.Set(x => x.Phone, phone);
            }),
            b.FocusIonInputEffect("id-otp-code"));
    }

    public override IRootControl OnRender(LayoutBuilder b, Var<Model> model)
    {
        return this.Root(
            b.IonHeader(
                b.IonToolbar(b.IonButtons(b.IonBackButton()))),
            b.IonContent(
                b =>
                {
                    b.AddClass("ion-padding");
                },
                b.HtmlDiv(
                    b =>
                    {
                        b.FlexColumn();
                        b.AlignItemsCenter();
                        b.JustifyContentCenter();
                        //b.HeightFull();
                    },
                    b.HtmlDiv(
                        b =>
                        {
                            b.FlexColumn();
                        },
                    b.IonText(
                        b =>
                        {
                            b.AddClass("ion-text-center");
                            b.SetColorMedium();
                        },
                        b.Text("Enter the access code you received through SMS.. or WhatsApp .. or maybe mail?")),
                    b.IonInputOtp(
                        b =>
                        {
                            b.SetId("id-otp-code");
                            b.SetValue(b.Get(model, x => x.Code));
                        }),
                    b.IonButton(
                        b=>
                        {
                            b.OnClickAction(b.MakeAction((SyntaxBuilder b, Var<Model> model) =>
                            {
                                return b.MakeStateWithEffects(
                                    model,
                                    b.PostJsonEffect<Model, SignInInput, SignInOutput>(
                                        b.Const("/sign-in"),
                                        b.NewObj<SignInInput>(
                                            b =>
                                            {
                                                b.Set(x => x.Phone, b.Get(model, x => x.Phone));
                                                b.Set(x => x.AccessCode, b.Get(model, x => x.Code));
                                            }),
                                        b.MakeAction((SyntaxBuilder b, Var<Model> model, Var<SignInOutput> output) =>
                                        {
                                            return b.MakeStateWithEffects(
                                                model,
                                                b => b.DispatchEvent(b.Window(), b.Const("reload-app")));
                                        }),
                                        b.AlertOnException<Model>()));
                            }));
                        },
                        b.Text("Access"))))));
    }
}

public static class OtpSignInExtensions
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