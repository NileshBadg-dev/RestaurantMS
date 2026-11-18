namespace RestaurantMS.Application.Common.Models.Validation;

public record ReservationValidationRequest(
    string GuestName,
    string Mobile,
    int GuestCount,
    DateTime ReservationDate,
    string TimeSlot);
