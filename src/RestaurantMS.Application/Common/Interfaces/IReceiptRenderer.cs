using RestaurantMS.Application.Common.Models.Billing;

namespace RestaurantMS.Application.Common.Interfaces;

public interface IReceiptRenderer
{
    string RenderBillReceiptHtml(SettledBillReceiptDto receipt, ReceiptRenderOptions options);

    string RenderParcelTokenHtml(SettledBillReceiptDto receipt, ReceiptRenderOptions options);

    string RenderKotHtml(KotPrintDto kot, ReceiptRenderOptions options);

    string BuildUpiPaymentUri(string upiId, string payeeName, decimal amount, string? transactionNote = null);
}
