# METAPSI CRM - An Ionic-based Metapsi sample

**<span style="color: red;">(in progress)</span>**

[See it running on metapsi.dev](https://metapsi.dev/sample-crm)

[Repository](https://github.com/qwebsolutions/metapsi.samples/tree/main/MetapsiCRM)

This sample showcases mobile-app-like screen navigation (go to detail, back, modal), UI model binding, API calls, direct Metapsi server calls. It is implemented using the multi-project approach. 

## HOW TO

### Create a Metapsi project from scratch

- Create a normal .NET console project (in this sample, MetapsiCRM.csproj)
- Reference nuget Metapsi.Web. This allows you to output HTML, JS and use embedded resources.
- If suitable, reference nuget Metapsi.Ionic. If offers a large set of controls and mobile-app-like navigation patterns.
- Setup your web application. Call AddMetapsi() on builder and .UseMetapsi() on app. 
- Register JavaScript modules with builder.Services.AddJsModules(...) if needed
- Call your setup in Main

#### For quick development

- Your setup method should receive a services resolver
- Create another .NET console project for running development scenarios (in this sample, MetapsiCRM.Playground.csproj)
- Reference the main project in the playground project (yes, .NET allows you to reference exe projects just like library projects)
- Setup your web application in the playground project Main method, resolving services to mockup implementations
- If suitable, reference Metapsi.ServiceDoc to add a local db service. Handle your scenario data in the playground Main

#### For interoperability

- Extract your data models and interfaces into a separate project (in this sample, MetapsiCRM.Contracts). Do not reference any other projects/nugets from this. 
- `dotnet pack` this to generate a nuget
- Reference the nuget in external projects you want to integrate with
- This allows type-safe interoperability


### Serve HTML

[In this sample: Homepage.MapHomepage](https://github.com/qwebsolutions/metapsi.samples/blob/main/MetapsiCRM/MetapsiCRM/Homepage.cs#L48)

- Map a GET async handler: 
```csharp
endpoint.MapGet("/route/{parameter}", async (HttpContext httpContext, string parameter) => 
{
    var model = await LoadModel(parameter);
    
    var html = HtmlBuilder.FromDefault(
        b =>
        {
            b.Render(model);
        });
    await httpContext.WriteHtmlDocumentResponse(html);
}
```
- Load the page model with an async call
- Initialize an `HtmlBuilder`
- Create one (or more, for nested calls) extension methods to transform that model into HTML. The methods on HtmlBuilder are **EXACT EQUIVALENTS** of the HTML tags. 

```C#
private static void Render(this HtmlBuilder b, Model model) 
{
    b.HeadAppend(b.HtmlTitle(...));
    b.BodyAppend(b.HtmlDiv(...));
}
```

- Write the resulting document to the HTTP response

### Render client-side

[In this sample: `b.Hyperapp<Model>(...)`](https://github.com/qwebsolutions/metapsi.samples/blob/main/MetapsiCRM/MetapsiCRM/Homepage.cs#L58)

- The HTML served by the server also contains `<script>` tags appended with `b.BodyAppend(b.HtmlScript(...))`
- One or more of these script tags might handle client-side rendering
- In Metapsi this is handled through [Hyperapp](https://github.com/jorgebucaran/hyperapp)

```csharp
b.BodyAppend(
    b.Hyperapp<Model>(
        model,
        (b, model) => b.IonApp(...)));
```

### Create custom element

- Create a class that inherits `CustomElement<TModel>`
- Override `OnInit` - the first action to be performed when the element is created. Here you can use the attributes on the element as parameters of the initialization process.
- Override `OnRender` - the virtual DOM refresh function. This *must* return `this.Root`
- To serve it as a JavaScript module:

```csharp
builder.Services.AddJsModules(
    b =>
    {
        b.AddModule(new MyCustomElement().ModulePath, () => new MyCustomElement().Module);
    });
```
This works with hot reload, as the constructor is called for each request. To auto-register:

```csharp
builder.Services.AddJsModules(
    b =>
    {
        b.AutoRegisterFrom(typeof(Program).Assembly);
    });
```

You can also [combine hot reload / auto-register](https://github.com/qwebsolutions/metapsi.samples/blob/main/MetapsiCRM/MetapsiCRM/Program.cs#L35)

### Improve custom-element type-safety

If you inherit from `CustomElement<TModel>` you can use it in any technology, in any web-page. If you know for sure that the element will only be used in your own Ionic projects, you can inherit from
`CustomElement<TModel, TProps>`. This allows you to pass an actual initialization object.

- Inherit from `CustomElement<TModel, TProps>`
- Override `OnInitProps` - initializes the control based on the passed-in object
- Override `OnRender`


### Navigate between app screens with Ionic

- Render the application as just a simple `ion-nav`

```csharp
(b, model) => b.IonApp(b.IonNav())
```

- Create the screens as custom elements
- Navigate to an app screen with `b.IonNavPushEffect<TCustomElement>(b=> { ... })`
- To go back it's enough to render a `<ion-back-button>` in the new screen

```csharp
b.IonToolbar(b.IonButtons(b.IonBackButton()))
```

### Open modals

- Create the modals as custom elements
- Show a modal with `b.ModalControllerPresentEffect<TCustomElement>`
- To enable the collapse of the modal with a vertical swipe gesture:

```csharp
b.Set(x => x.breakpoints, [0, 1]);
b.Set(x => x.initialBreakpoint, 1);
```

- Otherwise, add an ion-button that calls `b.ModalControllerDismiss()`
