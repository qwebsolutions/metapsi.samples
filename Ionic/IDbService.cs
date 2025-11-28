using System.Threading.Tasks;
using System.Collections.Generic;
//public class Model
//{

//}

public interface IDbService
{
    Task<IEnumerable<Customer>> GetCustomers();
    Task SaveCustomer(Customer customer);
}
