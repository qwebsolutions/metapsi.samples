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
    }

    public override Var<HyperType.StateWithEffects> OnInit(SyntaxBuilder b, Var<Element> element)
    {
        var phone = b.GetProperty<string>(element, b.Const("phone"));
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
