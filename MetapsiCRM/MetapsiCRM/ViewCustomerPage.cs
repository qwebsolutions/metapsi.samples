using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Ionic;
using Metapsi.Syntax;

namespace MetapsiCRM;

public class ViewCustomerPage : CustomElement<ViewCustomerPage.Model, ViewCustomerPage.Model>
{
    public class Model
    {
        public Customer Customer { get; set; } = new Customer();
    }

    public override Var<HyperType.StateWithEffects> OnInitProps(SyntaxBuilder b, Var<Model> props)
    {
        return b.MakeStateWithEffects(props);
    }

    public override IRootControl OnRender(LayoutBuilder b, Var<Model> model)
    {
        var customer = b.Get(model, x => x.Customer);

        return this.Root(
            b.IonHeader(
                b.IonToolbar(
                    b.IonButtons(b.IonBackButton()),
                    b.IonButtons(
                        b=>
                        {
                            b.SetSlot(IonToolbar.Slot.Primary);
                        },
                        b.IonButton(
                            b=>
                            {
                                b.OnClickAction((SyntaxBuilder b, Var<Model> model) =>
                                {
                                    var editedCustomer = b.Clone(customer);
                                    return b.MakeStateWithEffects(
                                        model,
                                        b.ModalControllerPresentEffect<EditCustomerModal>(
                                            b =>
                                            {
                                                b.SetInitProps(b.NewObj<EditCustomerModal.Model>(
                                                    b =>
                                                    {
                                                        b.Set(x => x.Customer, editedCustomer);
                                                        b.Set(x => x.Title, "Edit customer");
                                                    }));

                                                b.Set(x => x.breakpoints, [0, 1]);
                                                b.Set(x => x.initialBreakpoint, 1);
                                            },
                                            (b, dispatch, result) =>
                                            {
                                                var data = b.Get(result, x => x.data);
                                                b.If(
                                                    b.HasObject(data),
                                                    b =>
                                                    {
                                                        b.Dispatch(dispatch, b.MakeAction((SyntaxBuilder b, Var<Model> model) =>
                                                        {
                                                            b.Set(model, x => x.Customer, data.As<Customer>());
                                                            return b.Clone(model);
                                                        }));
                                                    });
                                            }));
                                });
                            },
                            b.Text("Edit")))
                    )
                ),
            b.IonContent(
                b.IonList(
                    b.IonItem(
                        b.IonLabel(b.Text("Name")),
                        b.IonText(b.Text(b.Get(customer, x=>x.Name)))),
                    b.IonItem(
                        b.IonLabel(b.Text("Email")),
                        b.IonText(b.Text(b.Get(customer, x => x.Email)))),
                    b.IonItem(
                        b.IonLabel(b.Text("Phone")),
                        b.IonText(b.Text(b.Get(customer, x => x.Phone)))),
                    b.IonItem(
                        b.IonLabel(b.Text("Notes")),
                        b.IonText(b.Text(b.Get(customer, x => x.Notes))))
                    )),
            b.IonFooter());
    }
}
