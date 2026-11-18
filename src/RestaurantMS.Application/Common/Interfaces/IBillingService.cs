using RestaurantMS.Application.Common.Models.Billing;
using RestaurantMS.SharedKernel;

namespace RestaurantMS.Application.Common.Interfaces;

public interface IBillingService
{
    BillCalculationResult CalculateTotals(BillCalculationRequest request);

    Result ValidateSettlement(SettleBillRequest request, BillingValidationPolicyDto policy);

    Task<Result<SettledBillReceiptDto>> SettleBillAsync(
        SettleBillRequest request,
        BillingValidationPolicyDto policy,
        string cashierName,
        CancellationToken cancellationToken = default);

    SettledBillReceiptDto? GetReceipt(string billNumber);

    IReadOnlyList<SettledBillReceiptDto> GetAllReceipts();
}
