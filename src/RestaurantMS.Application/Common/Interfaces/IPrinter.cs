namespace RestaurantMS.Application.Common.Interfaces;

public interface IPrinter
{
    Task PrintReceiptAsync(byte[] content, string printerName, CancellationToken cancellationToken = default);
    Task PrintKotAsync(byte[] content, string printerName, CancellationToken cancellationToken = default);
}
