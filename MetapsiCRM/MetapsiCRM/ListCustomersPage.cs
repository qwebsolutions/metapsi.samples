using Metapsi;
using Metapsi.Html;
using Metapsi.Hyperapp;
using Metapsi.Ionic;
using Metapsi.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MetapsiCRM;

public class ListCustomersPage: CustomElement<ListCustomersPage.Model>
{
    public const string ReloadCustomersEventName = "reload-customers";

    public class Model
    {
        public bool Loading { get; set; }
        public List<Customer> Customers { get; set; } = new List<Customer>();
    }

    public override Var<HyperType.StateWithEffects> OnInit(SyntaxBuilder b, Var<Element> element)
    {
        return b.MakeStateWithEffects(
            b.Const(new Model()
            {
                Loading = true
            }),
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
                                Customers = (await services.Customers.GetCustomers()).ToList()
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
                            Customers = (await services.Customers.GetCustomers()).ToList()
                        };
                    })));
    }

    public override IRootControl OnRender(LayoutBuilder b, Var<Model> model)
    {
        return this.Root(b.IonHeader(
                b.IonToolbar(
                    b.IonTitle(b.Text("Metapsi CRM")),
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
                                            })
                                        );
                                });
                            },
                            b.IonIcon(
                                b =>
                                {
                                    b.SetName("person-add-outline");
                                })),
                        b.IonButton(
                            b=>
                            {
                                b.OnClickAction(b.MakeAction((SyntaxBuilder b, Var<Model> model) =>
                                {
                                    return b.MakeStateWithEffects(
                                        model,
                                        b.PostJsonEffect<Model>(
                                            b.Const("/sign-out"),
                                            b.MakeAction((SyntaxBuilder b, Var<Model> model) =>
                                            {
                                                return b.MakeStateWithEffects(
                                                    model,
                                                    b => b.DispatchEvent(b.Window(), b.Const("reload-app")));
                                            }),
                                            b.AlertOnException<Model>()));
                                }));
                            },
                            b.Text("Sign out"))))),
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
                                                b.IonNavPushEffect<ViewCustomerPage>(
                                                    b =>
                                                    {
                                                        b.SetInitProps(b.NewObj<ViewCustomerPage.Model>(
                                                            b =>
                                                            {
                                                                b.Set(x => x.Customer, customer);
                                                            }));
                                                    }));
                                        });
                                    },
                                    b.IonLabel(
                                        b.Text(b.Get(customer, x => x.Name)),
                                        b.HtmlP(b.Text(
                                            b.If(
                                                b.HasObject(b.Get(customer, x => x.Email)),
                                                b => b.Get(customer, x => x.Email),
                                                b => b.Const("no email")))))),
                                b.IonItemOptions(
                                    b.IonItemOption(
                                        b =>
                                        {
                                            b.SetColorDanger();
                                            b.OnClickAction(
                                                b.CallServerAction(
                                                    b.Const("/server-action"),
                                                    customer,
                                                    async (Model model, Customer customer, Services services) =>
                                                    {
                                                        await services.Customers.DeleteCustomer(customer);
                                                        model.Customers = (await services.Customers.GetCustomers()).ToList();
                                                        return model;
                                                    },
                                                    b.MakeAction((SyntaxBuilder b, Var<Model> _oldModel, Var<Model> model) =>
                                                    {
                                                        return b.MakeStateWithEffects(
                                                            model,
                                                            b => b.ToastControllerPresent(b =>
                                                            {
                                                                b.Set(x => x.message, "Customer removed");
                                                                b.Set(x => x.swipeGesture, "vertical");
                                                                b.Set(x => x.duration, 5000);
                                                                b.Set(x => x.color, "primary");
                                                            }));
                                                    })));
                                        },
                                        b.IonIcon(
                                            b =>
                                            {
                                                b.SetSlot(IonItemOption.Slot.IconOnly);
                                                b.SetName("trash");
                                            }))));
                        }))
                ),
            b.IonFooter(
                b.IonToolbar(
                    b.Optional(
                        b.Get(model, x => !x.Loading),
                        b => b.IonTitle(
                            b.Text(
                                b.Concat(
                                    b.AsString(
                                        b.Get(model, x => x.Customers.Count())),
                                    b.Const(" customers"))))))));
    }
}
