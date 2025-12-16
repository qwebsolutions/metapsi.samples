using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MetapsiCRM;

public class IdentityService : IUsersDbService
{
    public async Task<User> GetUserByPhone(string phoneNumber)
    {
        return new User()
        {
            Id = "1",
            Phone = phoneNumber,
            Name = "John Doe"
        };
    }

    public async Task<User> GetUserById(string id)
    {
        return new User()
        {
            Id = id,
            Phone = "+1234567890",
            Name = "John Doe"
        };
    }
}

public class CookieDbService: ICustomersDbService
{
    const string CookieName = "fakeDb";

    private HttpContext httpContext;
    public CookieDbService(HttpContext httpContext)
    {
        this.httpContext = httpContext;

        var cookie = this.httpContext.Request.Cookies[CookieName];
        if (cookie == null)
        {
            this.Customers = DefaultList;
        }
        else
        {
            var customersList = System.Text.Json.JsonSerializer.Deserialize<List<Customer>>(Uri.UnescapeDataString(cookie));
            this.Customers = customersList.OrderBy(x => x.Name).ToList();
        }
    }

    private List<Customer> DefaultList { get; set; } = new List<Customer>()
    {
        new Customer()
        {
            Id = Guid.NewGuid(),
            Name = "First Customer",
            Email = "first.customer@email.com",
            Phone = "123-555-555-001",
            Notes = "Wants the project yesterday"
        }
    };

    private List<Customer> Customers { get; set; } = new List<Customer>();

    private void SaveToCookie()
    {
        var serialized = Uri.EscapeDataString(System.Text.Json.JsonSerializer.Serialize(this.Customers));
        if (serialized.Length > 4000)
            throw new Exception("Cannot save: maximum size exceeded");

        this.httpContext.Response.Cookies.Append(
            CookieName,
            serialized,
            new CookieOptions
            {
                Path = "/",
                HttpOnly = false,
                MaxAge = TimeSpan.FromDays(7)
            });
    }

    public async Task<IEnumerable<Customer>> GetCustomers()
    {
        return Customers;
    }


    public async Task SaveCustomer(Customer customer)
    {
        this.Customers.RemoveAll(x => x.Id == customer.Id);
        this.Customers.Add(customer);
        SaveToCookie();
    }

    public async Task DeleteCustomer(Customer customer)
    {
        this.Customers.RemoveAll(x => x.Id == customer.Id);
        SaveToCookie();
    }
}
