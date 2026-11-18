# RestaurantMS — Master Validation Matrix

Complete screen-by-screen audit of every form, action, validation rule, bilingual message (English & Marathi), enforcement layer, and automated test coverage.

> **Policy**: Zero generic `"Invalid input"` messages anywhere. Every rule gives a specific, actionable message, highlights the invalid field, focuses the first error, and preserves all entered data on failure.

---

## 1. Authentication, Users, Roles & Settings

| Screen / Action | Field | Rule | Message (EN) | Message (MR) | Layer |
|---|---|---|---|---|---|
| Login (`Account/Login`) | `UserId`, `Password` | Required, active account, lockout after failed attempts | User ID and password are required / Account locked | युजर आयडी आणि पासवर्ड आवश्यक आहे | Client + `LoginCommandValidator` + Handler |
| Change Password | `CurrentPassword`, `NewPassword`, `ConfirmPassword` | Required, min 8 chars, upper+lower+digit+special, confirm match | Password must be at least 8 characters with uppercase, lowercase, number and symbol | पासवर्ड किमान ८ अक्षरांचा (मोठे/लहान अक्षर, अंक व चिन्ह) असावा | Client + `ChangePasswordCommandValidator` |
| Users Create/Edit | `UserId`, `Email`, `Phone`, `RoleId` | Required, unique UserId, valid email, 10-digit mobile (`^[6-9]\d{9}$`), valid role | Enter a valid 10-digit mobile number starting with 6, 7, 8, or 9 | ६, ७, ८ किंवा ९ ने सुरू होणारा वैध १०-अंकी मोबाईल क्रमांक प्रविष्ट करा | Client + `CreateUserCommandValidator` / `UpdateUserCommandValidator` + Domain |
| Roles Create/Edit | `Name`, `Permissions` | Required, unique role name, cannot delete system role or role in use | Role name is required and must be unique | रोलचे नाव आवश्यक आणि अद्वितीय असावे | Client + `CreateRoleCommandValidator` / `UpdateRoleCommandValidator` |
| Hotel Settings (`Settings/Index`) | `HotelNameEn`, `HotelNameMr` | Required, max 150 chars | Hotel name (English/Marathi) is required | हॉटेलचे नाव (इंग्रजी/मराठी) आवश्यक आहे | Client + `SaveSettingsCommandValidator` |
| Hotel Settings (`Settings/Index`) | `Gstin`, `Fssai`, `ContactNumber`, `Email`, `HotelUpiId` | 15-char GSTIN Mod-36 checksum, 14-digit FSSAI, 10-digit mobile, email, UPI VPA | Enter a valid 15-character GSTIN / 14-digit FSSAI / UPI ID (`hotelname@okhdfc`) | वैध १५-अक्षरी जीएसटीआयएन / १४-अंकी FSSAI / वैध यूपीआय आयडी प्रविष्ट करा | Client + `SaveSettingsCommandValidator` + `IndianValidationRules` |
| Hotel Logo / Image Upload | `LogoFile` / dish image | Allowed signature (JPG/PNG/WEBP magic bytes), max 2 MB, GUID filename | Upload a valid JPG, PNG or WEBP image up to 2 MB | कमाल २ MB ची वैध JPG, PNG किंवा WEBP प्रतिमा अपलोड करा | Client (`appForm.js`) + `LocalFileStorage` |

---

## 2. Masters Hub (`/Masters`)

