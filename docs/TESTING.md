# Billing & Receipt Print Preview — Manual & UI Verification Script

Use this checklist to verify the Billing payment modes (`Cash`, `UPI`, `Card`, `Split`, `Udhari`), server-side calculations, accounting voucher posting, and popup-free iframe receipt / KOT print preview.

## Prerequisites
- Start the web app (`http://localhost:5015`) and sign in as `admin` / `Admin@123`.
- Optional: open `/Settings?tab=billing` and `/Settings?tab=printing` to inspect or toggle:
  - `Hotel UPI ID (VPA)` (`Payments.UpiId`)
  - `Require UPI UTR / Reference Number` (`Payments.UpiReferenceRequired`)
  - `Require Card Last 4 Digits & Approval Code` (`Payments.CardDetailsRequired`)
  - `Default Receipt Paper Size` (`58mm`, `80mm`, `A4`)
  - `Auto-Print Receipt immediately on Bill Settle` (`Printing.AutoPrintOnSettle`)
  - `Auto-Print KOT when sent to Kitchen` (`Kitchen.AutoPrintKot`)

---

## 1. Item Entry, Totals & Order Type
1. Navigate to `/Billing`.
2. Enter short code `M01` and press `Enter` (or click **Browse Menu** and add 2 dishes).
3. Enter a **Discount** (e.g., `20`) and **Packing Charge** (e.g., `15`), change **GST Slab** (`5%` / `12%`).
   - Verify Subtotal, Discount, Packing, CGST+SGST, Round Off, and Payable Total update immediately.
4. Switch order type to **Parcel** or **Delivery**.
   - Verify the Table dropdown hides and `"Parcel / Delivery — No table required"` badge appears.

## 2. Payment Mode: CASH
1. Click **Cash** — verify the Cash button highlights in solid teal and the Cash panel is visible.
2. Verify **Amount Received** defaults to the exact payable total.
3. Click `+500` — verify **Change to return** displays `₹500.00` in teal and **Settle & Print Receipt** is enabled.
4. Enter an amount less than total (e.g., `100`) — verify `"Balance due"` displays in red, inline error reads `"Received amount is less than the bill total"`, and **Settle & Print Receipt** is disabled.
5. Click **Exact**, then click **Settle & Print Receipt**.
   - Verify the receipt preview modal opens in an `<iframe>` (no popup blocked) and triggers `print()` when auto-print is ON.
   - Verify the bill appears in **Bills Settled Today** and a balanced `Sales` voucher appears in `/Accounts/DayBook` & `/Accounts/Vouchers`.

## 3. Payment Mode: UPI
1. Add items on `/Billing` and click **UPI**.
   - Verify the UPI button highlights in solid plum and the live QR displays with the exact payable amount and hotel UPI ID.
2. Notice **Settle & Print Receipt** stays disabled until **Payment Received** is clicked.
3. If `Payments.UpiReferenceRequired` is enabled in `/Settings`, verify leaving UTR blank displays `"UPI reference number is required"`.
4. Enter a UTR (e.g., `UTR4321987654`), click **Payment Received**, and click **Settle & Print Receipt**.
   - Verify receipt prints with the UTR number and balanced voucher debits `1120 Bank / UPI Account`.

## 4. Payment Mode: CARD
1. Add items and click **Card**.
2. Leave Last 4 Digits blank or enter `12` — verify error `"Enter the last 4 digits of the card"` and Settle disabled.
3. Enter `4242`, approval code `AUTH901`, bank `HDFC POS`, and settle.
   - Verify receipt displays `Card (xxxx 4242, Auth: AUTH901, HDFC POS)`.

## 5. Payment Mode: SPLIT
1. Add items (e.g., total `₹500.00`) and click **Split**.
2. Adjust first row to `Cash 200` and second row to `UPI 100`.
   - Verify Remaining shows `₹200.00`, error reads `"Total of payments must equal Rs 500.00. Remaining: Rs 200.00"`, and Settle is disabled.
3. Click **Add Row** — verify the new row auto-fills with `200` (exact remaining!), Remaining becomes `₹0.00`, and Settle enables.
4. Remove a row with `×` and verify Remaining recalculates live.
5. Test a Split row with `Udhari` + `Cash` after selecting a customer — settle and verify balanced multi-debit voucher.

## 6. Payment Mode: UDHARI (Credit)
1. Add items and click **Udhari**.
2. With no customer selected, verify error `"Select a customer for Udhari"` and Settle is disabled.
3. Search and select `Ganesh Joshi` (Credit Disabled).
   - Verify error `"This customer is not allowed credit"` and Settle stays blocked.
4. Search and select `Sunita Shinde` (Limit `₹5,000`). Add items totaling > `₹5,000`.
   - Verify error `"Credit limit exceeded. Available credit: Rs 5000.00"` and the **Manager Override** toggle appears.
   - Toggle **Manager Override** ON, enter a reason (e.g., `"Approved by Owner"`), and verify Settle unlocks.
5. Test **Quick Add** customer from `_CustomerPicker`, enter partial credit (e.g., `₹200` on Udhari, remainder on Cash), settle, and verify the receipt displays **Previous Outstanding**, **Credit Posted**, and **New Outstanding**.

## 7. Receipt Preview, Paper Sizes, Duplicate Watermark, Parcel Token & KOT
1. In the open receipt preview modal, click `58mm`, `80mm`, and `A4` — verify layout resizes cleanly.
2. On a settled **Parcel** bill, switch to the **Parcel Token Slip** tab and click **Print Token Slip**.
3. Click **Print Again**, **Download PDF**, **Share > WhatsApp**, and **New Order**.
4. In **Bills Settled Today**, click **Reprint** on any bill — verify the receipt shows the **`DUPLICATE`** watermark and `*** DUPLICATE COPY ***` banner.
5. Go to `/Orders/New`, send a KOT to kitchen — verify KOT auto-print preview opens when `Kitchen.AutoPrintKot` is enabled.
6. Go to `/Kitchen` and click the printer icon on any KOT card — verify KOT reprint preview opens inside the iframe modal.
7. Switch UI language to **मराठी** and verify Billing labels, validation messages, and Marathi dish names on receipts render cleanly.
