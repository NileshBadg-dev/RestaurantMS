# UI Kit & Design System Guide (Phase 2)

## 1. Overview & Colour Tokens

The application follows the custom colourful visual identity matching the approved specification and UI screens:

| Token | Hex | Role | Usage |
|---|---|---|---|
| `--color-ink` | `#1B1033` | Deep ink text & high-contrast elements | Primary headings, table text, dark accents |
| `--color-plum` | `#3D1F6D` | Dark Plum primary brand | Sidebar, primary buttons (`btn-plum`), active nav items |
| `--color-saffron` | `#FF9F1C` | Warm saffron secondary | Badges, warnings, parcel tags, highlight chips |
| `--color-chili` | `#C62839` | Chili danger | Late KOT badges, unpaid status, errors, critical alerts |
| `--color-teal` | `#0B7A70` | Teal success | Paid status, positive KPIs, bill settlement |
| `--color-cream` | `#FFF8EC` | Warm cream background | Main content area, subtle panel highlights |
| `--color-card` | `#FFFFFF` | Card white | Component surfaces, modal dialogs, data table backgrounds |
| `--color-muted` | `#6B5F80` | Muted secondary text | Subtitles, timestamps, breadcrumbs |

Typography:
- Primary Sans: `Inter`
- Devanagari (Marathi): `Noto Sans Devanagari` (hosted locally with local font fallback)
- Numerical / Monospace: Monospace with tabular numerals (`font-monospace`)

---

## 2. Shared Components & Tag Helpers

All screens are constructed exclusively with shared components:

1. **`_AppDataGrid.cshtml`**: Standardized data grid wrapper with responsive tables, empty states, and pagination controls.
2. **`_PageHeader.cshtml`**: Standardized page title, subtitle, and breadcrumb header.
3. **`_FormLayout.cshtml`**: Two-column responsive form layout with fixed standard action buttons (Save / Save & New / Cancel).
4. **`_ReportFilterBar.cshtml`**: Standard date-range and dimension selector (shift, waiter, table, category, payment mode).
5. **`_StatusChip.cshtml` & `<status-chip status="..." />`**: Visual status indicator badge with colour mapping and icons.
6. **`_KpiCard.cshtml`**: Metric card supporting title, formatted value, subtitle, custom theme (teal, plum, saffron, chili, ink), and icon.
7. **`_EmptyState.cshtml`**: Standard zero-data indicator with icon, friendly message, and call-to-action button.
8. **`_BillingPanel.cshtml`**: Unified settlement panel (items grid, order-type tabs, payment mode chips: Cash, UPI, Card, Udhari, and customer credit block).
9. **`_Chart.cshtml`**: Chart.js wrapper supporting Bar, Stacked Bar, and Line charts.
10. **Tag Helpers**:
   - `<money-input asp-for="..." />` (₹ prefix and 2-decimal formatting)
   - `<quantity-input asp-for="..." />` (stepper with `+` and `-` buttons)
   - `<mobile-input asp-for="..." />` (+91 prefix with 10-digit validation mask)
   - `<date-range-picker asp-for-start="..." asp-for-end="..." />`
   - `<permission name="...">` (permission-based element visibility)

---

## 3. UI Data Layer Architecture (Transition to Phase 3)

In Phase 2, every screen consumes mock operations via dedicated UI data interfaces located in `src/RestaurantMS.Application/Common/Interfaces/UiData/`:
- `IDashboardUiData`
- `ITablesUiData`
- `IParcelUiData`
- `IOrdersUiData`
- `IKitchenUiData`
- `IBillingUiData`
- `ICustomersUiData`
- `IAccountsUiData`
- `IReportsUiData`
- `IMenuUiData`
- `IInventoryUiData`
- `IStaffUiData`
- `IReservationsUiData`
- `IDayClosingUiData`

These interfaces are implemented in `src/RestaurantMS.Web/SampleData/` and registered with:
```csharp
builder.Services.AddSampleUiData(useSampleData: true);
```

### Transition to Real MediatR Queries in Phase 3
When implementing Phase 3 (business logic and database entities):
1. Create real query handlers returning the same DTOs.
2. Provide a production implementation of each `I*UiData` interface in `Infrastructure` or `Application` that dispatches MediatR queries/commands.
3. Toggle `builder.Services.AddSampleUiData(useSampleData: false)` to switch the entire application to database queries without changing any Razor views or ViewModels.

---

## 4. Keyboard Shortcuts

Standard POS keyboard shortcuts implemented:
- `F2`: New Dine-in Order (`/Orders/New`)
- `F4`: New Parcel (`/Parcel/Create`)
- `F5`: Send KOT / Refresh Kitchen Display
- `F9`: Quick Bill
- `F10`: Settle & Print Bill
