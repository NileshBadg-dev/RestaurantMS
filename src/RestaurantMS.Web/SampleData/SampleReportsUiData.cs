using RestaurantMS.Application.Common.Interfaces.UiData;
using RestaurantMS.Application.Common.Models.UiData;

namespace RestaurantMS.Web.SampleData;

public class SampleReportsUiData : IReportsUiData
{
    private static readonly List<ReportHubCardDto> ReportCards =
    [
        new("RPT01", "1. Sales & GST Summary", "विक्री आणि जीएसटी सारांश", "Gross sales, GST collected (CGST/SGST), expenses, estimated net profit", "एकूण विक्री, गोळा केलेला जीएसटी आणि अंदाजे निव्वळ नफा", "bi-calculator", "Sales"),
        new("RPT02", "2. GST Filing Data", "जीएसटी विवरण डेटा", "Taxable value, CGST, SGST and total tax per tax rate (5%, 12%, 18%)", "करपात्र मूल्य, सीजीएसटी, एसजीएसटी आणि कर दरानुसार कर", "bi-receipt-cutoff", "Financial"),
        new("RPT03", "3. Sales by Menu Category", "मेनू श्रेणीनुसार विक्री", "Units sold and sales value per category (Starters, Main, Biryani, Breads, Beverages)", "श्रेणीनुसार विक्री संख्या आणि महसूल", "bi-pie-chart", "Sales"),
        new("RPT04", "4. Cashier & Staff Sales", "कॅशियर आणि कर्मचारी विक्री", "Bills generated and sales value per user / cashier / waiter", "प्रत्येक कर्मचारी व कॅशियरने केलेली विक्री", "bi-person-badge", "Operations"),
        new("RPT05", "5. Revenue Trend", "दैनिक महसूल कल", "Daily sales for the last 7 days or chosen period with growth comparison", "गेल्या ७ दिवसांचा दैनिक विक्री महसूल", "bi-graph-up-arrow", "Sales"),
        new("RPT06", "6. Payment Method Split", "पेमेंट पद्धतीनुसार विभागणी", "Sales split across Cash, UPI, Card, Udhari and Split payments", "रोकड, यूपीआय, कार्ड, उधारी आणि स्प्लिट पेमेंट", "bi-wallet2", "Financial"),
        new("RPT07", "7. Orders & Billing History", "ऑर्डर्स आणि बिलिंग इतिहास", "All bills with date, customer, channel, items, total, payment, status; view & reprint", "सर्व बिलांची माहिती, पुनर्मुद्रण आणि तपशील", "bi-clock-history", "Sales"),
        new("RPT08", "8. Expenses Outflow", "खर्च आणि आउटफ्लो", "Outflow by category and payment method with totals (Raw materials, Utilities, Rent)", "खर्चाची वर्गवारी आणि पेमेंट पद्धत", "bi-cash-stack", "Financial"),
        new("RPT09", "9. Inventory & Low Stock", "साठा आणि कमी साठा सूचना", "Current stock, low-stock alerts, raw-material consumption, stock log", "सध्याचा साठा, कमी साठा सूचना आणि वापर", "bi-boxes", "Inventory"),
        new("RPT10", "10. Purchases by Supplier", "पुरवठादार खरेदी नोंद", "Purchases by supplier, material, quantity, rate, tax and date", "पुरवठादारानुसार खरेदी, साहित्य आणि दर", "bi-truck", "Inventory"),
        new("RPT11", "11. Customers Spend & Ledger", "ग्राहक खर्च आणि उधारी", "Customers, order count, total spent and outstanding udhari balance", "ग्राहक यादी, एकूण खर्च आणि उधारी रक्कम", "bi-people", "Operations"),
        new("RPT12", "12. Table Time & Turnover (TAT)", "टेबल वेळ आणि टर्नओव्हर", "Seated, bill paid, freed, time on table, bill-to-leave time; average and longest stay", "टेबल वेळ, बिल भरल्यानंतरचा वेळ आणि सरासरी मुक्काम", "bi-hourglass-split", "Operations"),
        new("RPT13", "13. KOT Preparation Time", "किचन केओटी वेळ", "KOT sent, started, ready, served, wait to start, KOT time; fastest, slowest; by hour", "केओटी पाठवले, सुरू केले, तयार आणि वाढले - वेळ विश्लेषण", "bi-fire", "Operations"),
        new("RPT14", "14. Bill Delivery Log", "बिल वितरण नोंद (WhatsApp)", "Bills sent on WhatsApp and email with status, delivery timestamp, failure reason, resend", "व्हॉट्सॲप आणि ईमेलवर पाठवलेल्या बिलांची नोंद", "bi-whatsapp", "Operations"),
        new("RPT15", "15. Discounts & Concessions", "सूट आणि सवलती अहवाल", "Today's discounts given and list of discounted bills with cashier, %, and reason", "दिलेली सवलत, व्यवस्थापक मंजुरी आणि कारणे", "bi-percent", "Financial"),
        new("RPT16", "16. Peak Hours Analysis", "गर्दीच्या वेळेचे विश्लेषण", "Sales and orders by hour of the day; busiest and quietest hours", "दिवसाच्या तासांनुसार ऑर्डर्स आणि महसूल", "bi-bar-chart", "Sales"),
        new("RPT17", "17. Returns & Cancellations", "रद्द ऑर्डर्स आणि परतावे", "Edited, cancelled and refunded bills with reason, amount, staff, and approver", "रद्द केलेली बिले, आयटम्स, कारणे आणि मंजुरी", "bi-x-circle", "Operations")
    ];

