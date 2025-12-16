using Metapsi;

namespace MetapsiCRM.Playground;

public class PlaygroundDb : ICustomersDbService, IUsersDbService
{
    private ServiceDoc.DbQueue dbQueue;

    public PlaygroundDb(ServiceDoc.DbQueue dbQueue)
    {
        this.dbQueue = dbQueue;
    }

    public async Task DeleteCustomer(Customer customer)
    {
        await this.dbQueue.DeleteDocument<Customer>(customer.Id);
    }

    public async Task<IEnumerable<Customer>> GetCustomers()
    {
        return await this.dbQueue.ListDocuments<Customer>();
    }

    public async Task SaveCustomer(Customer customer)
    {
        await this.dbQueue.SaveDocument(customer);
    }

    public async Task<User> GetUserByPhone(string phoneNumber)
    {
        var users = await this.dbQueue.SqliteQueue.GetDocuments<User>(x => x.Phone, phoneNumber);
        return users.SingleOrDefault();
    }

    public async Task<User> GetUserById(string id)
    {
        return await this.dbQueue.GetDocument<User>(id);
    }
}