| Screen / Action | Field | Rule | Message (EN) | Message (MR) | Layer |
|---|---|---|---|---|---|
| Menu Item Form | `ShortCode` | Required, 2–16 alnum/`-`/`_`, unique | Short code is required (e.g. M01) / Code already exists | शॉर्ट कोड आवश्यक आहे / कोड आधीच अस्तित्वात आहे | Client + `MenuItemValidationRequestValidator` + `OperationalValidationService` |
| Menu Item Form | `NameEn`, `NameMr`, `CategoryId` | Required, 2–120 chars, valid category | Dish name (English/Marathi) and category are required | पदार्थाचे नाव व श्रेणी निवडणे आवश्यक आहे | Client + `MenuItemValidationRequestValidator` |
| Menu Item Form | `Price`, `GstPercent`, `PrepTimeMinutes` | `0 < Price <= 100000` (max 2 decimals), `0 <= GST <= 28`, `1 <= Prep <= 240` | Price must be greater than Rs 0.00 (max 2 decimal places) | किंमत रु. ० पेक्षा जास्त व कमाल २ दशांश असावी | Client + `MenuItemValidationRequestValidator` |
| Customer Master | `FullName`, `Mobile`, `Address` | Required name, 10-digit mobile, unique mobile, address mandatory when credit enabled | Enter a valid 10-digit mobile / Address required when Udhari enabled | वैध १०-अंकी मोबाईल प्रविष्ट करा / उधारीसाठी पत्ता आवश्यक आहे | Client + `CustomerValidationRequestValidator` + `OperationalValidationService` |
| Customer Master | `Email`, `Gstin`, `Pan`, `CreditLimit` | Valid email/GSTIN/PAN, `0 <= CreditLimit <= 10,00,000` (max 2 decimals) | Enter a valid 10-character PAN (e.g. ABCDE1234F) / GSTIN checksum | वैध १०-अक्षरी पॅन / १५-अक्षरी जीएसटीआयएन प्रविष्ट करा | Client + `CustomerValidationRequestValidator` |
| Supplier Master | `Name`, `ContactPerson`, `Mobile`, `Gstin`, `Pan`, `UpiId` | Required name/contact/mobile, unique name/mobile, valid GSTIN/PAN/UPI | Supplier name, contact and valid 10-digit mobile are required | पुरवठादाराचे नाव, संपर्क व वैध मोबाईल आवश्यक आहे | Client + `SupplierValidationRequestValidator` + `OperationalValidationService` |
| Delete Master Record (Menu/Customer/Table/Supplier) | `Id` | Block hard delete when referenced in bills | `Cannot delete, used in {n} bills. Deactivate instead` | `हटवता येत नाही, {0} बिलांमध्ये वापरलेले आहे. त्याऐवजी निष्क्रिय करा` | `OperationalValidationService.CheckCanDelete` |
| Concurrent Edit (`RowVersion`) | `RowVersion` | Optimistic concurrency token match | This record was updated by another user while you were editing. Reload the latest data and apply your changes again. | आपण बदल करत असताना इतर वापरकर्त्याने ही नोंद अद्यतनित केली आहे. कृपया ताजी माहिती रीलोड करून पुन्हा प्रयत्न करा. | Domain / EF + `OperationalValidationService.CheckConcurrency` |

---

## 3. Operations — Tables, Orders, Reservations, Billing & Day Closing

