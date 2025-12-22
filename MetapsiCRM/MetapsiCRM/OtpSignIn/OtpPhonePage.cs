using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Ionic;
using Metapsi.Syntax;

namespace MetapsiCRM;

/// <summary>
/// The screen for 'one time password' phone input
/// </summary>
/// <remarks> It's a custom element. Ionic uses these for navigation between different application screens.</remarks>
public class OtpPhonePage : CustomElement<OtpPhonePage.Model>
{
    // This custom element is written in a reusability-centric way, so it can be added to any html page like this:
    // <otp-phone-page />
    // Changing this name does not break anything, because the custom element is referenced through the class itself, not the tag.
    // Find references and notice it is added like this: b.HtmlScriptModuleReference(new OtpPhonePage())
    // and used like this: b.IonNavSetRootEffect<OtpPhonePage>()
    // This keeps the tag usage in sync with the tag definition
    public OtpPhonePage()
    {
        // The tag name is explicitly declared in the constructor. This avoids any ambiguity in case you want to add it to a page
        this.Tag = "otp-phone-page";
    }

    /// <summary>
    /// The model for the client-side app running inside the custom element
    /// </summary>
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
                },
                b.HtmlDiv(
                    b =>
                    {
                        b.FlexRow();
                        b.HeightFull();
                        b.AlignItemsCenter();
                        b.JustifyContentCenter();
                    },
                    b.HtmlDiv(
                        b=>
                        {
                            b.AddStyle("max-width", "500px");
                        },
                    b.HtmlDiv(
                        b =>
                        {
                            b.FlexColumn();
                        },
                        b.WelcomeMessage(),
                        b.LoginPhoneImage(),
                        b.OtpCodeNote(),
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
}

file static class LoginExtensions
{
    public static Var<IVNode> LoginPhoneImage(this LayoutBuilder b)
    {
        // https://undraw.co
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