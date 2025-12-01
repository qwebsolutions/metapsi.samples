using Metapsi;
using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Ionic;
using Metapsi.Syntax;

public class EditCustomerModal : CustomElement<EditCustomerModal.Model, EditCustomerModal.Model>
{
    public class Model
    {
        public Customer Customer { get; set; } = new Customer();
        public string Title { get; set; } = "New customer";
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
                    b.IonTitle(b.Text(b.Get(model, x => x.Title))),
                    b.IonButtons(
                        b =>
                        {
                            b.SetSlot(IonToolbar.Slot.Primary);
                        },
                        b.IonButton(
                            b =>
                            {
                                b.SetDisabled(b.Not(b.HasValue(b.Get(customer, x => x.Name))));
                                b.SetFillSolid();
                                b.SetColorPrimary();
                                b.OnClickAction((SyntaxBuilder b, Var<Model> model) =>
                                {
                                    return b.MakeStateWithEffects(
                                        model,
                                        (b, dispatch) =>
                                        {
                                            b.Dispatch(
                                                dispatch,
                                                b.CallServerAction(
                                                    b.Const("/server-action"),
                                                    async (Model model, Services services) =>
                                                    {
                                                        await services.Db.SaveCustomer(model.Customer);
                                                        return model;
                                                    },
                                                    b.MakeAction((SyntaxBuilder b, Var<Model> _oldModel, Var<Model> model) =>
                                                    {
                                                        return b.MakeStateWithEffects(
                                                            model,
                                                            b => b.ModalControllerDismiss(b.Get(model, x => x.Customer)),
                                                            b => b.ToastControllerPresent(b =>
                                                            {
                                                                b.Set(x => x.message, "Customer saved");
                                                                b.Set(x => x.swipeGesture, "vertical");
                                                                b.Set(x => x.duration, 5000);
                                                                b.Set(x => x.color, "success");
                                                            }),
                                                            b => b.DispatchEvent(b.Window(), b.Const(ListCustomersPage.ReloadCustomersEventName)));
                                                    }),
                                                    b.MakeAction((SyntaxBuilder b, Var<Model> model, Var<Error> error) =>
                                                    {
                                                        return b.MakeStateWithEffects(
                                                            model,
                                                            b => b.ToastControllerPresent(b =>
                                                            {
                                                                b.Set(x => x.message, b.Get(error, x => x.message));
                                                                b.Set(x => x.swipeGesture, "vertical");
                                                                b.Set(x => x.duration, 5000);
                                                                b.Set(x => x.color, "danger");
                                                            }));
                                                    })));
                                        });
                                });
                            },
                            b.Text("Save"))))),
                b.IonContent(
                    b.IonList(
                        b.IonItem(
                            b.IonInput(
                                b =>
                                {
                                    b.SetLabel("Name");
                                    b.BindTo(customer, x => x.Name);
                                })),
                        b.IonItem(
                            b.IonInput(
                                b =>
                                {
                                    b.SetLabel("Email");
                                    b.BindTo(customer, x => x.Email);
                                })),
                        b.IonItem(
                            b.IonInput(
                                b =>
                                {
                                    b.SetLabel("Phone");
                                    b.BindTo(customer, x => x.Phone);
                                })),
                        b.IonItem(
                            b.IonTextarea(
                                b =>
                                {
                                    b.SetRows(10);
                                    b.SetLabel("Notes");
                                    b.BindTo(customer, x => x.Notes);
                                }))
                        )));
    }
}
