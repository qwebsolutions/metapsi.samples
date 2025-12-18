using Metapsi;
using Microsoft.AspNetCore.Builder;

namespace MetapsiCRM.Playground;

class Program
{
    static async Task Main(string[] args)
    {
        var projectPath = RelativePath.SearchUpfolder(RelativePath.From.CurrentDir, "MetapsiCRM.Playground");
        var dbPath = System.IO.Path.Combine(projectPath, "playground.db");

        var dbQueue = new ServiceDoc.DbQueue(new Metapsi.Sqlite.SqliteQueue(dbPath));
        
        var dbService = new PlaygroundDb(dbQueue);

        var webApplication = MetapsiCRM.Program.Setup(async httpContext =>
        {
            return new Services()
            {
                Customers = dbService,
                Users = dbService,
                User = await httpContext.GetUser(dbService),
                UiMode = httpContext.UiMode()
            };
        });

        await webApplication.MapGroup("db").UseDocs(
            dbQueue,
            b =>
            {
                b.AddDoc<Customer>(x => x.Id);
                b.AddDoc<User>(
                    x => x.Id,
                    b =>
                    {
                        b.AddIndex(x => x.Phone);
                    });
            });

        await Scenario_OneDefaultCustomer(dbQueue);

        await webApplication.RunAsync();
    }

    public static async Task Scenario_OneDefaultCustomer(Metapsi.ServiceDoc.DbQueue dbQueue)
    {
        var users = await dbQueue.ListDocuments<User>();
        if (!users.Any())
        {
            await dbQueue.SaveDocument(new User()
            {
                Id = "default-user",
                Name = "Default User",
                Phone = "+1234567890"
            });
        }

        var testCustomers = await dbQueue.ListDocuments<Customer>();
        if(!testCustomers.Any())
        {
            await dbQueue.SaveDocument(new Customer()
            {
                Id = Guid.NewGuid(),
                Email = "first@customer.com",
                Name = "First Customer",
                Notes = "Wants the project yesterday"
            });
        }
    }
}
