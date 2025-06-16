using Microsoft.AspNetCore.Builder;
using System.Threading.Tasks;
using Metapsi;
using System;
using Metapsi.Html;
using Metapsi.Syntax;
using Metapsi.Hyperapp;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Routing;
using Metapsi.SignalR;
using Microsoft.AspNetCore.Http;
using Metapsi.Shoelace;
using Metapsi.Web;
using System.Net.Http;
using System.Xml.Linq;

public class Entry
{
    public string Ticker { get; set; }
    public string Name { get; set; }
    public decimal Value { get; set; } = 100;
    public bool GoingUp { get; set; } = true;
}

public class MarketData
{
    public List<Entry> Entries { get; set; } = new List<Entry>()
    {
        new Entry(){ Ticker = "NVDA", Name = "Nvidia" },
        new Entry(){ Ticker = "AAPL", Name = "Apple" },
        new Entry(){ Ticker = "TSLA", Name = "Tesla" },
        new Entry(){ Ticker = "AMZN", Name = "Amazon" },
        new Entry(){ Ticker = "MSFT", Name = "Microsoft" }
    };
}

public class Refresh
{
    public MarketData MarketData { get; set; }
}

public static class Program
{
    /// <summary>
    /// Adds a SignalR hub on DefaultMetapsiSignalRHub.Path
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static HubEndpointConventionBuilder MapDefaultSignalRHub(
        this IEndpointRouteBuilder builder,
        string path = nameof(DefaultMetapsiSignalRHub),
        Action<IHubContext> storeHubContext = null)
    {
        var hubBuilder = builder.MapHub<DefaultMetapsiSignalRHub>(path);
        if (storeHubContext != null)
        {
            var hubContext = builder.ServiceProvider.GetService(typeof(IHubContext<DefaultMetapsiSignalRHub>)) as IHubContext;
            storeHubContext(hubContext);
        }
        return hubBuilder;
    }

    public static async Task Main()
    {
        var builder = WebApplication.CreateBuilder().AddMetapsi();
        builder.Services.AddSignalR()
            .AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.PropertyNamingPolicy = null;
            });
        var app = builder.Build();
        app.UseMetapsi();
        app.Urls.Add("http://localhost:5000");
        app.MapDefaultSignalRHub(storeHubContext: hc => StaticHubContext = hc);

        app.MapGet("/", () => Results.Redirect("/market"));

        app.MapGet("/market", () => Page.Result(new MarketData()));

        app.UseRenderer<MarketData>(model =>
        {
            return HtmlBuilder.FromDefault(
                b =>
                {
                    b.UseWebComponentsFadeIn();
                    b.HeadAppend(b.HtmlTitle("Market data, absolutely real time for sure ..."));
                    b.BodyAppend(
                        b.Hyperapp<MarketData>(
                            InitializeClientSideApp,
                            RenderClientSideApp,
                            ListenForUpdates));
                });
        });

        var _notAwaited = Task.Run(GenerateRandomData);

        await app.RunAsync();
    }

    public static Var<HyperType.Init> InitializeClientSideApp(SyntaxBuilder b)
    {
        return b.MakeInit(
            b.MakeStateWithEffects(
                b.Const(new MarketData()),
                // Connect to default SignalR hub
                b.SignalRConnect(nameof(DefaultMetapsiSignalRHub))));
    }

    public static Var<HyperType.Subscription> ListenForUpdates(SyntaxBuilder b, Var<MarketData> _modelNotUsed)
    {
        // Listen to SignalR event
        return b.Listen(
            b.Window(),
            b.Const("Refresh"),
            b.MakeAction((SyntaxBuilder b, Var<MarketData> model, Var<CustomEvent<Refresh>> refreshEvent) =>
            {
                // Return the data. Hyperapp triggers automatic refresh
                return b.Get(refreshEvent, x => x.detail.MarketData);
            }));
    }


    public static Var<IVNode> RenderClientSideApp(LayoutBuilder b, Var<MarketData> model)
    {
        return b.HtmlDiv(
            b.HtmlDiv(
                b.Map(
                    b.Get(model, x => x.Entries.OrderByDescending(x => x.Value).ToList()),
                    (b, company) =>
                    {
                        return b.HtmlDiv(
                            b =>
                            {
                                b.AddStyle("display", "flex");
                                b.Comment("Display flex");
                                b.AddStyle(
                                    "color",
                                    b.If(
                                        b.Get(company, x => x.GoingUp),
                                        b => b.Const("green"),
                                        b => b.Const("red")));
                            },
                            b.SlBadge(
                                b =>
                                {
                                    b.AddStyle("width", "100px");
                                },
                                b.Text(b.Get(company, x => x.Ticker))),
                            b.HtmlDiv(
                                b =>
                                {
                                    b.AddStyle("width", "150px");
                                },
                                b.Text(b.Get(company, x => x.Name))),
                            b.HtmlDiv(
                                b =>
                                {
                                    b.AddStyle("width", "50px");
                                },
                                b.Text(b.AsString(b.Get(company, x => x.Value)))));
                    })));
    }

    public static IHubContext StaticHubContext;

    public static async Task GenerateRandomData()
    {
        MarketData marketData = new();

        Random r = new Random();

        while (true)
        {
            await Task.Delay(3000);
            foreach (var entry in marketData.Entries)
            {
                var newValue = entry.Value + new decimal(r.NextDouble() - 0.5) * 2;
                entry.GoingUp = newValue > entry.Value;
                entry.Value = decimal.Round(newValue, 2);
            }

            // Raise SignalR event with new data
            await StaticHubContext.Clients.All.RaiseCustomEvent(
                typeof(Refresh).Name,
                new Refresh()
                {
                    MarketData = marketData
                });
        }
    }
}