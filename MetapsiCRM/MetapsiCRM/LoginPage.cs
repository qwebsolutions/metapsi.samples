using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Ionic;
using Metapsi.Syntax;

namespace MetapsiCRM;

public class LoginPage : CustomElement<LoginPage.Model>
{
    public class Model
    {
        public string Phone { get; set; } = "+1234567890";
    }

    public override Var<HyperType.StateWithEffects> OnInit(SyntaxBuilder b, Var<Element> element)
    {
        return b.MakeStateWithEffects(
            b.NewObj<Model>(),
            b.FocusIonInputEffect("id-otp-phone"));
    }


    public override IRootControl OnRender(LayoutBuilder b, Var<Model> model)
    {
        return this.Root(
            b.IonContent(
                b =>
                {
                    //b.AddClass("ion-padding");
                },
                b.HtmlDiv(
                    b =>
                    {

                        //b.AddStyle("background-color", "green");
                        b.FlexRow();
                        b.HeightFull();
                        b.AlignItemsCenter();
                        b.JustifyContentCenter();
                    },
                    b.HtmlDiv(
                        b=>
                        {
                            b.AddStyle("max-width", "500px");
                            //b.AddStyle("background-color", "red");
                        },
                    b.HtmlDiv(
                        b =>
                        {
                            b.FlexColumn();
                            //b.AddStyle("background-color", "green");
                            //b.AddStyle("gap", "2rem");
                            //b.AddStyle("padding", "10%");
                            //b.AlignItemsCenter();
                        },
                        b.WelcomeMessage(),
                        b.LoginPhoneImage(),
                        b.OtpCodeNote(),
                        //b.HtmlDiv(
                        //    b=>
                        //    {
                        //        b.AddStyle("padding", "30px 20px");
                        //    },
                        //    b.IonText(
                        //        b=>
                        //        {
                        //            b.SetColorMedium();
                        //        },
                        //        b.Text("We will not send a code to your phone, but pretend we did, so enter whatever here, it's just a sample"))),
                        b.HtmlDiv(
                            b=>
                            {
                                b.AddStyle("padding", "30px 30px 10px 30px");
                                b.FlexColumn();
                                b.AddStyle("gap", "10px");
                            },
                        b.IonList(
                            b =>
                            {
                                b.SetLinesFull();
                            },
                            b.IonItem(
                                b.IonInput(
                                    b =>
                                    {
                                        b.SetLabel("Phone");
                                        b.SetId("id-otp-phone");
                                        b.SetTypeTel();
                                        b.BindTo(model, x => x.Phone);
                                    })
                                //b.IonButton(
                                //    b =>
                                //    {
                                //        b.SetSlot(IonItem.Slot.End);
                                //        b.SetFillClear();
                                //    },
                                //    b.IonIcon(
                                //        b =>
                                //        {
                                //            b.SetName("send-outline");
                                //            b.SetSlot(IonButton.Slot.IconOnly);
                                //        }))
                                )),
                        b.IonButton(
                            b =>
                            {
                                b.OnClickAction(b.MakeAction((SyntaxBuilder b, Var<Model> model) =>
                                {
                                    return b.MakeStateWithEffects(
                                        model,
                                        b.IonNavPushEffect(
                                            b.Const(new OtpCodePage().Tag),
                                            b =>
                                            {
                                                b.SetProperty("phone", b.Get(model, x => x.Phone));
                                            }));
                                }));
                            },
                            b.Text("Continue"))))))));
    }


    //private Var<IVNode> WelcomeMessage(LayoutBuilder b)
    //{
    //    return b.HtmlDiv(
    //        b =>
    //        {
    //            b.FlexColumn();
    //        },
    //        b.HtmlH3(
    //            b.IonText(
    //                b =>
    //                {
    //                    b.SetColorMedium();
    //                },
    //                b.Text("Welcome to"))),
    //        b.HtmlH2(
    //            b.IonText(
    //                b =>
    //                {
    //                    b.SetColorDark();
    //                },
    //                b.Text("Metapsi CRM"))));
    //}
}

file static class LoginExtensions
{
    public static Var<IVNode> LoginPhoneImage(this LayoutBuilder b)
    {
        var otpPng = b.AddEmbeddedResourceMetadata(typeof(LoginExtensions).Assembly, "undraw_mobile-log-in_0n4q.png");

        return b.HtmlDiv(
            b =>
            {
                b.FlexRow();
                b.JustifyContentCenter();
                b.AddStyle("z-index", "-1");
            },
            b.HtmlImg(
                b =>
                {
                    b.AddStyle("width", "300px");
                    b.AddStyle("margin", "-75px 0px 0px 0px");
                    
                    b.SetSrc(otpPng.GetDefaultHttpPath());
                    //b.AddStyle("max-width", "300px");
                    //b.AddStyle("margin", "-20px");
                    //b.AddStyle("padding", "10%");
                    //b.AddStyle("background-color", "var(--ion-color-light)");
                    //b.AddStyle("border-radius", "20px");
                }));
    }

    public static Var<IVNode> OtpCodeNote(this LayoutBuilder b)
    {
        return b.HtmlDiv(
            b =>
            {
                b.AddStyle("padding", "30px 30px");
            },
            b.IonText(
                b =>
                {
                    b.SetColorMedium();
                },
                b.Text("We will not send a code to your phone, but pretend we did, so enter whatever here, it's just a sample")));
    }

    public static Var<IVNode> WelcomeMessage(this LayoutBuilder b)
    {
        return b.HtmlDiv(
            b =>
            {
                b.FlexColumn();
                b.AddStyle("padding", "30px");
            },
            b.HtmlH6(
                b.IonText(
                    b =>
                    {
                        b.SetColorMedium();
                    },
                    b.Text("Welcome to")
                    )
                ),
            b.HtmlH2(
                b.IonText(
                    b =>
                    {
                        b.SetColorDark();
                        //b.SetColorTertiary();
                        b.AddStyle("font-weight", "600");
                        b.AddStyle("letter-spacing", "-0.025em");
                    },
                    b.Text("Metapsi CRM"))));
    }
}