| Screen / Action | Field | Rule | Message (EN) | Message (MR) | Layer |
|---|---|---|---|---|---|
| Seat / Open Table | `TableCode` | Cannot seat on an already occupied table | `Table {T} is already occupied. Open the running order or select a free table.` | `टेबल {0} आधीच व्यापलेले आहे. चालू ऑर्डर उघडा किंवा रिकामे टेबल निवडा.` | `OperationalValidationService.CheckTableCanOccupy` |
| Move Table | `SourceTable`, `TargetTable`, `ActiveKotCount` | Target != Source, target must be free, explicit KOT transfer confirmation if running KOTs exist | `Table {S} has {n} active KOT(s) in kitchen. Confirm KOT transfer to {T} before moving.` | `टेबल {0} वर {1} सक्रिय केओटी किचनमध्ये चालू आहेत. टेबल {2} वर हलवण्यापूर्वी केओटी हस्तांतरणाची खात्री करा.` | `TableMoveValidationRequestValidator` + `OperationalValidationService` |
| Table Reservation | `GuestName`, `Mobile`, `GuestCount`, `ReservationDate` | Required name, 10-digit mobile, 1–50 guests, date >= today | Reservation date cannot be in the past / Party size must be between 1 and 50 | आरक्षणाचा दिनांक मागील असू शकत नाही / पाहुण्यांची संख्या १ ते ५० असावी | Client + `ReservationValidationRequestValidator` |
| Billing — Empty Cart | `Items` | At least 1 item & `GrandTotal > 0` | Add at least one item to settle the bill | बिल सेटल करण्यासाठी किमान एक पदार्थ जोडा | Client (`billing.js`) + `SettleBillRequestValidator` + `BillingService` |
| Billing — Cash | `CashReceived` | `CashReceived >= GrandTotal` | `Received amount is less than the bill total` | `मिळालेली रक्कम बिलाच्या एकूण रकमेपेक्षा कमी आहे` | Client (`billing.js`) + `BillingService` |
| Billing — UPI | `UpiReference`, `UpiConfirmed` | UTR required if `Payments.UpiReferenceRequired` ON; confirmation required | `UPI reference number is required` | `यूपीआय संदर्भ क्रमांक (UTR) आवश्यक आहे` | Client (`billing.js`) + `BillingService` |
| Billing — Card | `CardLast4`, `CardApprovalCode` | Exactly 4 digits (`^\d{4}$`) & approval code if `Payments.CardDetailsRequired` ON | `Enter the last 4 digits of the card` | `कार्डचे शेवटचे ४ अंक प्रविष्ट करा` | Client (`billing.js`) + `BillingService` |
| Billing — Split | `Payments` sum | `Sum(Payments) == GrandTotal` | `Total of payments must equal Rs {0}. Remaining: Rs {1}` | `एकूण पेमेंट रु. {0} इतके असणे आवश्यक आहे. बाकी: रु. {1}` | Client (`billing.js`) + `BillingService` |
| Billing — Udhari | `CustomerId`, `CreditAllowed`, `CreditLimit`, `ManagerOverride` | Customer required, credit allowed, within available credit unless manager override + reason | `Select a customer for Udhari` / `This customer is not allowed credit` / `Credit limit exceeded. Available credit: Rs {0}` | `उधारीसाठी ग्राहक निवडा` / `या ग्राहकाला उधारीची परवानगी नाही` / `उधारी मर्यादा ओलांडली आहे. उपलब्ध उधारी: रु. {0}` | Client (`billing.js`) + `BillingService` |
| Billing — Double Submit | `IdempotencyKey` | Unique key per bill; replay returns original bill with 0 duplicate vouchers | Prevented silently via idempotency + disabled submit button | दुहेरी नोंद रोखली जाते | Client (`billing.js` / `appForm.js`) + `BillingService` |
| Billing — Session Timeout | Cart state | Persist cart to `localStorage`, catch `401` / login redirect, restore cart after re-login | `Your POS cart has been safely saved. Sign in again to resume billing without losing items.` | `तुमची पीओएस कार्ट सुरक्षित ठेवली आहे. ऑर्डर न गमावता बिलिंग पुढे सुरू ठेवण्यासाठी पुन्हा लॉगिन करा.` | Client (`billing.js`) |
| Day Closing | `ClosingDate`, `CountedCash`, `VarianceRemarks` | Date not future, amounts >= 0, remarks mandatory if `|Counted - Expected| >= 1` | Cash variance detected. Enter remarks explaining the shortage or excess before closing. | रोख रकमेत तफावत आढळली आहे. क्लोजिंगपूर्वी कारण नोंदवा. | Client + `DayClosingValidationRequestValidator` |

---

## 4. Inventory Module (`/Inventory`, `Inventory.Enabled` Toggle)

| Screen / Action | Field | Rule | Message (EN) | Message (MR) | Layer |
|---|---|---|---|---|---|
| All Inventory Actions | `FeatureKeys.Inventory` | Must be enabled in Feature Toggles | Inventory module is disabled in Feature Toggles | इन्व्हेंटरी मॉड्यूल फीचर टॉगलमध्ये बंद आहे | UI (`NavMenu`) + `FeatureEnabledAttribute` + `InventoryService` |
| Purchase Entry | `SupplierId`, `InvoiceNumber`, `InvoiceDate` | Supplier required; unique `(SupplierId, InvoiceNumber)`; date <= today | Supplier is required / `Invoice '{no}' has already been recorded for {sup}` / Invoice date cannot be in the future | पुरवठादार निवडा / बिल क्रमांक आधीच नोंदवला आहे / भविष्यातील दिनांक चालणार नाही | Client + `RecordPurchaseRequestValidator` + `InventoryService` |
| Purchase Entry Lines | `Quantity`, `Rate`, `TaxPercent` | `Qty > 0`, `Rate > 0`, `0 <= Tax <= 28` | Quantity and rate must be greater than zero | प्रमाण आणि दर शून्यापेक्षा जास्त असावेत | Client + `RecordPurchaseRequestValidator` + `InventoryService` |
| Purchase Attachment | `AttachmentFile` | Valid JPG/PNG/WEBP/PDF magic bytes, max 5 MB | File content signature does not match allowed invoice types (JPG, PNG, WEBP, PDF) | वैध JPG, PNG, WEBP किंवा PDF (कमाल ५ MB) अपलोड करा | Client + `LocalFileStorage.SaveDocumentAsync` |
| Stock Adjustment / Consumption / Wastage | `Quantity` / `PhysicalCount` | Negative stock blocked unless `Inventory.AllowNegativeStock == true`; date <= today | Insufficient stock for {item}. Available: {qty} | साठा अपुरा आहे; ऋण साठ्याला परवानगी नाही | Client + Validators + `InventoryService` |
| Wastage Entry | `Reason` | Mandatory non-empty reason (max 300 chars) | Wastage reason is required | नुकसानीचे कारण लिहिणे आवश्यक आहे | Client + `StockWastageRequestValidator` + `InventoryService` |

