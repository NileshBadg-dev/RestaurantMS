using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.Models;

public class CustomerAgeingViewModel
{
    public IReadOnlyList<CustomerAgeingDto> CustomerAgeings { get; set; } = new List<CustomerAgeingDto>();
    public decimal TotalBucket0To30 { get; set; }
    public decimal TotalBucket31To60 { get; set; }
    public decimal TotalBucket61To90 { get; set; }
    public decimal TotalBucketOver90 { get; set; }
    public decimal GrandTotalOutstanding { get; set; }
}
