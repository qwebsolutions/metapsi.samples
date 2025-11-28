using Metapsi;
using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Ionic;
using Metapsi.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

public class ListCustomersPage: CustomElement<ListCustomersPage.Model>
{
    public const string ReloadCustomersEventName = "reload-customers";

    public class Model
    {
        public List<Customer> Customers { get; set; } = new List<Customer>();
    }

    public override Var<HyperType.StateWithEffects> OnInit(SyntaxBuilder b, Var<Element> element)
    {
        return b.MakeStateWithEffects(
            b.Const(new Model()),
            (b, dispatch) =>
            {
                b.Dispatch(
                    dispatch,
                    b.CallServerAction(
                        b.Const("/server-action"),
                        async (Model model, Services services) =>
                        {
                            return new Model()
                            {
                                Customers = (await services.Db.GetCustomers()).ToList()
                            };
                        }));
            });
    }

    public override void OnSubscribe(SyntaxBuilder b, Var<Model> model, Var<List<HyperType.Subscription>> subscriptions)
    {
        b.Push(
            subscriptions,
            b.Listen(
                b.Window(),
                b.Const(ReloadCustomersEventName),
                b.CallServerAction(
                    b.Const("/server-action"),
                    async (Model model, Services services) =>
                    {
                        return new Model()
                        {
                            Customers = (await services.Db.GetCustomers()).ToList()
                        };
                    })));
    }

    public override IRootControl OnRender(LayoutBuilder b, Var<Model> model)
    {
        return this.Root(b.IonHeader(
                b.IonToolbar(
                    b.IonTitle(b.Text("Ionic quick-start")),
                    b.IonButtons(
                        b =>
                        {
                            b.SetSlot(IonToolbar.Slot.Primary);
                        },
                        b.IonButton(
                            b =>
                            {
                                b.OnClickAction((SyntaxBuilder b, Var<Model> model) =>
                                {
                                    var newCustomer = b.NewObj<Customer>(
                                        b =>
                                        {
                                            b.Set(x => x.Id, b.NewId());
                                        });

                                    return b.MakeStateWithEffects(
                                        model,
                                        b.ModalControllerPresentEffect<EditCustomerModal>(
                                            b =>
                                            {
                                                b.SetInitProps(b.NewObj<EditCustomerModal.Model>(
                                                    b =>
                                                    {
                                                        b.Set(x => x.Customer, newCustomer);
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
                                                            b.Push(b.Get(model, x => x.Customers), data.As<Customer>());
                                                            return b.Clone(model);
                                                        }));
                                                    });
                                            }));
                                });
                            },
                            b.IonIcon(
                                b =>
                                {
                                    b.SetName("person-add-outline");
                                }))))),
            b.IonContent(
                b.IonList(
                    b.IonListHeader(b.Text("Your customers list")),
                    b.Map(
                        b.Get(model, x => x.Customers),
                        (b, customer) =>
                        {
                            return b.IonItemSliding(
                                b =>
                                {
                                    b.SetProperty(b.Props, "key", b.Get(customer, x => x.Id));
                                },
                                b.IonItem(
                                    b =>
                                    {
                                        b.SetButton();
                                        b.OnClickAction((SyntaxBuilder b, Var<Model> model) =>
                                        {
                                            return b.MakeStateWithEffects(
                                                model,
                                                b =>
                                                {
                                                    var nav = b.QuerySelector("ion-nav");
                                                    b.CallOnObject(
                                                        nav,
                                                        IonNav.Method.Push,
                                                        b.Const(new ViewCustomerPage().Tag),
                                                        b.NewObj<object>(
                                                            b =>
                                                            {
                                                                b.SetProperty(b.Props, "props",
                                                        b.NewObj<ViewCustomerPage.Model>(
                                                            b =>
                                                            {
                                                                b.Set(x => x.Customer, customer);
                                                            }));
                                                            }));
                                                });
                                        });
                                    },
                                    b.IonLabel(
                                        b.Text(b.Get(customer, x => x.Name)),
                                        b.HtmlP(b.Text(
                                            b.If(
                                                b.HasObject(b.Get(customer, x=>x.Email)),
                                                b=> b.Get(customer, x=>x.Email),
                                                b=> b.Const("no email")))))),
                                b.IonItemOptions(
                                    b.IonItemOption(
                                        b =>
                                        {
                                            b.SetColorDanger();
                                            b.OnClickAction((SyntaxBuilder b, Var<Model> model) =>
                                            {
                                                b.Remove(b.Get(model, x => x.Customers), customer);
                                                return b.Clone(model);
                                            });
                                        },
                                        b.IonIcon(
                                            b =>
                                            {
                                                b.SetSlot(IonItemOption.Slot.IconOnly);
                                                b.SetName("trash");
                                            }))));
                        }))
                ),
            b.IonFooter(b.IonToolbar()));
    }
}
