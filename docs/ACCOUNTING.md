# Accounting Specifications

## 1. Chart of Accounts & Group Tree
Double-entry accounting with primary account groups:
- **Assets**
  - Cash (Cash in Hand, Cash Drawer)
  - Bank Accounts (Current Accounts, UPI Settlement Accounts, Card Clearing)
  - Debtors / Customers (Customer Balances)
  - Stock / Inventory (Food Stock, Beverage Stock, Supplies)
- **Liabilities**
  - Creditors / Suppliers
  - Duties & Taxes (GST Output: CGST Payable, SGST Payable; GST Input Credit)
  - Loans & Advances
- **Capital / Equity**
  - Owner Equity / Retained Earnings
- **Income**
  - Food Sales
  - Beverage Sales
  - Service Charge Collected
  - Other Income
- **Expenses**
  - Purchases (Raw Ingredients, Groceries, Packaging)
  - Salaries & Wages
  - Rent, Electricity & Water
  - Gas & Utilities
  - Repairs & Maintenance
  - Discounts & Round-Off Adjustments

## 2. Automatic Posting Rules
- **Cash Sale**:
  - Debit: Cash / Cash Drawer
  - Credit: Food/Beverage Sales
  - Credit: GST Output (CGST & SGST)
- **UPI / Card Sale**:
  - Debit: Bank / Payment Clearing Account
  - Credit: Sales
  - Credit: GST Output
- **Credit Sale (Udhari)**:
  - Debit: Customer Account (Debtors)
  - Credit: Sales
  - Credit: GST Output
- **Customer Recovery (Receipt)**:
  - Debit: Cash / Bank
  - Credit: Customer Account
- **Supplier Purchase**:
  - Debit: Purchase / Stock Inventory
  - Debit: GST Input Credit
  - Credit: Supplier Account (Creditors)

## 3. Voucher Rules & Invariants
- Every voucher must balance: Total Debits == Total Credits.
- Minimum 2 voucher lines per voucher.
- Voucher dates must fall within an open Financial Year and not inside locked accounting periods.
- Sequential numbering series per voucher type (Sales, Receipt, Payment, Journal, Contra, Purchase, Expense).

## 4. Reports Definitions
- **Day Book**: chronological listing of all vouchers posted on a selected date.
- **Cash Book & Bank Book**: cash and bank transactions with opening and closing balances.
- **Ledger**: detailed account statement showing debits, credits, and running balance for any account.
- **Trial Balance**: summary of all accounts with debit and credit totals verifying mathematical accuracy.
- **Profit & Loss**: income versus expense calculation for a specified date range.
- **Balance Sheet**: financial position statement (Assets = Liabilities + Equity).
- **Customer Outstanding & Ageing**: 0-30, 31-60, 61-90, 90+ days aging buckets.
- **GST Summary**: GSTR-1 matching sales with tax rates and CGST/SGST breakdown.
