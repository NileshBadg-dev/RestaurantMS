using RestaurantMS.Application.Common.Models.Reports;
using RestaurantMS.Domain.Constants;

namespace RestaurantMS.Application.Reports.Services;

public static class ReportsCatalog
{
    public static readonly IReadOnlyList<string> Categories =
    [
        "Sales",
        "Tables",
        "Menu",
        "Parcel",
        "Staff",
        "Payments and Credit",
        "Kitchen",
        "Inventory",
        "Accounts",
        "Tax"
    ];

    public static readonly IReadOnlyList<ReportDefinitionDto> Definitions =
    [
        // SALES
        new("RPT01", "Sales & Revenue Summary", "विक्री व महसूल सारांश", "Gross sales, discounts, GST collected and estimated net margin by channel", "एकूण विक्री, सूट, जीएसटी व चॅनेलनुसार महसूल", "Sales", "bi-graph-up-arrow", Permissions.Reports.Sales, true, "bar", "Channel", "bill"),
        new("RPT05", "Daily Revenue Trend", "दैनिक महसूल कल", "Day-by-day dine-in, parcel and delivery sales trajectory", "दररोजची डाइन-इन, पार्सल आणि डिलिव्हरी विक्री", "Sales", "bi-activity", Permissions.Reports.Sales, true, "line", null, "bill"),
        new("RPT16", "Hourly & Peak Hours Sales", "तासानुसार विक्री व गर्दीचा काळ", "Orders and revenue by hour of the day with peak rush highlight", "दिवसाच्या प्रत्येक तासानुसार ऑर्डर्स आणि विक्री", "Sales", "bi-bar-chart-steps", Permissions.Reports.Sales, true, "bar", "Shift", "bill"),
        new("RPT15", "Discounts & Concessions", "सूट व सवलती अहवाल", "Discounted bills with amount, percentage, cashier, reason and approver", "दिलेल्या सवलती, कारण व व्यवस्थापक मंजुरी", "Sales", "bi-percent", Permissions.Reports.Sales, true, "bar", "Cashier", "bill"),
        new("RPT07", "Bill Register & History", "बिल रजिस्टर व इतिहास", "Complete server-paged bill register with channel, customer, payment and drill-down", "सर्व बिलांची संपूर्ण यादी व तपशील", "Sales", "bi-receipt-cutoff", Permissions.Reports.Sales, false, "bar", "Order Type", "bill"),
        new("RPT18", "Day Closing Summary", "दैनिक क्लोजिंग सारांश", "Shift-wise register opening, cash expected vs counted, variance and closure", "शिफ्टनुसार रोख ताळमेळ व दैनिक क्लोजिंग", "Sales", "bi-clock-history", Permissions.Reports.Sales, true, "bar", "Shift", "bill"),

        // TABLES
        new("RPT19", "Table-Wise Collection", "टेबलनुसार संकलन", "Orders, guest count, average bill and revenue collected per table and zone", "प्रत्येक टेबल व विभागानुसार एकूण विक्री", "Tables", "bi-grid-3x3-gap", Permissions.Reports.Tables, true, "bar", "Zone", "table"),
        new("RPT12", "Table Spent Time & Turnover", "टेबल वेळ व टर्नओव्हर", "Seated to bill paid, bill-to-leave, average and longest stay per table", "टेबलवरील मुक्काम, बिल वेळ व सरासरी कालावधी", "Tables", "bi-hourglass-split", Permissions.Reports.Tables, true, "bar", "Zone", "table"),

        // MENU
        new("RPT20", "Menu Card & Margin Report", "मेनू कार्ड व नफा अहवाल", "Menu items with pricing, tax slab, food cost, contribution margin and availability", "मेनू पदार्थ, दर, कर आणि नफ्याचे प्रमाण", "Menu", "bi-book", Permissions.Reports.Menu, true, "bar", "Category", "item"),
        new("RPT03", "Category-Wise Sales", "श्रेणीनुसार विक्री", "Units sold, revenue share and average selling price per menu category", "मेनू श्रेणीनुसार विक्री व वाटा", "Menu", "bi-pie-chart", Permissions.Reports.Menu, true, "doughnut", null, "item"),
        new("RPT21", "Item-Wise Sales", "पदार्थानुसार विक्री", "Dish-level quantity sold, gross value, GST and ranking across order types", "प्रत्येक पदार्थाची विक्री संख्या व महसूल", "Menu", "bi-list-ol", Permissions.Reports.Menu, true, "bar", "Category", "item"),

        // PARCEL
        new("RPT22", "Parcel & Delivery Report", "पार्सल व डिलिव्हरी अहवाल", "Token-wise parcel & delivery orders, packing charges, prep SLA and payment mode", "पार्सल टोकन, पॅकिंग शुल्क व वितरण वेळ", "Parcel", "bi-bag-check", Permissions.Reports.Parcel, true, "bar", "Order Type", "bill"),

        // STAFF
        new("RPT04", "Waiter-Wise & Cashier Sales", "वेटर व कॅशियर विक्री", "Bills handled, gross sales, average ticket size and tip/incentive metrics", "कर्मचारी व वेटरनुसार विक्री कामगिरी", "Staff", "bi-person-badge", Permissions.Reports.Staff, true, "bar", "Role", "staff"),
        new("RPT23", "Staff Attendance Report", "कर्मचारी हजेरी अहवाल", "Present, absent, half-day, leave and overtime hours by employee and shift", "हजेरी, अनुपस्थिती, अर्धा दिवस व ओव्हरटाईम", "Staff", "bi-calendar2-check", Permissions.Reports.Staff, true, "bar", "Shift", "staff"),
        new("RPT24", "Employee Advances & Recovery", "कर्मचारी अ‍ॅडव्हान्स व वसुली", "Advances disbursed, salary recoveries and pending balances per staff member", "दिलेला अ‍ॅडव्हान्स, पगारातून वसुली व बाकी", "Staff", "bi-cash-coin", Permissions.Reports.Staff, true, "bar", "Department", "staff"),

        // PAYMENTS AND CREDIT
        new("RPT06", "Payment Mode Split", "पेमेंट पद्धतीनुसार विभागणी", "Collection breakdown across Cash, UPI, Card, Split and Udhari", "रोख, यूपीआय, कार्ड, स्प्लिट व उधारी संकलन", "Payments and Credit", "bi-wallet2", Permissions.Reports.Payments, true, "doughnut", null, "bill"),
        new("RPT11", "Credit (Udhari) & Recovery", "उधारी व वसुली अहवाल", "Customer credit posted, payments recovered, credit limit utilization and balance", "ग्राहक उधारी नोंद, वसुली व थकबाकी", "Payments and Credit", "bi-journal-bookmark", Permissions.Reports.Payments, true, "bar", "Status", "customer"),
        new("RPT25", "Customer Credit Ageing", "उधारी कालावधी विश्लेषण", "Overdue Udhari ageing buckets: 0-30, 31-60, 61-90 and 90+ days", "०-३०, ३१-६०, ६१-९० आणि ९०+ दिवस थकबाकी", "Payments and Credit", "bi-calendar-range", Permissions.Reports.Payments, true, "bar", "Risk Bucket", "customer"),
        new("RPT14", "Bill Delivery Log", "बिल वितरण नोंद", "WhatsApp and Email digital bill delivery status, failure reason and resends", "व्हॉट्सअ‍ॅप व ईमेल बिल वितरण स्थिती", "Payments and Credit", "bi-whatsapp", Permissions.Reports.Payments, false, "bar", "Channel", "bill"),

        // KITCHEN
        new("RPT13", "Kitchen KOT Performance", "किचन केओटी कार्यक्षमता", "KOT sent to started, prep time, served SLA and station bottlenecks", "केओटी तयारी वेळ व किचन स्टेशन विश्लेषण", "Kitchen", "bi-fire", Permissions.Reports.Kitchen, true, "bar", "Station", "table"),
        new("RPT17", "Cancelled Bills & Voided KOTs", "रद्द बिले व केओटी नोंद", "Voided items, cancelled bills, staff, reason and manager approval audit", "रद्द केलेली बिले, कारणे व मंजुरी नोंद", "Kitchen", "bi-x-circle", Permissions.Reports.Kitchen, true, "bar", "Reason", "bill"),

        // INVENTORY
        new("RPT09", "Stock Summary & Valuation", "साठा सारांश व मूल्यांकन", "Live stock levels, reorder alerts, expiry tracking and valuation by category", "सध्याचा साठा, कमी साठा व मूल्यांकन", "Inventory", "bi-boxes", Permissions.Reports.Inventory, true, "bar", "Category", "item"),
        new("RPT10", "Purchases, Wastage & Consumption", "खरेदी, नुकसान व वापर अहवाल", "Supplier inward purchases, kitchen consumption and wastage cost analysis", "पुरवठादार खरेदी, वापर व नुकसान विश्लेषण", "Inventory", "bi-truck", Permissions.Reports.Inventory, true, "bar", "Movement Type", "item"),

        // ACCOUNTS
        new("RPT08", "Expenses & Outflow Report", "खर्च व आउटफ्लो अहवाल", "Operating expenses by expense head, vendor and payment mode", "खर्च शीर्षक व पेमेंट पद्धतीनुसार तपशील", "Accounts", "bi-cash-stack", Permissions.Reports.Accounts, true, "bar", "Expense Head", "bill"),
        new("RPT26", "Accounts Financial Summary", "लेखा आर्थिक सारांश", "Day Book, Cash Book, Trial Balance, Profit & Loss and Balance Sheet highlights", "डे बुक, कॅश बुक, ट्रायल बॅलन्स व नफा-तोटा", "Accounts", "bi-journal-text", Permissions.Reports.Accounts, true, "bar", "Statement", "bill"),

        // TAX
        new("RPT02", "GST & Tax Filing Report", "जीएसटी व कर विवरण अहवाल", "Slab-wise taxable value, CGST, SGST, Input Tax Credit and net GST payable", "दरानुसार करपात्र मूल्य, सीजीएसटी व एसजीएसटी", "Tax", "bi-file-earmark-Spreadsheet", Permissions.Reports.Tax, true, "bar", "Tax Slab", "bill")
    ];
}
