using Metapsi;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MetapsiCRM;

public interface ICustomersDbService
{
    Task<IEnumerable<Customer>> GetCustomers();
    Task SaveCustomer(Customer customer);
    Task DeleteCustomer(Customer customer);
}

public interface IUsersDbService
{
    Task<User> GetUserByPhone(string phoneNumber);
    Task<User> GetUserById(string userId);
}

public class Services
{
    public ICustomersDbService Customers { get; set; }
    public IUsersDbService Users { get; set; }
    public User User { get; set; }
    public string UiMode { get; set; } = "md";
}

public static class ServicesExtensions
{
    public static async Task<User> GetUser(this HttpContext httpContext, IUsersDbService identityService)
    {
        var user = httpContext.User;
        if (user.Identity != null)
        {
            if (user.Identity.IsAuthenticated)
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrWhiteSpace(userId))
                {
                    return await identityService.GetUserById(userId);
                }
            }
        }

        return new User();
    }

    public static string UiMode(this HttpContext httpContext)
    {
        var mode = httpContext.Request.Query["mode"];
        if (string.IsNullOrWhiteSpace(mode))
            return "md";
        var first = mode.FirstOrDefault();
        if (first.ToLowerInvariant() == "ios")
            return "ios";
        return "md";
    }
}