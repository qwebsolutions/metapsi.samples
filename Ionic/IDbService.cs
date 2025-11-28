using System.Threading.Tasks;
using System.Collections.Generic;

public interface IDbService
{
    Task<IEnumerable<Customer>> GetCustomers();
    Task SaveCustomer(Customer customer);
    Task DeleteCustomer(Customer customer);
}