    public Task<IReadOnlyList<ReportHubCardDto>> GetReportHubCardsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<ReportHubCardDto>>(ReportCards);
    }

    public Task<ReportResultDto> GetReportDataAsync(string reportKey, DateTime? from = null, DateTime? to = null, string? filter = null, CancellationToken cancellationToken = default)
    {
        var card = ReportCards.FirstOrDefault(r => r.Key.Equals(reportKey, StringComparison.OrdinalIgnoreCase)) ?? ReportCards.First();
        var code = card.Key.ToUpperInvariant();

        List<string> headers;
        List<IReadOnlyList<string>> rows;
        Dictionary<string, string> totals;

        switch (code)
        {
            case "RPT01": // Sales & GST Summary
                headers = new List<string> { "Metric", "Dine-In", "Parcel", "Online", "Total (₹)" };
                rows = new List<IReadOnlyList<string>>
                {
                    new List<string> { "Gross Sales", "₹ 485,000.00", "₹ 195,000.00", "₹ 142,000.00", "₹ 822,000.00" },
                    new List<string> { "Discounts Given", "₹ 12,000.00", "₹ 3,500.00", "₹ 3,000.00", "₹ 18,500.00" },
                    new List<string> { "Net Taxable Sales", "₹ 473,000.00", "₹ 191,500.00", "₹ 139,000.00", "₹ 803,500.00" },
                    new List<string> { "CGST (2.5%)", "₹ 11,825.00", "₹ 4,787.50", "₹ 3,475.00", "₹ 20,087.50" },
                    new List<string> { "SGST (2.5%)", "₹ 11,825.00", "₹ 4,787.50", "₹ 3,475.00", "₹ 20,087.50" },
                    new List<string> { "Total GST Collected", "₹ 23,650.00", "₹ 9,575.00", "₹ 6,950.00", "₹ 40,175.00" },
                    new List<string> { "Total Invoiced Revenue", "₹ 496,650.00", "₹ 201,075.00", "₹ 145,950.00", "₹ 843,675.00" },
                    new List<string> { "Direct COGS (Food)", "₹ 245,000.00", "₹ 98,000.00", "₹ 72,000.00", "₹ 415,000.00" },
                    new List<string> { "Estimated Gross Margin", "50.5%", "50.9%", "49.6%", "50.6%" }
                };
                totals = new Dictionary<string, string>
                {
                    ["Net Taxable Value"] = "₹ 803,500.00",
                    ["Total GST (CGST+SGST)"] = "₹ 40,175.00",
                    ["Total Invoiced"] = "₹ 843,675.00",
                    ["Estimated Net Profit"] = "₹ 139,775.00 (16.6%)"
                };
                break;

            case "RPT02": // GST Filing Data
                headers = new List<string> { "GST Rate", "Description", "Taxable Turnover (₹)", "CGST (₹)", "SGST (₹)", "Total Tax (₹)", "Gross Invoiced (₹)" };
                rows = new List<IReadOnlyList<string>>
                {
                    new List<string> { "5% (2.5%+2.5%)", "Food & Restaurant Services (Non-AC/AC)", "₹ 745,000.00", "₹ 18,625.00", "₹ 18,625.00", "₹ 37,250.00", "₹ 782,250.00" },
                    new List<string> { "12% (6%+6%)", "Packaged Sweets / Bakery Goods", "₹ 42,500.00", "₹ 2,550.00", "₹ 2,550.00", "₹ 5,100.00", "₹ 47,600.00" },
                    new List<string> { "18% (9%+9%)", "Aerated Beverages & Water Bottles", "₹ 16,000.00", "₹ 1,440.00", "₹ 1,440.00", "₹ 2,880.00", "₹ 18,880.00" },
                    new List<string> { "0% (Exempt)", "Fresh Milk & Unprocessed Dairy", "₹ 0.00", "₹ 0.00", "₹ 0.00", "₹ 0.00", "₹ 0.00" }
                };
                totals = new Dictionary<string, string>
                {
                    ["Total Taxable Turnover"] = "₹ 803,500.00",
                    ["Total CGST Output"] = "₹ 22,615.00",
                    ["Total SGST Output"] = "₹ 22,615.00",
                    ["Total Tax Payable"] = "₹ 45,230.00"
                };
                break;

            case "RPT03": // Sales by Menu Category
                headers = new List<string> { "Category Name", "Dishes Count", "Quantity Sold", "Sales Value (₹)", "Contribution %", "Avg Price (₹)" };
                rows = new List<IReadOnlyList<string>>
                {
                    new List<string> { "Main Course (Gravies & Dal)", "14", "1,240 qty", "₹ 298,000.00", "36.2%", "₹ 240.32" },
                    new List<string> { "Biryani & Rice", "6", "980 qty", "₹ 245,000.00", "29.8%", "₹ 250.00" },
                    new List<string> { "Starters & Appetizers", "8", "620 qty", "₹ 142,600.00", "17.3%", "₹ 230.00" },
                    new List<string> { "Breads (Roti, Naan, Kulcha)", "5", "2,150 qty", "₹ 75,250.00", "9.1%", "₹ 35.00" },
                    new List<string> { "Beverages & Desserts", "6", "840 qty", "₹ 61,150.00", "7.4%", "₹ 72.80" }
                };
                totals = new Dictionary<string, string>
                {
                    ["Total Categories"] = "5",
                    ["Total Units Sold"] = "5,830 qty",
                    ["Total Category Sales"] = "₹ 822,000.00"
                };
                break;

            case "RPT04": // Cashier & Staff Sales
                headers = new List<string> { "Staff Code", "Staff Name", "Role / Designation", "Bills Handled", "Gross Sales (₹)", "Avg Bill Value (₹)", "Cash Collected", "UPI/Card Handled" };
                rows = new List<IReadOnlyList<string>>
                {
                    new List<string> { "EMP-005", "Prakash Shinde", "Cashier", "482 bills", "₹ 412,000.00", "₹ 854.77", "₹ 185,000.00", "₹ 227,000.00" },
                    new List<string> { "EMP-001", "Suresh Patil", "Head Waiter / Captain", "294 orders", "₹ 284,500.00", "₹ 967.68", "₹ 94,000.00", "₹ 190,500.00" },
                    new List<string> { "EMP-003", "Meena Jadhav", "Captain", "210 orders", "₹ 198,000.00", "₹ 942.85", "₹ 62,000.00", "₹ 136,000.00" },
                    new List<string> { "EMP-002", "Anil Deshmukh", "Waiter", "165 orders", "₹ 148,200.00", "₹ 898.18", "₹ 48,000.00", "₹ 100,200.00" }
                };
                totals = new Dictionary<string, string>
                {
                    ["Total Staff Active"] = "4",
                    ["Total Bills Handled"] = "1,151",
                    ["Total Staff Turnover"] = "₹ 1,042,700.00"
                };
                break;

            case "RPT05": // Revenue Trend
                headers = new List<string> { "Date", "Day of Week", "Dine-In Sales (₹)", "Parcel Sales (₹)", "Online Sales (₹)", "Total Sales (₹)", "Order Count" };
                rows = new List<IReadOnlyList<string>>
                {
                    new List<string> { "11-Nov-2026", "Wednesday", "₹ 64,200.00", "₹ 24,100.00", "₹ 18,500.00", "₹ 106,800.00", "124" },
                    new List<string> { "12-Nov-2026", "Thursday", "₹ 68,500.00", "₹ 26,400.00", "₹ 19,200.00", "₹ 114,100.00", "138" },
                    new List<string> { "13-Nov-2026", "Friday", "₹ 76,000.00", "₹ 31,000.00", "₹ 24,800.00", "₹ 131,800.00", "156" },
                    new List<string> { "14-Nov-2026", "Saturday", "₹ 98,400.00", "₹ 42,500.00", "₹ 34,200.00", "₹ 175,100.00", "212" },
                    new List<string> { "15-Nov-2026", "Sunday", "₹ 112,000.00", "₹ 48,000.00", "₹ 38,500.00", "₹ 198,500.00", "248" },
                    new List<string> { "16-Nov-2026", "Monday", "₹ 58,100.00", "₹ 21,500.00", "₹ 16,400.00", "₹ 96,000.00", "110" },
                    new List<string> { "17-Nov-2026", "Tuesday (Today)", "₹ 62,400.00", "₹ 23,800.00", "₹ 17,900.00", "₹ 104,100.00", "128" }
                };
                totals = new Dictionary<string, string>
                {
                    ["7-Day Total Sales"] = "₹ 926,400.00",
                    ["7-Day Orders"] = "1,116",
                    ["Highest Day"] = "Sunday 15-Nov (₹ 198,500.00)"
                };
                break;

            case "RPT06": // Payment Method Split
                headers = new List<string> { "Payment Method", "Transactions Count", "Amount Collected (₹)", "Share of Total %", "Status" };
                rows = new List<IReadOnlyList<string>>
                {
                    new List<string> { "UPI / Dynamic QR (PhonePe/GPay)", "612 txn", "₹ 448,500.00", "53.2%", "Settled to Bank" },
                    new List<string> { "Cash (Till Drawer)", "345 txn", "₹ 246,000.00", "29.1%", "Counted in Safe" },
                    new List<string> { "Credit / Debit Card (POS Machine)", "98 txn", "₹ 88,400.00", "10.5%", "Settled by Terminal" },
                    new List<string> { "Udhari (Customer Credit Ledger)", "38 txn", "₹ 42,100.00", "5.0%", "Pending Customer Due" },
                    new List<string> { "Split Payment (Cash + UPI)", "24 txn", "₹ 18,675.00", "2.2%", "Mixed Settlement" }
                };
                totals = new Dictionary<string, string>
                {
                    ["Total Transactions"] = "1,117",
                    ["Total Invoiced"] = "₹ 843,675.00",
                    ["Digital Payment Share"] = "63.7%"
                };
                break;

            case "RPT07": // Orders & Billing History
                headers = new List<string> { "Bill #", "Date & Time", "Channel", "Table / Token", "Customer Name", "Items Summary", "Total (₹)", "Payment", "Status" };
                rows = new List<IReadOnlyList<string>>
                {
                    new List<string> { "TAX-0842", "17-Nov 14:30", "Dine-In", "Table T4", "Ramesh Patil", "2x Paneer Butter Masala, 4x Butter Naan", "₹ 777.00", "Cash", "Settled" },
                    new List<string> { "TAX-0841", "17-Nov 14:15", "Parcel", "Token #21", "Walk-in Guest", "1x Chicken Biryani, 1x Sweet Lassi", "₹ 420.00", "UPI", "Settled" },
                    new List<string> { "TAX-0840", "17-Nov 13:50", "Online", "ZM-841", "Pooja Verma", "1x Veg Biryani, 2x Tandoori Roti", "₹ 283.50", "Online", "Dispatched" },
                    new List<string> { "TAX-0839", "17-Nov 13:30", "Dine-In", "Table T2", "Amit Deshmukh", "3x Chicken Biryani, 1x Dal Tadka", "₹ 1,197.00", "Udhari", "Credit Settled" },
                    new List<string> { "TAX-0838", "17-Nov 13:00", "Dine-In", "Table T7", "Walk-in Guest", "2x Masala Dosa, 2x Filter Coffee", "₹ 240.00", "Card", "Settled" }
                };
                totals = new Dictionary<string, string>
                {
                    ["Showing Bills"] = "Last 5 of 842",
                    ["Total Sales"] = "₹ 2,917.50",
                    ["Reprint Status"] = "Ready on 80mm/50mm"
                };
                break;

            case "RPT08": // Expenses Outflow
                headers = new List<string> { "Voucher #", "Date", "Expense Category", "Paid To / Vendor", "Payment Mode", "Description", "Amount (₹)" };
                rows = new List<IReadOnlyList<string>>
                {
                    new List<string> { "EXP-2026-041", "16-Nov-2026", "Raw Materials & Vegetables", "Green Grocers APMC", "UPI", "Fresh weekly vegetable stock", "₹ 14,500.00" },
                    new List<string> { "EXP-2026-042", "15-Nov-2026", "Commercial LPG Gas", "Bharat Gas Agency", "Bank Transfer", "4x 19kg commercial cylinders", "₹ 7,600.00" },
                    new List<string> { "EXP-2026-043", "14-Nov-2026", "Staff Salary Advance", "Suresh Patil", "Cash", "Emergency family advance", "₹ 5,000.00" },
                    new List<string> { "EXP-2026-044", "12-Nov-2026", "Maintenance & Repairs", "Cool Care Services", "Cash", "AC Servicing in dining hall", "₹ 2,800.00" },
                    new List<string> { "EXP-2026-045", "10-Nov-2026", "Packaging Materials", "Super Poly Pack", "UPI", "Parcel takeaway boxes & bags", "₹ 6,200.00" }
                };
                totals = new Dictionary<string, string>
                {
                    ["Total Expenses Recorded"] = "₹ 36,100.00",
                    ["Paid via Cash"] = "₹ 7,800.00",
                    ["Paid via Bank/UPI"] = "₹ 28,300.00"
                };
                break;

            case "RPT09": // Inventory & Low Stock
                headers = new List<string> { "Item Code", "Material Name", "Category", "Current Stock", "Reorder Level", "Unit", "Valuation (₹)", "Status" };
                rows = new List<IReadOnlyList<string>>
                {
                    new List<string> { "RAW-001", "Basmati Rice (Royal)", "Grains", "8.0", "15.0", "Kg", "₹ 680.00", "CRITICAL LOW" },
                    new List<string> { "RAW-002", "Fresh Paneer (Malai)", "Dairy", "4.5", "10.0", "Kg", "₹ 1,575.00", "LOW STOCK" },
                    new List<string> { "RAW-003", "Cooking Oil (Refined Sunflower)", "Oils", "32.0", "20.0", "Ltr", "₹ 4,480.00", "NORMAL" },
                    new List<string> { "RAW-004", "Chicken (Fresh Curry Cut)", "Poultry", "6.0", "12.0", "Kg", "₹ 1,320.00", "LOW STOCK" },
                    new List<string> { "RAW-005", "Wheat Flour (Atta)", "Grains", "45.0", "25.0", "Kg", "₹ 1,620.00", "NORMAL" },
                    new List<string> { "RAW-006", "Commercial LPG Cylinder", "Fuel", "2.0", "2.0", "Nos", "₹ 3,800.00", "NORMAL" }
                };
                totals = new Dictionary<string, string>
                {
                    ["Total Raw Items"] = "24 items",
                    ["Low Stock Alerts"] = "3 critical items",
                    ["Total Stock Valuation"] = "₹ 84,250.00"
                };
                break;

            case "RPT10": // Purchases by Supplier
                headers = new List<string> { "Invoice #", "Date", "Supplier / Vendor", "Items Supplied", "Quantity", "Tax Rate", "Total Invoice (₹)", "Payment Status" };
                rows = new List<IReadOnlyList<string>>
                {
                    new List<string> { "PUR-2026-101", "14-Nov-2026", "Green Grocers APMC", "Onion, Tomato, Potato, Ginger", "120 kg", "0%", "₹ 12,400.00", "PAID (UPI)" },
                    new List<string> { "PUR-2026-102", "12-Nov-2026", "Metro Cash & Carry", "Basmati Rice, Spices, Oil", "85 kg/ltr", "5%", "₹ 24,800.00", "PAID (NEFT)" },
                    new List<string> { "PUR-2026-103", "10-Nov-2026", "Kalyani Dairy Farms", "Paneer, Curd, Butter, Cream", "45 kg", "5%", "₹ 18,200.00", "PENDING (₹ 18,200)" },
                    new List<string> { "PUR-2026-104", "08-Nov-2026", "Fresh Meat & Poultry Co.", "Curry Cut Broiler Chicken", "50 kg", "0%", "₹ 11,000.00", "PAID (Cash)" }
                };
                totals = new Dictionary<string, string>
                {
                    ["Total Inward Purchases"] = "₹ 66,400.00",
                    ["Total Paid"] = "₹ 48,200.00",
                    ["Pending to Suppliers"] = "₹ 18,200.00"
                };
                break;

            case "RPT11": // Customers Spend & Ledger
                headers = new List<string> { "Customer Code", "Customer Name", "Mobile Number", "Orders Count", "Total Spent (₹)", "Credit Limit (₹)", "Outstanding Udhari (₹)", "Risk Level" };
                rows = new List<IReadOnlyList<string>>
                {
                    new List<string> { "CUST-001", "Ramesh Patil", "9822112233", "24 visits", "₹ 28,450.00", "₹ 15,000.00", "₹ 4,200.00", "Good" },
                    new List<string> { "CUST-002", "Amit Deshmukh", "9822334455", "18 visits", "₹ 19,200.00", "₹ 10,000.00", "₹ 6,800.00", "Moderate" },
                    new List<string> { "CUST-003", "Ganesh Kulkarni", "9822556677", "12 visits", "₹ 14,100.00", "₹ 5,000.00", "₹ 0.00", "Zero Due" },
                    new List<string> { "CUST-004", "Sunil Shinde", "9822778899", "9 visits", "₹ 9,800.00", "₹ 8,000.00", "₹ 7,500.00", "Near Limit" },
                    new List<string> { "CUST-005", "Pooja Verma", "9822990011", "31 visits", "₹ 34,600.00", "₹ 20,000.00", "₹ 1,200.00", "VIP Regular" }
                };
                totals = new Dictionary<string, string>
                {
                    ["Total Registered Customers"] = "142",
                    ["Total Lifetime Spend"] = "₹ 485,000.00",
                    ["Total Outstanding Udhari"] = "₹ 19,700.00"
                };
                break;

            case "RPT12": // Table Time & Turnover (TAT)
                headers = new List<string> { "Table", "Zone / Section", "Table Seated", "Bill Paid", "Table Freed", "Time on Table", "Bill-to-Leave Time", "Bill Amount (₹)" };
                rows = new List<IReadOnlyList<string>>
                {
                    new List<string> { "T1", "AC Hall", "12:15 PM", "12:55 PM", "01:02 PM", "47 min", "7 min", "₹ 860.00" },
                    new List<string> { "T2", "AC Hall", "12:30 PM", "01:10 PM", "01:18 PM", "48 min", "8 min", "₹ 1,240.00" },
                    new List<string> { "T5", "Garden", "01:00 PM", "02:05 PM", "02:15 PM", "1h 15m", "10 min", "₹ 3,480.00" },
                    new List<string> { "T7", "Garden", "01:15 PM", "02:35 PM", "02:47 PM", "1h 32m", "12 min", "₹ 2,010.00" },
                    new List<string> { "T9", "Family", "01:45 PM", "02:22 PM", "02:28 PM", "43 min", "6 min", "₹ 720.00" },
                    new List<string> { "T10", "Family", "02:00 PM", "02:35 PM", "02:40 PM", "40 min", "5 min", "₹ 1,515.00" }
                };
                totals = new Dictionary<string, string>
                {
                    ["Average Time on Table"] = "48.2 minutes",
                    ["Average Bill-to-Leave Time"] = "8.0 minutes",
                    ["Longest Stay Today"] = "Table T7 (1h 32m · Garden)",
                    ["Shortest Turnover"] = "Table T10 (40m · Family)"
                };
                break;

            case "RPT13": // KOT Preparation Time
                headers = new List<string> { "KOT #", "Table / Token", "KOT Sent", "Kitchen Started", "Wait to Start", "Marked Ready", "KOT Time (Prep)", "Marked Served", "Total Service Time", "Station" };
                rows = new List<IReadOnlyList<string>>
                {
                    new List<string> { "KOT-101", "Table T1", "12:20 PM", "12:22 PM", "2 min", "12:31 PM", "9 min", "12:34 PM", "14 min", "Curry / Tandoor" },
                    new List<string> { "KOT-102", "Table T2", "12:35 PM", "12:37 PM", "2 min", "12:48 PM", "11 min", "12:51 PM", "16 min", "Main / Breads" },
                    new List<string> { "KOT-103", "Parcel #14", "12:45 PM", "12:47 PM", "2 min", "12:56 PM", "9 min", "12:58 PM", "13 min", "Biryani" },
                    new List<string> { "KOT-104", "Table T5", "01:05 PM", "01:09 PM", "4 min", "01:23 PM", "14 min", "01:26 PM", "21 min", "Tandoor" },
                    new List<string> { "KOT-105", "Table T7", "01:20 PM", "01:22 PM", "2 min", "01:33 PM", "11 min", "01:36 PM", "16 min", "South Indian" }
                };
                totals = new Dictionary<string, string>
                {
                    ["Average Wait to Start"] = "2.4 minutes",
                    ["Average KOT Prep Time"] = "10.8 minutes",
                    ["Average Total Service Time"] = "16.0 minutes",
                    ["Fastest Order Prepared"] = "KOT-101 (9 min)",
                    ["Slowest Order Prepared"] = "KOT-104 (14 min)"
                };
                break;

            case "RPT14": // Bill Delivery Log (WhatsApp & Email)
                headers = new List<string> { "Invoice #", "Customer", "Phone Number", "Dispatch Channel", "Sent Timestamp", "Delivery Status", "Failure Reason", "Actions" };
                rows = new List<IReadOnlyList<string>>
                {
                    new List<string> { "TAX-0842", "Ramesh Patil", "9822112233", "WhatsApp API", "17-Nov 14:31", "DELIVERED", "-", "View Receipt" },
                    new List<string> { "TAX-0841", "Walk-in Guest", "9822334455", "WhatsApp API", "17-Nov 14:16", "READ", "-", "View Receipt" },
                    new List<string> { "TAX-0839", "Amit Deshmukh", "9822556677", "WhatsApp + Email", "17-Nov 13:32", "DELIVERED", "-", "Resend" },
                    new List<string> { "TAX-0835", "Sunil Shinde", "9822001122", "WhatsApp API", "17-Nov 12:45", "FAILED", "Number not on WhatsApp", "Resend SMS" },
                    new List<string> { "TAX-0830", "Pooja Verma", "9822990011", "WhatsApp API", "17-Nov 12:10", "READ", "-", "View Receipt" }
                };
                totals = new Dictionary<string, string>
                {
                    ["Total Digital Bills Sent"] = "348 bills",
                    ["Successfully Delivered"] = "341 (98%)",
                    ["Failed Dispatches"] = "7 (2%)"
                };
                break;

            case "RPT15": // Discounts & Concessions
                headers = new List<string> { "Bill #", "Date & Time", "Customer Name", "Cashier", "Bill Amount (₹)", "Discount (₹)", "Discount %", "Reason / Scheme", "Approved By" };
                rows = new List<IReadOnlyList<string>>
                {
                    new List<string> { "TAX-0839", "17-Nov 13:30", "Amit Deshmukh", "Prakash Shinde", "₹ 1,260.00", "₹ 63.00", "5.0%", "Regular Customer Courtesy", "Manager (Vinod)" },
                    new List<string> { "TAX-0824", "17-Nov 12:00", "Pooja Verma", "Prakash Shinde", "₹ 950.00", "₹ 95.00", "10.0%", "Corporate Birthday Offer", "Manager (Vinod)" },
                    new List<string> { "TAX-0792", "16-Nov 21:15", "Dr. Kulkarni", "Prakash Shinde", "₹ 2,400.00", "₹ 240.00", "10.0%", "Doctor / Hospital Privilege", "Owner (Admin)" },
                    new List<string> { "TAX-0785", "16-Nov 20:30", "Walk-in Guest", "Prakash Shinde", "₹ 820.00", "₹ 50.00", "6.1%", "Slight Delay Compensation", "Manager (Vinod)" }
                };
                totals = new Dictionary<string, string>
                {
                    ["Today Discounts Count"] = "4 bills",
                    ["Total Discounts Given"] = "₹ 448.00",
                    ["Average Discount %"] = "7.8%"
                };
                break;

            case "RPT16": // Peak Hours Sales
                headers = new List<string> { "Hour Window", "Time of Day", "Dine-In Bills", "Parcel Bills", "Total Orders", "Hourly Sales (₹)", "Occupancy %", "Volume Category" };
                rows = new List<IReadOnlyList<string>>
                {
                    new List<string> { "11:00 AM - 12:00 PM", "Pre-Lunch", "4", "6", "10", "₹ 6,400.00", "25%", "Quiet" },
                    new List<string> { "12:00 PM - 01:00 PM", "Lunch Start", "14", "12", "26", "₹ 21,800.00", "65%", "Moderate" },
                    new List<string> { "01:00 PM - 02:00 PM", "Lunch Peak", "26", "18", "44", "₹ 42,600.00", "94%", "PEAK RUSH" },
                    new List<string> { "02:00 PM - 03:00 PM", "Lunch End", "16", "9", "25", "₹ 19,500.00", "55%", "Moderate" },
                    new List<string> { "03:00 PM - 07:00 PM", "Afternoon Prep", "6", "11", "17", "₹ 11,200.00", "18%", "Quiet" },
                    new List<string> { "07:00 PM - 08:00 PM", "Dinner Start", "18", "14", "32", "₹ 28,400.00", "70%", "Moderate" },
                    new List<string> { "08:00 PM - 09:30 PM", "Dinner Peak", "32", "22", "54", "₹ 56,200.00", "100%", "SUPER PEAK" },
                    new List<string> { "09:30 PM - 11:00 PM", "Dinner Late", "18", "8", "26", "₹ 22,000.00", "50%", "Moderate" }
                };
                totals = new Dictionary<string, string>
                {
                    ["Busiest Period"] = "08:00 PM - 09:30 PM (54 orders · ₹ 56,200)",
                    ["Quietest Period"] = "03:00 PM - 07:00 PM (17 orders · ₹ 11,200)",
                    ["Total Daily Revenue"] = "₹ 208,100.00"
                };
                break;

            case "RPT17": // Returns & Cancellations
                headers = new List<string> { "Void #", "Original Order/Bill", "Table / Token", "Cancelled Item", "Qty", "Amount (₹)", "Staff / Waiter", "Reason for Cancellation", "Approved By" };
                rows = new List<IReadOnlyList<string>>
                {
                    new List<string> { "VOID-012", "ORD-184 / KOT-92", "Table T3", "Paneer Tikka Starter", "1", "₹ 240.00", "Anil Deshmukh", "Guest changed mind before cooking", "Manager (Vinod)" },
                    new List<string> { "VOID-011", "ORD-172 / KOT-84", "Table T5", "Butter Naan", "2", "₹ 90.00", "Suresh Patil", "Accidental extra punch by captain", "Manager (Vinod)" },
                    new List<string> { "VOID-010", "TAX-0812", "Parcel #09", "Chicken Biryani", "1", "₹ 320.00", "Counter (Cashier)", "Customer cancelled takeaway pickup", "Owner (Admin)" },
                    new List<string> { "VOID-009", "ORD-150 / KOT-70", "Table T8", "Sweet Lassi", "1", "₹ 80.00", "Meena Jadhav", "Guest reported excessive sweetness", "Manager (Vinod)" }
                };
                totals = new Dictionary<string, string>
                {
                    ["Total Cancellations Today"] = "4 items",
                    ["Total Value Voided"] = "₹ 730.00",
                    ["Audit Log Status"] = "Logged with timestamp & user ID"
                };
                break;

            default:
                headers = new List<string> { "Code", "Description", "Count", "Value (₹)" };
                rows = new List<IReadOnlyList<string>>
                {
                    new List<string> { "01", "Operational Item", "24", "₹ 18,500.00" }
                };
                totals = new Dictionary<string, string> { ["Total"] = "₹ 18,500.00" };
                break;
        }

        var result = new ReportResultDto(card.Key, card.Title, "Reporting Period: " + (from?.ToString("dd MMM yyyy") ?? "Current Month"), headers, rows, totals);
        return Task.FromResult(result);
    }
}
