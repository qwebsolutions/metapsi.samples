using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Ionic;
using Metapsi.Syntax;

public static class CustomerExtensions
{
    public static Var<IVNode> CustomerDetails(this LayoutBuilder b, Var<Customer> customer, Var<bool> disabled)
    {
        return b.IonList(
            b.IonItem(
                b.IonInput(
                    b =>
                    {
                        b.SetDisabled(disabled);
                        b.SetLabel("Name");
                        b.BindTo(customer, x => x.Name);
                    })),
            b.IonItem(
                b.IonInput(
                    b =>
                    {
                        b.SetDisabled(disabled);
                        b.SetLabel("Email");
                        b.BindTo(customer, x => x.Email);
                    })),
            b.IonItem(
                b.IonInput(
                    b =>
                    {
                        b.SetDisabled(disabled);
                        b.SetLabel("Phone");
                        b.BindTo(customer, x => x.Phone);
                    })));
    }
}
