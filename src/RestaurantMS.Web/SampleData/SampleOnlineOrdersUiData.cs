using RestaurantMS.Application.Common.Interfaces.UiData;
using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.SampleData;

public class SampleOnlineOrdersUiData : IOnlineOrdersUiData
{
    private readonly List<OnlineOrderDto> _orders = new()
    {
        new(
            OrderId: "ZOM-9481",
            Channel: "Zomato",
            CustomerName: "Pooja Kadam",
            CustomerPhone: "9822998811",
            DeliveryAddress: "Flat 402, Shivshakti Apts, FC Road, Pune",
            ItemsSummary: "2x Butter Chicken, 4x Butter Naan, 1x Jeera Rice",
            TotalAmount: 780m,
            CommissionAmount: 156m,
            NetPayout: 624m,
            Status: "New",
            OrderTime: "14:15 (5m ago)",
            PrepTimeMinutes: 25,
            RiderName: "Vikram R (Zomato)",
            RiderPhone: "9823001122",
            RiderOtp: "4821"
        ),
        new(
            OrderId: "SWG-3204",
            Channel: "Swiggy",
            CustomerName: "Amit Joshi",
            CustomerPhone: "9876112233",
            DeliveryAddress: "Office 12, Cyber Park, Senapati Bapat Marg, Pune",
            ItemsSummary: "1x Paneer Tikka Biryani Handi, 1x Gulab Jamun (2 pcs)",
            TotalAmount: 390m,
            CommissionAmount: 78m,
            NetPayout: 312m,
            Status: "InPreparation",
            OrderTime: "14:05 (15m ago)",
            PrepTimeMinutes: 20,
            RiderName: "Rahul Shinde (Swiggy)",
            RiderPhone: "9766554433",
            RiderOtp: "7914"
        ),
        new(
            OrderId: "ZOM-9477",
            Channel: "Zomato",
            CustomerName: "Kiran Deshpande",
            CustomerPhone: "9422001155",
            DeliveryAddress: "Bungalow 7, Model Colony, Pune",
            ItemsSummary: "1x Dal Makhani, 1x Kadhai Paneer, 6x Tandoori Roti",
            TotalAmount: 620m,
            CommissionAmount: 124m,
            NetPayout: 496m,
            Status: "FoodReady",
            OrderTime: "13:50 (30m ago)",
            PrepTimeMinutes: 20,
            RiderName: "Sunil More (Zomato)",
            RiderPhone: "9158443322",
            RiderOtp: "3290"
        ),
        new(
            OrderId: "WEB-1082",
            Channel: "Direct Web",
            CustomerName: "Dr. Sandeep Mane",
            CustomerPhone: "9881223344",
            DeliveryAddress: "Plot 15, Prabhat Road, Lane 4, Pune",
            ItemsSummary: "2x Mutton Sukka, 1x Chicken Biryani, 4x Chapati",
            TotalAmount: 940m,
            CommissionAmount: 0m,
            NetPayout: 940m,
            Status: "OutForDelivery",
            OrderTime: "13:35 (45m ago)",
            PrepTimeMinutes: 30,
            RiderName: "Hotel Delivery (Santosh)",
            RiderPhone: "9822012345",
            RiderOtp: "8841"
        ),
        new(
            OrderId: "SWG-3199",
            Channel: "Swiggy",
            CustomerName: "Neha Sharma",
            CustomerPhone: "9890445566",
            DeliveryAddress: "A-201, Rohan Heights, Kothrud, Pune",
            ItemsSummary: "1x Masala Dosa, 1x Filter Coffee, 1x Medu Vada",
            TotalAmount: 260m,
            CommissionAmount: 52m,
            NetPayout: 208m,
            Status: "Delivered",
            OrderTime: "12:55",
            PrepTimeMinutes: 15,
            RiderName: "Pradeep G",
            RiderPhone: "9765112233",
            RiderOtp: "1192"
        )
    };

    public Task<IReadOnlyList<OnlineOrderDto>> GetOnlineOrdersAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<OnlineOrderDto>>(_orders);
    }

    public Task<bool> UpdateOrderStatusAsync(string orderId, string newStatus, CancellationToken cancellationToken = default)
    {
        var existing = _orders.FirstOrDefault(o => o.OrderId == orderId);
        if (existing == null) return Task.FromResult(false);

        var updated = existing with { Status = newStatus };
        _orders.Remove(existing);
        _orders.Add(updated);
        return Task.FromResult(true);
    }
}