---

## 5. Staff, Attendance, Employee Advances, Accounts & Reports

| Screen / Action | Field | Rule | Message (EN) | Message (MR) | Layer |
|---|---|---|---|---|---|
| Staff Profile | `EmployeeCode`, `FullName`, `Mobile`, `JoiningDate`, `BasicSalary`, `Pan`, `UpiId` | Unique code/mobile, valid mobile/PAN/UPI, joining date <= today, `Salary > 0` | Joining date cannot be in the future / Basic salary must be greater than Rs 0 | रुजू दिनांक भविष्यातील असू शकत नाही / मूळ पगार ० पेक्षा जास्त असावा | Client + `StaffProfileValidationRequestValidator` + `OperationalValidationService` |
| Attendance Sheet | `Year`, `Month` | Cannot modify attendance once month is locked for payroll | `Attendance for {Month Year} is locked after payroll finalization and cannot be edited.` | `{0} महिन्याचा पगार अंतिम झाल्यामुळे हजेरी लॉक केली आहे; बदल करता येणार नाहीत.` | `OperationalValidationService.CheckAttendanceMonthUnlocked` + `StaffController` |
| Employee Advance | `StaffId`, `Amount`, `Date`, `ManagerOverride` | Active staff only, `Amount > 0`, date not in locked accounting period, `<= MaxAdvanceSalaryPercent` unless manager override + reason | Advance exceeds allowed monthly salary cap. Provide manager override reason. | अ‍ॅडव्हान्स मर्यादेपेक्षा जास्त आहे; मॅनेजर ओव्हरराइडचे कारण आवश्यक आहे. | Client + `GiveEmployeeAdvanceRequestValidator` + `SampleEmployeeAdvancesUiData` |
| Advance Recovery | `Amount` | `0 < Amount <= Outstanding`, date not in locked period | Recovery amount cannot exceed outstanding advance balance | वसुली रक्कम थकबाकी अ‍ॅडव्हान्सपेक्षा जास्त असू शकत नाही | Client + `RecordAdvanceRecoveryRequestValidator` |
| Accounts Voucher | `VoucherDate`, `Lines` | Date not in locked accounting period, >= 2 lines, `Sum(Debit) == Sum(Credit)` | Total Debit must equal Total Credit / Transactions on locked period are blocked | एकूण डेबिट व क्रेडिट समान असावेत / लॉक कालावधीत व्यवहार बंद आहेत | Client + `PostVoucherRequestValidator` + `IAccountingService` |
| Reports Filter | `DatePreset`, `FromDate`, `ToDate`, `PageSize` | Auto-swap inverted custom dates, clamp page size 5..100, permission check per report category | Friendly empty state: `No data for the selected filters` + Error retry + Offline toast | निवडलेल्या फिल्टरसाठी कोणतीही माहिती उपलब्ध नाही | `ReportFilterModel` + `ReportsService` + `reports.js` |

---

## 6. Global UX & Resilience Guarantees (`appForm.js`, `billing.js`)
- **Inline Blur & Submit Validation**: Specific field-labeled messages (never `"Invalid input"`), inline beside every field and summarized at the top of the form.
- **Server Error Placement & Focus**: Server-returned ModelState / JSON `errors` map directly onto matching field inputs, focus + scroll to the first invalid control, and **never clear or lose entered data**.
- **Double-Submit Protection**: Submit buttons lock immediately on valid submission; billing uses server-side `IdempotencyKey`.
- **Offline / Network Error Handling**: Global `offline` / `online` browser events and fetch catch blocks show clear SweetAlert2 toasts while keeping form data intact.
- **Session Timeout During POS Billing**: Cart auto-saves to `localStorage` and restores cleanly after re-login.
