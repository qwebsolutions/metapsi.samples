using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Ionic;
using Metapsi.Syntax;

namespace MetapsiCRM;

public static class HyperappExtensions
{
    public static Var<HyperType.Effect> FocusIonInputEffect(this SyntaxBuilder b, string id)
    {
        return b.MakeEffect(
            b =>
            {
                b.SetTimeout(b.Def((SyntaxBuilder b) =>
                {
                    var ionInput = b.QuerySelector("#" + id);
                    b.If(
                        b.HasObject(ionInput),
                        b =>
                        {
                            b.CallOnObject(ionInput, IonInput.Method.SetFocus);
                        });
                }),
                b.Const(500));
            });
    }
}
