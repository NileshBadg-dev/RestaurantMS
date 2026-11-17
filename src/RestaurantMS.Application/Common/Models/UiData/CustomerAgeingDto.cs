namespace RestaurantMS.Application.Common.Models.UiData;

public record CustomerAgeingDto(
    string CustomerName,
    string Mobile,
    decimal TotalOutstanding,
    decimal Bucket0To30,
    decimal Bucket31To60,
    decimal Bucket61To90,
    decimal BucketOver90);
