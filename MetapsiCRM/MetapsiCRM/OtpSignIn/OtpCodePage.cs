using Metapsi;
using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Ionic;
using Metapsi.Syntax;
using Microsoft.AspNetCore.Http;

namespace MetapsiCRM;


public class OtpCodePage: CustomElement<OtpCodePage.Model>
{
    public class Model
    {
        public string Phone { get; set; } = string.Empty;
        public string Code { get; set; } = "1234";
        public string AccessNote { get; set; } = "Enter the access code you received through SMS.. or WhatsApp .. or maybe mail?";
        public string SignInUrl { get; set; } = "/sign-in";
    }

    public override Var<HyperType.StateWithEffects> OnInit(SyntaxBuilder b, Var<Element> element)
    {
        var phone = b.GetProperty<string>(element, b.Const("phone"));
        var signInUrl = b.GetProperty<string>(element, "sign-in-url");

        return b.MakeStateWithEffects(b.NewObj<Model>(
            b =>
            {
                b.Set(x => x.Phone, phone);
                b.If(b.HasValue(signInUrl), b => b.Set(x => x.SignInUrl, signInUrl));
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
                        b.Text(b.Get(model, x => x.AccessNote))),
                    b.IonInputOtp(
                        b =>
                        {
                            b.SetId("id-otp-code");
                            b.BindTo(model, x => x.Code);
                        }),
                    b.IonButton(
                        b =>
                        {
                            b.OnClickAction(b.MakeAction((SyntaxBuilder b, Var<Model> model) =>
                            {
                                return b.MakeStateWithEffects(
                                    model,
                                    b.PostJsonEffect<Model, SignInInput, SignInOutput>(
                                        b.Get(model, x => x.SignInUrl),
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
