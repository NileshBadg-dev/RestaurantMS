using RestaurantMS.Application.Common.Interfaces.UiData;
using RestaurantMS.Application.Common.Models;
using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.SampleData;

public class SampleCustomersUiData : ICustomersUiData
{
    private static readonly List<CustomerListItemDto> CustomersList =
    [
        new(1, "Ramesh Patil", "9876543210", "14 Shivaji Nagar, Pune - 411005", "27AAAAA0000A1Z2", true, 10000m, 0m),
        new(2, "Sunita Shinde", "9765432109", "25 Kothrud, Pune - 411038", null, true, 5000m, 0m),
        new(3, "Amit Deshmukh", "9823456789", "8 FC Road, Deccan, Pune - 411004", "27BBBBB1111B1Z1", true, 8000m, 0m),
        new(4, "Vijay Shrestha", "9922334455", "102 Viman Nagar, Pune - 411014", null, true, 5000m, 0m),
        new(5, "Ganesh Joshi", "9811223344", "55 Deccan Gymkhana, Pune - 411004", null, false, 0m, 0m)
    ];

    private static long _nextId = 6;
    private static readonly object _lock = new();

    public Task<PagedResult<CustomerListItemDto>> GetCustomersAsync(DataTableRequest request, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var all = CustomersList.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(request.SearchValue))
            {
                var search = request.SearchValue.Trim().ToLowerInvariant();
                all = all.Where(c => c.Name.ToLowerInvariant().Contains(search) ||
                                     c.Mobile.Contains(search) ||
                                     (c.Address != null && c.Address.ToLowerInvariant().Contains(search)));
            }

            var list = all.ToList();
            var paged = list.Skip(request.Start).Take(request.PageSize).ToList();
            return Task.FromResult(new PagedResult<CustomerListItemDto>(paged, list.Count, request.PageNumber, request.PageSize));
        }
    }

    public Task<CustomerListItemDto?> GetCustomerByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var customer = CustomersList.FirstOrDefault(c => c.Id == id);
            return Task.FromResult<CustomerListItemDto?>(customer);
        }
    }

    public Task<IReadOnlyList<CustomerLedgerEntryDto>> GetCustomerLedgerAsync(long customerId, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var customer = CustomersList.FirstOrDefault(c => c.Id == customerId);
            var balance = customer?.OutstandingBalance ?? 0m;
            var entries = new List<CustomerLedgerEntryDto>
            {
                new(DateTime.UtcNow.ToString("dd/MM/yyyy"), "Opening Balance", "OB-001", balance, 0m, balance)
            };
            return Task.FromResult<IReadOnlyList<CustomerLedgerEntryDto>>(entries);
        }
    }

    public Task<IReadOnlyList<CustomerAgeingDto>> GetCustomerAgeingAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var ageing = CustomersList
                .Where(c => c.OutstandingBalance > 0)
                .Select(c => new CustomerAgeingDto(c.Name, c.Mobile, c.OutstandingBalance, c.OutstandingBalance, 0m, 0m, 0m))
                .ToList();

            return Task.FromResult<IReadOnlyList<CustomerAgeingDto>>(ageing);
        }
    }

    public static CustomerListItemDto AddCustomer(string name, string mobile, string address, decimal limit = 10000m)
    {
        lock (_lock)
        {
            var existing = CustomersList.FirstOrDefault(c => c.Mobile == mobile);
            if (existing != null)
            {
                return existing;
            }

            var customer = new CustomerListItemDto(_nextId++, name, mobile, address, null, true, limit, 0m);
            CustomersList.Add(customer);
            return customer;
        }
    }

    public static IReadOnlyList<CustomerListItemDto> GetAllCustomers()
    {
        lock (_lock)
        {
            return CustomersList.ToList();
        }
    }

    public static void RecordCreditDebt(string mobile, decimal amount)
    {
        lock (_lock)
        {
            var idx = CustomersList.FindIndex(c => c.Mobile == mobile);
            if (idx >= 0)
            {
                var cur = CustomersList[idx];
                CustomersList[idx] = new CustomerListItemDto(cur.Id, cur.Name, cur.Mobile, cur.Address, cur.Gstin, cur.CreditAllowed, cur.CreditLimit, cur.OutstandingBalance + amount);
            }
        }
    }
}
