using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

public class CookieDbService: IDbService
{
    const string CookieName = "fakeDb";

    private HttpContext httpContext;
    public CookieDbService(HttpContext httpContext)
    {
        this.httpContext = httpContext;
    }

    private static List<Customer> DefaultList { get; set; } = new List<Customer>()
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


    private void SaveToCookie(IEnumerable<Customer> customers)
    {
        var serialized = Uri.EscapeDataString(System.Text.Json.JsonSerializer.Serialize(customers));

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
        var cookie = this.httpContext.Request.Cookies[CookieName];
        if (cookie == null)
        {
            SaveToCookie(DefaultList);
            return DefaultList;
        }
        else
        {
            var customersList = System.Text.Json.JsonSerializer.Deserialize<List<Customer>>(Uri.UnescapeDataString(cookie));
            return customersList.OrderBy(x => x.Name);
        }
    }


    public async Task SaveCustomer(Customer customer)
    {
        var customers = (await this.GetCustomers()).ToList();
        customers.RemoveAll(x => x.Id == customer.Id);
        customers.Add(customer);
        SaveToCookie(customers);
    }
}
