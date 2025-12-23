# METAPSI CRM - An Ionic-based Metapsi sample

<span style="color: red;">(in progress)</span>

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