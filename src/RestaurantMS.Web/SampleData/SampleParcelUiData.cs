using RestaurantMS.Application.Common.Interfaces.UiData;
using RestaurantMS.Application.Common.Models;
using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.SampleData;

public class SampleParcelUiData : IParcelUiData
{
    private static readonly List<ParcelItemDto> ParcelsList = [];
    private static int _tokenSeq = 101;
    private static readonly object _lock = new();

    public Task<PagedResult<ParcelItemDto>> GetParcelsAsync(DataTableRequest request, string? status = null, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var all = ParcelsList.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                all = all.Where(p => p.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(request.SearchValue))
            {
                var search = request.SearchValue.Trim().ToLowerInvariant();
                all = all.Where(p => p.TokenNo.ToLowerInvariant().Contains(search) ||
                                     p.CustomerName.ToLowerInvariant().Contains(search) ||
                                     p.CustomerMobile.ToLowerInvariant().Contains(search));
            }

            var list = all.ToList();
            var paged = list.Skip(request.Start).Take(request.PageSize).ToList();
            return Task.FromResult(new PagedResult<ParcelItemDto>(paged, list.Count, request.PageNumber, request.PageSize));
        }
    }

    public Task<ParcelDetailsDto?> GetParcelDetailsAsync(string tokenNo, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var parcel = ParcelsList.FirstOrDefault(p => p.TokenNo.Equals(tokenNo, StringComparison.OrdinalIgnoreCase));
            if (parcel is null)
            {
                return Task.FromResult<ParcelDetailsDto?>(null);
            }

            var details = new ParcelDetailsDto(
                TokenNo: parcel.TokenNo,
                CustomerName: parcel.CustomerName,
                CustomerMobile: parcel.CustomerMobile,
                PickupTime: parcel.PickupTime,
                PackingCharge: 20m,
                Subtotal: parcel.Amount - 20m,
                GstAmount: (parcel.Amount - 20m) * 0.05m,
                TotalAmount: parcel.Amount,
                Status: parcel.Status,
                Items: new List<BillingItemRowDto>());

            return Task.FromResult<ParcelDetailsDto?>(details);
        }
    }

    public static string CreateParcel(string customerName, string mobile, string itemsSummary, decimal amount)
    {
        lock (_lock)
        {
            var token = $"P-{_tokenSeq++}";
            var pickup = DateTime.UtcNow.AddMinutes(20).ToString("h:mm tt");
            var item = new ParcelItemDto(token, customerName, mobile, itemsSummary, amount, "New", DateTime.UtcNow.ToString("h:mm tt"), pickup);
            ParcelsList.Insert(0, item);
            return token;
        }
    }
}
