using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMS.Application.Common.Interfaces.UiData;
using RestaurantMS.Application.Common.Models;
using RestaurantMS.Domain.Constants;
using RestaurantMS.Web.Models;

namespace RestaurantMS.Web.Controllers;

[Authorize]
public class CustomersController : Controller
{
    private readonly ICustomersUiData _customersUiData;

    public CustomersController(ICustomersUiData customersUiData)
    {
        _customersUiData = customersUiData;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var request = new DataTableRequest { Start = 0, Length = 50 };
        var pagedCustomers = await _customersUiData.GetCustomersAsync(request, cancellationToken);
        var customers = pagedCustomers.Items;

        var viewModel = new CustomerListViewModel
        {
            Customers = customers,
            TotalOutstanding = customers.Sum(c => c.OutstandingBalance),
            TotalCreditCustomers = customers.Count(c => c.OutstandingBalance > 0),
            TotalCreditLimit = customers.Sum(c => c.CreditLimit)
        };

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Create()
    {
        var viewModel = new CustomerFormViewModel();
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CustomerFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        TempData["SuccessMessage"] = $"Customer '{model.FullName}' added successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Ledger(long id, CancellationToken cancellationToken)
    {
        var customer = await _customersUiData.GetCustomerByIdAsync(id, cancellationToken);
        var entries = await _customersUiData.GetCustomerLedgerAsync(id, null, null, cancellationToken);

        var viewModel = new CustomerLedgerViewModel
        {
            CustomerId = id,
            CustomerName = customer?.Name ?? "Ramesh Patil",
            MobileNumber = customer?.Mobile ?? "9822012345",
            CurrentBalance = customer?.OutstandingBalance ?? 4250,
            CreditLimit = customer?.CreditLimit ?? 10000,
            Entries = entries
        };

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Receipt(string id)
    {
        var viewModel = new CustomerReceiptViewModel
        {
            CustomerId = id,
            CustomerName = "Ramesh Patil",
            CurrentOutstanding = 4250,
            Amount = 4250
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Receipt(CustomerReceiptViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        TempData["SuccessMessage"] = $"Payment of ₹{model.Amount:N2} received for {model.CustomerName}!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Ageing(CancellationToken cancellationToken)
    {
        var ageings = await _customersUiData.GetCustomerAgeingAsync(cancellationToken);

        var viewModel = new CustomerAgeingViewModel
        {
            CustomerAgeings = ageings,
            TotalBucket0To30 = ageings.Sum(a => a.Bucket0To30),
            TotalBucket31To60 = ageings.Sum(a => a.Bucket31To60),
            TotalBucket61To90 = ageings.Sum(a => a.Bucket61To90),
            TotalBucketOver90 = ageings.Sum(a => a.BucketOver90),
            GrandTotalOutstanding = ageings.Sum(a => a.TotalOutstanding)
        };

        return View(viewModel);
    }
}
