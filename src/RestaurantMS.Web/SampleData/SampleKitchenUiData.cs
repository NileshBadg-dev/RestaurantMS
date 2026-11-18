using RestaurantMS.Application.Common.Interfaces.UiData;
using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.SampleData;

public class SampleKitchenUiData : IKitchenUiData
{
    private static readonly List<KitchenTicketDto> NewTickets = [];
    private static readonly List<KitchenTicketDto> PreparingTickets = [];
    private static readonly List<KitchenTicketDto> ReadyTickets = [];
    private static readonly object _lock = new();

    public Task<KitchenDisplayDto> GetKitchenDisplayAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var display = new KitchenDisplayDto(
                NewTickets.ToList(),
                PreparingTickets.ToList(),
                ReadyTickets.ToList());

            return Task.FromResult(display);
        }
    }

    public static void AddTicketFromOrder(string orderId, string tableOrType, List<BillingItemRowDto> items)
    {
        lock (_lock)
        {
            var ticketNo = orderId.Replace("ORD-", "KOT-");
            var kotItems = items.Select(i => new KitchenTicketItemDto(i.ItemName, i.Quantity, i.Notes)).ToList();

            var isTakeaway = tableOrType.Contains("Parcel", StringComparison.OrdinalIgnoreCase) || tableOrType.Contains("Takeaway", StringComparison.OrdinalIgnoreCase);
            var ticket = new KitchenTicketDto(
                ticketNo,
                orderId.Replace("ORD-", ""),
                tableOrType,
                "0:10",
                false,
                isTakeaway,
                "New",
                kotItems);

            NewTickets.Insert(0, ticket);
        }
    }

    public static void MoveToPreparing(string ticketId)
    {
        lock (_lock)
        {
            var idx = NewTickets.FindIndex(t => t.TicketId.Equals(ticketId, StringComparison.OrdinalIgnoreCase));
            if (idx >= 0)
            {
                var t = NewTickets[idx];
                NewTickets.RemoveAt(idx);
                var prep = new KitchenTicketDto(t.TicketId, t.KotNumber, t.TableOrTakeaway, "1:00", false, t.IsParcel, "Preparing", t.Items);
                PreparingTickets.Insert(0, prep);
            }
        }
    }

    public static void MoveToReady(string ticketId)
    {
        lock (_lock)
        {
            var idx = PreparingTickets.FindIndex(t => t.TicketId.Equals(ticketId, StringComparison.OrdinalIgnoreCase));
            if (idx >= 0)
            {
                var t = PreparingTickets[idx];
                PreparingTickets.RemoveAt(idx);
                var rdy = new KitchenTicketDto(t.TicketId, t.KotNumber, t.TableOrTakeaway, "Ready", false, t.IsParcel, "Ready", t.Items);
                ReadyTickets.Insert(0, rdy);
            }
        }
    }

    public static void ClearServed(string ticketId)
    {
        lock (_lock)
        {
            ReadyTickets.RemoveAll(t => t.TicketId.Equals(ticketId, StringComparison.OrdinalIgnoreCase));
        }
    }

    public static KitchenTicketDto? FindTicket(string kotOrTicketId)
    {
        if (string.IsNullOrWhiteSpace(kotOrTicketId))
        {
            return null;
        }

        lock (_lock)
        {
            return NewTickets
                .Concat(PreparingTickets)
                .Concat(ReadyTickets)
                .FirstOrDefault(t =>
                    t.TicketId.Equals(kotOrTicketId, StringComparison.OrdinalIgnoreCase) ||
                    t.KotNumber.Equals(kotOrTicketId, StringComparison.OrdinalIgnoreCase));
        }
    }
}
