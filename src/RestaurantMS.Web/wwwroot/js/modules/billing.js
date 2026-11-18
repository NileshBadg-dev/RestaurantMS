(function () {
    'use strict';

    var root = document.getElementById('billingPanelRoot');
    if (!root) return;

    function parseJsonScript(id, fallback) {
        var el = document.getElementById(id);
        if (!el) return fallback;
        try {
            return JSON.parse(el.textContent || '');
        } catch (e) {
            return fallback;
        }
    }

    var config = parseJsonScript('billingConfigJson', {});
    var msgs = config.messages || {};
    var menuCatalog = parseJsonScript('billingMenuCatalogJson', []);
    var customersCache = parseJsonScript('billingCustomersJson', []);

    var state = {
        idempotencyKey: generateKey(),
        orderType: config.orderType || 'Dine-in',
        tableCode: config.tableCode || 'T1',
        items: parseJsonScript('billingInitialItemsJson', []),
        discount: 0,
        packingCharge: 0,
        gstPercent: Number(config.gstPercent || 5),
        totals: { subtotal: 0, discount: 0, packing: 0, taxable: 0, cgst: 0, sgst: 0, totalTax: 0, rawTotal: 0, roundOff: 0, grandTotal: 0 },
        paymentMode: 'Cash',
        cashReceived: 0,
        cashTouched: false,
        upiRef: '',
        upiConfirmed: false,
        cardLast4: '',
        cardApproval: '',
        cardBank: '',
        splitRows: [],
        udhariCreditAmount: 0,
        udhariTouched: false,
        udhariBalanceMode: 'Cash',
        udhariNote: '',
        managerOverride: false,
        managerOverrideReason: '',
        selectedCustomer: null,
        settling: false
    };

    function generateKey() {
        if (window.crypto && typeof window.crypto.randomUUID === 'function') {
            return window.crypto.randomUUID();
        }
        return 'bill-' + Date.now() + '-' + Math.random().toString(36).slice(2, 10);
    }

    function getAntiForgeryToken() {
        var input = root.querySelector('input[name="__RequestVerificationToken"]') || document.querySelector('input[name="__RequestVerificationToken"]');
        return input ? input.value : '';
    }

    function round2(n) {
        return Math.round((Number(n) || 0) * 100) / 100;
    }

    function round0AwayFromZero(n) {
        var v = Number(n) || 0;
        return v >= 0 ? Math.floor(v + 0.5) : Math.ceil(v - 0.5);
    }

    function fmtRs(n) {
        return '₹' + (Number(n) || 0).toFixed(2);
    }

    function fmtPlain(n) {
        return (Number(n) || 0).toFixed(2);
    }

    function formatTemplate(template, args) {
        var out = template || '';
        for (var i = 0; i < args.length; i++) {
            out = out.replace('{' + i + '}', args[i]);
        }
        return out;
    }

    function el(id) {
        return document.getElementById(id);
    }

    // Mirror exact server-side math from BillingService.CalculateTotals
    function recalculateTotals() {
        var subtotal = 0;
        state.items.forEach(function (item) {
            if (item.quantity > 0 && item.rate >= 0) {
                subtotal = round2(subtotal + round2(item.quantity * item.rate));
            }
        });

        var discount = Math.min(subtotal, Math.max(0, round2(state.discount)));
        var packing = Math.max(0, round2(state.packingCharge));
        var taxable = Math.max(0, round2(subtotal - discount + packing));
        var gstPct = Math.min(28, Math.max(0, Number(state.gstPercent) || 0));
        var totalTax = round2(taxable * (gstPct / 100));
        var cgst = round2(totalTax / 2);
        var sgst = round2(totalTax - cgst);
        var rawTotal = round2(taxable + totalTax);
        var grandTotal = Math.max(0, round0AwayFromZero(rawTotal));
        var roundOff = round2(grandTotal - rawTotal);

        state.totals = {
            subtotal: subtotal,
            discount: discount,
            packing: packing,
            taxable: taxable,
            gstPercent: gstPct,
            cgst: cgst,
            sgst: sgst,
            totalTax: totalTax,
            rawTotal: rawTotal,
            roundOff: roundOff,
            grandTotal: grandTotal
        };

        if (!state.cashTouched) {
            state.cashReceived = grandTotal;
            var cashInput = el('cashReceivedInput');
            if (cashInput) cashInput.value = grandTotal.toFixed(0);
        }

        if (!state.udhariTouched) {
            state.udhariCreditAmount = grandTotal;
            var udhariInput = el('udhariCreditAmountInput');
            if (udhariInput) udhariInput.value = grandTotal.toFixed(0);
        }

        renderTotalsSummary();
        updateModeDisplays();
        validateAndUpdateCta();
    }

    function renderTotalsSummary() {
        var t = state.totals;
        if (el('summarySubtotal')) el('summarySubtotal').textContent = fmtRs(t.subtotal);

        var discRow = el('summaryDiscountRow');
        if (discRow) {
            discRow.classList.toggle('d-none', t.discount <= 0);
            if (el('summaryDiscount')) el('summaryDiscount').textContent = '- ' + fmtRs(t.discount);
        }

        var packRow = el('summaryPackingRow');
        if (packRow) {
            packRow.classList.toggle('d-none', t.packing <= 0);
            if (el('summaryPacking')) el('summaryPacking').textContent = fmtRs(t.packing);
        }

        if (el('summaryTaxLabel')) {
            var half = (t.gstPercent / 2).toFixed(1).replace(/\.0$/, '');
            el('summaryTaxLabel').textContent = 'GST ' + t.gstPercent + '% (CGST ' + half + '% + SGST ' + half + '%)';
        }
        if (el('summaryTax')) el('summaryTax').textContent = fmtRs(t.totalTax);

        var roundRow = el('summaryRoundOffRow');
        if (roundRow) {
            roundRow.classList.toggle('d-none', Math.abs(t.roundOff) < 0.005);
            if (el('summaryRoundOff')) {
                el('summaryRoundOff').textContent = (t.roundOff >= 0 ? '+ ' : '- ') + fmtRs(Math.abs(t.roundOff));
            }
        }

        if (el('summaryGrandTotal')) el('summaryGrandTotal').textContent = fmtRs(t.grandTotal);
    }

    function renderItemsTable() {
        var tbody = el('billingItemsBody');
        if (!tbody) return;

        if (state.items.length === 0) {
            tbody.innerHTML = '<tr><td colspan="5" class="text-center text-muted py-4 small">' +
                '<i class="bi bi-basket2 d-block fs-4 mb-1"></i>No items added yet. Enter a short code (e.g. M01) or click Browse Menu.' +
                '</td></tr>';
            recalculateTotals();
            return;
        }

        var html = '';
        state.items.forEach(function (item, index) {
            var amt = round2(item.quantity * item.rate);
            html += '<tr data-item-idx="' + index + '">' +
                '<td><div class="fw-semibold text-ink small">' + escapeHtml(item.itemName) + '</div>' +
                (item.marathiName ? '<div class="text-muted" style="font-size:0.75rem;">' + escapeHtml(item.marathiName) + '</div>' : '') +
                '</td>' +
                '<td class="text-center">' +
                '<div class="btn-group btn-group-sm">' +
                '<button type="button" class="btn btn-outline-secondary py-0 px-2" data-qty-delta="-1" data-idx="' + index + '">-</button>' +
                '<span class="btn btn-light border py-0 px-2 font-monospace fw-bold">' + item.quantity + '</span>' +
                '<button type="button" class="btn btn-outline-secondary py-0 px-2" data-qty-delta="1" data-idx="' + index + '">+</button>' +
                '</div>' +
                '</td>' +
                '<td class="text-end font-monospace small">' + fmtRs(item.rate) + '</td>' +
                '<td class="text-end font-monospace fw-semibold small">' + fmtRs(amt) + '</td>' +
                '<td class="text-center"><button type="button" class="btn btn-sm text-danger p-0" data-remove-item="' + index + '" title="Remove"><i class="bi bi-trash"></i></button></td>' +
                '</tr>';
        });

        tbody.innerHTML = html;
        persistCartSnapshot();
        recalculateTotals();
    }

    function addMenuItem(dish) {
        if (!dish) return;
        var existing = state.items.find(function (i) {
            return i.itemName.toLowerCase() === dish.name.toLowerCase() && Number(i.rate) === Number(dish.price);
        });
        if (existing) {
            existing.quantity += 1;
        } else {
            state.items.push({
                itemName: dish.name,
                marathiName: dish.marathiName || null,
                quantity: 1,
                rate: Number(dish.price) || 0,
                notes: null
            });
        }
        renderItemsTable();
    }

    function renderCatalogGrid(filterText) {
        var grid = el('billingCatalogGrid');
        if (!grid) return;
        var term = (filterText || '').trim().toLowerCase();
        var filtered = menuCatalog.filter(function (m) {
            if (!term) return true;
            return (m.name || '').toLowerCase().indexOf(term) >= 0 ||
                (m.shortCode || '').toLowerCase().indexOf(term) >= 0 ||
                (m.category || '').toLowerCase().indexOf(term) >= 0;
        });

        if (filtered.length === 0) {
            grid.innerHTML = '<div class="col-12 text-center text-muted py-4 small">No matching menu items found.</div>';
            return;
        }

        grid.innerHTML = filtered.map(function (m) {
            return '<div class="col-6 col-md-4">' +
                '<button type="button" class="btn btn-light border w-100 text-start p-2 d-flex flex-column align-items-start gap-1" data-catalog-add="' + m.id + '">' +
                '<div class="d-flex justify-content-between w-100 align-items-center">' +
                '<span class="badge bg-plum-subtle text-plum font-monospace">' + escapeHtml(m.shortCode) + '</span>' +
                '<span class="badge ' + (m.isVeg ? 'bg-teal-subtle text-teal' : 'bg-chili-subtle text-chili') + '">' + (m.isVeg ? 'Veg' : 'Non-Veg') + '</span>' +
                '</div>' +
                '<div class="fw-semibold small text-ink text-truncate w-100">' + escapeHtml(m.name) + '</div>' +
                '<div class="fw-bold text-plum font-monospace small">' + fmtRs(m.price) + '</div>' +
                '</button></div>';
        }).join('');
    }

    function switchPaymentMode(mode) {
        state.paymentMode = mode;

        document.querySelectorAll('[data-mode-panel]').forEach(function (panel) {
            panel.classList.toggle('d-none', panel.getAttribute('data-mode-panel') !== mode);
        });

        if (mode === 'Cash') {
            if (!state.cashTouched) {
                state.cashReceived = state.totals.grandTotal;
                if (el('cashReceivedInput')) el('cashReceivedInput').value = state.totals.grandTotal.toFixed(0);
            }
        } else if (mode === 'UPI') {
            updateUpiQr();
        } else if (mode === 'Split') {
            if (state.splitRows.length === 0) {
                var half = Math.floor(state.totals.grandTotal / 2);
                var rest = round2(state.totals.grandTotal - half);
                state.splitRows = [
                    { mode: 'Cash', amount: half, ref: '' },
                    { mode: 'UPI', amount: rest, ref: '' }
                ];
            }
            renderSplitRows();
        } else if (mode === 'Udhari') {
            if (!state.udhariTouched) {
                state.udhariCreditAmount = state.totals.grandTotal;
                if (el('udhariCreditAmountInput')) el('udhariCreditAmountInput').value = state.totals.grandTotal.toFixed(0);
            }
            var searchInput = el('customerSearchInput');
            if (!state.selectedCustomer && searchInput) {
                searchInput.focus();
            }
        }

        updateModeDisplays();
        validateAndUpdateCta();
    }

    function updateUpiQr() {
        var img = el('upiQrImg');
        var amtEl = el('upiAmountDisplay');
        var amt = state.totals.grandTotal;
        if (amtEl) amtEl.textContent = fmtRs(amt);
        if (img && config.upiQrUrl) {
            img.src = config.upiQrUrl + '?amount=' + encodeURIComponent(fmtPlain(amt)) + '&note=' + encodeURIComponent('Table ' + (state.tableCode || 'POS'));
        }
    }

    function updateModeDisplays() {
        var total = state.totals.grandTotal;

        // Cash status
        var cashDiff = round2((Number(state.cashReceived) || 0) - total);
        var cashLabel = el('cashStatusLabel');
        var cashAmt = el('cashStatusAmount');
        if (cashLabel && cashAmt) {
            if (cashDiff < 0) {
                cashLabel.textContent = msgs.balanceDue || 'Balance due';
                cashAmt.textContent = fmtRs(Math.abs(cashDiff));
                cashAmt.classList.remove('text-teal');
                cashAmt.classList.add('text-chili');
            } else {
                cashLabel.textContent = msgs.changeToReturn || 'Change to return';
                cashAmt.textContent = fmtRs(cashDiff);
                cashAmt.classList.remove('text-chili');
                cashAmt.classList.add('text-teal');
            }
        }

        // UPI
        if (state.paymentMode === 'UPI') {
            updateUpiQr();
        }
        var upiBtn = el('btnUpiPaymentReceived');
        if (upiBtn) {
            upiBtn.classList.toggle('btn-teal', state.upiConfirmed);
            upiBtn.classList.toggle('btn-outline-teal', !state.upiConfirmed);
            upiBtn.innerHTML = state.upiConfirmed
                ? '<i class="bi bi-check-circle-fill"></i> Payment Received Confirmed'
                : '<i class="bi bi-check2-circle"></i> Payment Received';
        }

        // Split totals
        if (state.paymentMode === 'Split') {
            var paid = round2(state.splitRows.reduce(function (acc, r) { return acc + (Number(r.amount) || 0); }, 0));
            var rem = round2(total - paid);
            if (el('splitPaidTotal')) el('splitPaidTotal').textContent = fmtRs(paid);
            if (el('splitRemainingTotal')) {
                el('splitRemainingTotal').textContent = fmtRs(rem);
                el('splitRemainingTotal').classList.toggle('text-teal', Math.abs(rem) < 0.01);
                el('splitRemainingTotal').classList.toggle('text-chili', Math.abs(rem) >= 0.01);
            }
        }

        // Udhari displays
        var creditAmt = Math.min(total, Math.max(0, round2(state.udhariCreditAmount)));
        var partPay = round2(total - creditAmt);
        var partWrap = el('udhariPartPaySummary');
        if (partWrap) {
            partWrap.classList.toggle('d-none', partPay <= 0);
            if (el('udhariPartPayAmount')) el('udhariPartPayAmount').textContent = fmtRs(partPay);
        }

        var overrideBox = el('udhariOverrideBox');
        if (overrideBox) {
            var overLimit = false;
            if (state.selectedCustomer && state.selectedCustomer.creditAllowed) {
                var avail = Math.max(0, round2(state.selectedCustomer.creditLimit - state.selectedCustomer.outstandingBalance));
                overLimit = creditAmt > avail;
            }
            overrideBox.classList.toggle('d-none', !overLimit || !config.canOverrideCreditLimit);
            var reasonInput = el('udhariOverrideReasonInput');
            if (reasonInput) {
                reasonInput.classList.toggle('d-none', !state.managerOverride);
            }
        }
    }

    function renderSplitRows() {
        var container = el('splitRowsContainer');
        if (!container) return;

        container.innerHTML = state.splitRows.map(function (row, idx) {
            var refPlaceholder = row.mode === 'UPI' ? 'UTR / Ref' : row.mode === 'Card' ? 'Last 4 digits' : 'Note (optional)';
            var showRef = row.mode === 'UPI' || row.mode === 'Card';
            return '<div class="border rounded p-2 bg-white" data-split-row="' + idx + '">' +
                '<div class="d-flex gap-2 align-items-center">' +
                '<select class="form-select form-select-sm" style="width: 110px;" data-split-mode="' + idx + '">' +
                '<option value="Cash"' + (row.mode === 'Cash' ? ' selected' : '') + '>Cash</option>' +
                '<option value="UPI"' + (row.mode === 'UPI' ? ' selected' : '') + '>UPI</option>' +
                '<option value="Card"' + (row.mode === 'Card' ? ' selected' : '') + '>Card</option>' +
                '<option value="Udhari"' + (row.mode === 'Udhari' ? ' selected' : '') + '>Udhari</option>' +
                '</select>' +
                '<div class="input-group input-group-sm flex-grow-1">' +
                '<span class="input-group-text">₹</span>' +
                '<input type="number" class="form-control font-monospace" min="0" step="1" value="' + (Number(row.amount) || 0) + '" data-split-amount="' + idx + '" />' +
                '</div>' +
                '<button type="button" class="btn btn-sm btn-outline-danger px-2" data-split-remove="' + idx + '" title="Remove row"><i class="bi bi-x-lg"></i></button>' +
                '</div>' +
                (showRef
                    ? '<div class="mt-2"><input type="text" class="form-control form-control-sm font-monospace" placeholder="' + refPlaceholder + '" value="' + escapeHtml(row.ref || '') + '" data-split-ref="' + idx + '" /></div>'
                    : '') +
                '</div>';
        }).join('');

        updateModeDisplays();
        validateAndUpdateCta();
    }

    function buildPaymentsPayload() {
        var total = state.totals.grandTotal;
        if (state.paymentMode === 'Cash') {
            return [{
                mode: 'Cash',
                amount: total,
                cashReceived: Number(state.cashReceived) || 0
            }];
        }

        if (state.paymentMode === 'UPI') {
            return [{
                mode: 'UPI',
                amount: total,
                upiReference: state.upiRef.trim(),
                upiConfirmed: state.upiConfirmed
            }];
        }

        if (state.paymentMode === 'Card') {
            return [{
                mode: 'Card',
                amount: total,
                cardLast4: state.cardLast4.trim(),
                cardApprovalCode: state.cardApproval.trim(),
                cardBankName: state.cardBank.trim()
            }];
        }

        if (state.paymentMode === 'Split') {
            return state.splitRows.map(function (r) {
                var amt = round2(Number(r.amount) || 0);
                var obj = { mode: r.mode, amount: amt };
                if (r.mode === 'Cash') obj.cashReceived = amt;
                if (r.mode === 'UPI') {
                    obj.upiReference = (r.ref || '').trim();
                    obj.upiConfirmed = true;
                }
                if (r.mode === 'Card') {
                    obj.cardLast4 = (r.ref || '').trim();
                }
                return obj;
            });
        }

        if (state.paymentMode === 'Udhari') {
            var creditAmt = Math.min(total, Math.max(0, round2(state.udhariCreditAmount)));
            var rest = round2(total - creditAmt);
            var list = [];
            if (creditAmt > 0) {
                list.push({ mode: 'Udhari', amount: creditAmt });
            }
            if (rest > 0) {
                var restLine = { mode: state.udhariBalanceMode || 'Cash', amount: rest };
                if (restLine.mode === 'Cash') restLine.cashReceived = rest;
                if (restLine.mode === 'UPI') restLine.upiConfirmed = true;
                list.push(restLine);
            }
            return list;
        }

        return [];
    }

    function getValidationError() {
        var total = state.totals.grandTotal;
        if (state.items.length === 0 || total <= 0) {
            return msgs.emptyItems || 'Add at least one item to settle the bill';
        }

        if (state.paymentMode === 'Cash') {
            if ((Number(state.cashReceived) || 0) < total) {
                return msgs.cashUnderpaid || 'Received amount is less than the bill total';
            }
            return null;
        }

        if (state.paymentMode === 'UPI') {
            if (config.upiReferenceRequired && !state.upiRef.trim()) {
                return msgs.upiRefRequired || 'UPI reference number is required';
            }
            if (!state.upiConfirmed) {
                return msgs.upiConfirmRequired || 'Click "Payment Received" once UPI payment is confirmed';
            }
            return null;
        }

        if (state.paymentMode === 'Card') {
            var last4 = state.cardLast4.trim();
            if ((config.cardDetailsRequired || last4.length > 0) && !/^\d{4}$/.test(last4)) {
                return msgs.cardLast4Required || 'Enter the last 4 digits of the card';
            }
            if (config.cardDetailsRequired && !state.cardApproval.trim()) {
                return msgs.cardApprovalRequired || 'Card approval code is required';
            }
            return null;
        }

        if (state.paymentMode === 'Split') {
            if (state.splitRows.length === 0) {
                return formatTemplate(msgs.splitMismatch || 'Total of payments must equal Rs {0}. Remaining: Rs {1}', [fmtPlain(total), fmtPlain(total)]);
            }

            var paid = 0;
            var udhariSum = 0;
            for (var i = 0; i < state.splitRows.length; i++) {
                var r = state.splitRows[i];
                var amt = round2(Number(r.amount) || 0);
                paid = round2(paid + amt);

                if (r.mode === 'UPI' && config.upiReferenceRequired && !(r.ref || '').trim()) {
                    return msgs.upiRefRequired || 'UPI reference number is required';
                }
                if (r.mode === 'Card' && (config.cardDetailsRequired || (r.ref || '').trim().length > 0) && !/^\d{4}$/.test((r.ref || '').trim())) {
                    return msgs.cardLast4Required || 'Enter the last 4 digits of the card';
                }
                if (r.mode === 'Udhari') {
                    udhariSum = round2(udhariSum + amt);
                }
            }

            var rem = round2(total - paid);
            if (Math.abs(rem) >= 0.01) {
                return formatTemplate(msgs.splitMismatch || 'Total of payments must equal Rs {0}. Remaining: Rs {1}', [fmtPlain(total), fmtPlain(rem)]);
            }

            if (udhariSum > 0) {
                return validateUdhariCustomer(udhariSum);
            }
            return null;
        }

        if (state.paymentMode === 'Udhari') {
            var creditAmt = round2(Number(state.udhariCreditAmount) || 0);
            if (creditAmt <= 0 || creditAmt > total) {
                return formatTemplate(msgs.splitMismatch || 'Total of payments must equal Rs {0}. Remaining: Rs {1}', [fmtPlain(total), fmtPlain(round2(total - creditAmt))]);
            }
            return validateUdhariCustomer(creditAmt);
        }

        return null;
    }

    function validateUdhariCustomer(creditAmount) {
        if (!state.selectedCustomer) {
            return msgs.udhariCustomerRequired || 'Select a customer for Udhari';
        }
        if (!state.selectedCustomer.creditAllowed) {
            return msgs.creditDisabled || 'This customer is not allowed credit';
        }
        var avail = Math.max(0, round2(state.selectedCustomer.creditLimit - state.selectedCustomer.outstandingBalance));
        if (creditAmount > avail) {
            if (!state.managerOverride) {
                return formatTemplate(msgs.creditLimitExceeded || 'Credit limit exceeded. Available credit: Rs {0}', [fmtPlain(avail)]);
            }
            if (!state.managerOverrideReason.trim()) {
                return msgs.overrideReasonRequired || 'Enter a manager override reason to exceed credit limit';
            }
        }
        return null;
    }

    function validateAndUpdateCta() {
        var err = getValidationError();
        var alertEl = el('billingValidationAlert');
        var textEl = el('billingValidationText');
        var btn = el('btnSettleBill');

        if (err && state.items.length > 0) {
            if (alertEl) alertEl.classList.remove('d-none');
            if (textEl) textEl.textContent = err;
        } else {
            if (alertEl) alertEl.classList.add('d-none');
        }

        if (btn) {
            btn.disabled = !!err || state.settling;
        }
    }

    function renderCustomerSearchResults(list) {
        var box = el('customerSearchResults');
        if (!box) return;
        if (!list || list.length === 0) {
            box.innerHTML = '<div class="p-2 small text-muted text-center">No customer found. Use Quick Add above.</div>';
            box.classList.remove('d-none');
            return;
        }

        box.innerHTML = list.map(function (c) {
            var avail = Math.max(0, round2(c.creditLimit - c.outstandingBalance));
            var badge = c.creditAllowed
                ? '<span class="badge bg-teal-subtle text-teal">Avail: ' + fmtRs(avail) + '</span>'
                : '<span class="badge bg-chili text-white">No Credit</span>';
            return '<button type="button" class="w-100 text-start btn btn-sm btn-light border-0 border-bottom rounded-0 px-3 py-2 d-flex justify-content-between align-items-center" data-pick-customer="' + c.id + '">' +
                '<div><div class="fw-semibold text-ink">' + escapeHtml(c.name) + '</div>' +
                '<div class="small text-muted font-monospace">' + escapeHtml(c.mobile) + '</div></div>' +
                badge +
                '</button>';
        }).join('');
        box.classList.remove('d-none');
    }

    function selectCustomer(customer) {
        state.selectedCustomer = customer;
        var summary = el('customerSummaryBlock');
        var searchInput = el('customerSearchInput');
        var resultsBox = el('customerSearchResults');
        var hiddenId = el('selectedCustomerId');

        if (resultsBox) resultsBox.classList.add('d-none');

        if (!customer) {
            if (summary) summary.classList.add('d-none');
            if (hiddenId) hiddenId.value = '';
            if (searchInput) searchInput.value = '';
            state.managerOverride = false;
            state.managerOverrideReason = '';
            if (el('udhariManagerOverrideCheck')) el('udhariManagerOverrideCheck').checked = false;
            if (el('udhariOverrideReasonInput')) el('udhariOverrideReasonInput').value = '';
            updateModeDisplays();
            validateAndUpdateCta();
            return;
        }

        if (hiddenId) hiddenId.value = customer.id;
        if (searchInput) searchInput.value = customer.name + ' (' + customer.mobile + ')';
        if (el('pickedCustomerName')) el('pickedCustomerName').textContent = customer.name;
        if (el('pickedCustomerMobile')) el('pickedCustomerMobile').textContent = customer.mobile;
        if (el('pickedCustomerAddress')) el('pickedCustomerAddress').textContent = customer.address || '';
        if (el('pickedCustomerOutstanding')) el('pickedCustomerOutstanding').textContent = fmtRs(customer.outstandingBalance);
        if (el('pickedCustomerLimit')) el('pickedCustomerLimit').textContent = fmtRs(customer.creditLimit);
        var avail = Math.max(0, round2(customer.creditLimit - customer.outstandingBalance));
        if (el('pickedCustomerAvailable')) el('pickedCustomerAvailable').textContent = fmtRs(avail);
        if (el('pickedCustomerCreditBlocked')) {
            el('pickedCustomerCreditBlocked').classList.toggle('d-none', !!customer.creditAllowed);
        }
        if (summary) summary.classList.remove('d-none');

        updateModeDisplays();
        validateAndUpdateCta();
    }

    function buildSettlePayload() {
        return {
            idempotencyKey: state.idempotencyKey,
            orderType: state.orderType,
            tableCode: state.orderType === 'Dine-in' ? state.tableCode : null,
            parcelToken: null,
            waiterName: 'Counter',
            items: state.items.map(function (i) {
                return {
                    itemName: i.itemName,
                    marathiName: i.marathiName || null,
                    quantity: i.quantity,
                    rate: i.rate,
                    taxPercent: state.gstPercent,
                    notes: i.notes || null
                };
            }),
            discountAmount: state.totals.discount,
            packingCharge: state.totals.packing,
            gstPercent: state.gstPercent,
            paymentMode: state.paymentMode,
            payments: buildPaymentsPayload(),
            customerId: state.selectedCustomer ? state.selectedCustomer.id : null,
            customerName: state.selectedCustomer ? state.selectedCustomer.name : 'Walk-in Customer',
            customerPhone: state.selectedCustomer ? state.selectedCustomer.mobile : null,
            customerAddress: state.selectedCustomer ? state.selectedCustomer.address : null,
            customerOutstanding: state.selectedCustomer ? state.selectedCustomer.outstandingBalance : 0,
            customerCreditLimit: state.selectedCustomer ? state.selectedCustomer.creditLimit : 0,
            customerCreditAllowed: state.selectedCustomer ? state.selectedCustomer.creditAllowed : true,
            isManagerOverride: state.managerOverride,
            managerOverrideReason: state.managerOverrideReason.trim(),
            creditNote: state.udhariNote.trim()
        };
    }

    var CART_STORAGE_KEY = 'restaurantms_pos_cart_v1';

    function persistCartSnapshot() {
        try {
            if (state.items.length === 0) {
                localStorage.removeItem(CART_STORAGE_KEY);
                return;
            }
            localStorage.setItem(CART_STORAGE_KEY, JSON.stringify({
                orderType: state.orderType,
                tableCode: state.tableCode,
                items: state.items,
                discount: state.discount,
                packingCharge: state.packingCharge,
                gstPercent: state.gstPercent,
                selectedCustomer: state.selectedCustomer,
                savedAt: Date.now()
            }));
        } catch (e) { /* storage quota / private mode */ }
    }

    function restoreCartSnapshotIfNeeded() {
        if (state.items.length > 0) return;
        try {
            var raw = localStorage.getItem(CART_STORAGE_KEY);
            if (!raw) return;
            var snap = JSON.parse(raw);
            if (snap && Array.isArray(snap.items) && snap.items.length > 0) {
                state.items = snap.items;
                state.discount = Number(snap.discount || 0);
                state.packingCharge = Number(snap.packingCharge || 0);
                state.gstPercent = Number(snap.gstPercent || 5);
                if (el('billingDiscountInput')) el('billingDiscountInput').value = String(state.discount);
                if (el('billingPackingInput')) el('billingPackingInput').value = String(state.packingCharge);
                if (el('billingGstSelect')) el('billingGstSelect').value = String(state.gstPercent);
                if (snap.selectedCustomer) selectCustomer(snap.selectedCustomer);
                showToast('info', 'Restored your saved POS cart (' + snap.items.length + ' items).');
            }
        } catch (e) { /* ignore corrupted snapshot */ }
    }

    function clearCartSnapshot() {
        try { localStorage.removeItem(CART_STORAGE_KEY); } catch (e) { /* ignore */ }
    }

    function resetForNewOrder() {
        clearCartSnapshot();
        state.idempotencyKey = generateKey();
        state.items = [];
        state.discount = 0;
        state.packingCharge = 0;
        state.cashTouched = false;
        state.cashReceived = 0;
        state.upiRef = '';
        state.upiConfirmed = false;
        state.cardLast4 = '';
        state.cardApproval = '';
        state.cardBank = '';
        state.splitRows = [];
        state.udhariTouched = false;
        state.udhariCreditAmount = 0;
        state.udhariNote = '';
        state.managerOverride = false;
        state.managerOverrideReason = '';
        selectCustomer(null);

        if (el('billingDiscountInput')) el('billingDiscountInput').value = '0';
        if (el('billingPackingInput')) el('billingPackingInput').value = '0';
        if (el('upiRefInput')) el('upiRefInput').value = '';
        if (el('cardLast4Input')) el('cardLast4Input').value = '';
        if (el('cardApprovalInput')) el('cardApprovalInput').value = '';
        if (el('cardBankInput')) el('cardBankInput').value = '';
        if (el('udhariNoteInput')) el('udhariNoteInput').value = '';

        var cashRadio = el('payModeCash');
        if (cashRadio) cashRadio.checked = true;
        switchPaymentMode('Cash');
        renderItemsTable();
    }

    function prependSettledBillRow(data) {
        var tbody = document.getElementById('settledBillsTodayBody');
        if (!tbody) return;
        var emptyRow = tbody.querySelector('[data-empty-bills-row]');
        if (emptyRow) emptyRow.remove();

        var badgeClass = data.primaryPaymentMode === 'Udhari'
            ? 'bg-danger-subtle text-chili'
            : data.primaryPaymentMode === 'Cash'
                ? 'bg-teal-subtle text-teal'
                : 'bg-plum-subtle text-plum';
        var typeClass = data.orderType === 'Dine-in' ? 'bg-plum' : 'bg-saffron text-dark';
        var label = data.parcelToken
            ? ('Parcel ' + data.parcelToken)
            : (data.orderType === 'Dine-in' ? ('Table ' + state.tableCode) : (state.selectedCustomer ? state.selectedCustomer.name : 'Walk-in'));

        var tr = document.createElement('tr');
        tr.className = 'table-success';
        tr.innerHTML =
            '<td class="fw-bold text-plum">' + escapeHtml(data.billNumber) + '</td>' +
            '<td class="small text-muted font-sans">Just now</td>' +
            '<td><span class="badge ' + typeClass + '">' + escapeHtml(data.orderType) + '</span></td>' +
            '<td class="font-sans">' + escapeHtml(label) + '</td>' +
            '<td class="text-end fw-bold text-ink">' + fmtRs(data.grandTotal) + '</td>' +
            '<td class="font-sans"><span class="badge ' + badgeClass + '">' + escapeHtml(data.primaryPaymentMode) + '</span></td>' +
            '<td class="font-sans"><span class="badge bg-teal-subtle text-teal">Settled</span></td>' +
            '<td class="text-end font-sans">' +
            '<button type="button" class="btn btn-sm btn-outline-plum px-2 py-1" data-reprint-bill="' + escapeHtml(data.billNumber) + '" data-bill-amount="' + fmtRs(data.grandTotal) + '">' +
            '<i class="bi bi-printer me-1"></i> Reprint</button>' +
            '</td>';
        tbody.insertBefore(tr, tbody.firstChild);
    }

    function showToast(type, message) {
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                toast: true,
                position: 'top-end',
                icon: type,
                title: message,
                showConfirmButton: false,
                timer: 3500
            });
        }
    }

    function submitSettle() {
        var err = getValidationError();
        if (err || state.settling) {
            validateAndUpdateCta();
            return;
        }

        state.settling = true;
        var btn = el('btnSettleBill');
        var btnText = el('btnSettleBillText');
        if (btn) btn.disabled = true;
        if (btnText) btnText.textContent = 'Settling...';

        fetch(config.settleUrl || '/Billing/Settle', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken(),
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: JSON.stringify(buildSettlePayload())
        })
            .then(function (resp) {
                if (resp.status === 401 || (resp.redirected && resp.url && resp.url.indexOf('/Account/Login') >= 0)) {
                    persistCartSnapshot();
                    throw { sessionExpired: true };
                }
                return resp.json();
            })
            .then(function (res) {
                state.settling = false;
                if (btnText) btnText.textContent = 'Settle & Print Receipt';

                if (!res || !res.isSuccess) {
                    var errMsg = (res && res.error && (res.error.description || res.error.message)) || 'Settlement failed. Please check payment details.';
                    if (el('billingValidationAlert')) el('billingValidationAlert').classList.remove('d-none');
                    if (el('billingValidationText')) el('billingValidationText').textContent = errMsg;
                    showToast('error', errMsg);
                    validateAndUpdateCta();
                    return;
                }

                var data = res.value;
                clearCartSnapshot();
                // Update local customer outstanding if Udhari was posted
                if (data.creditAmount > 0 && state.selectedCustomer) {
                    state.selectedCustomer.outstandingBalance = data.newOutstanding;
                }

                prependSettledBillRow(data);
                showToast('success', 'Bill ' + data.billNumber + ' settled (' + fmtRs(data.grandTotal) + ')');

                // Generate a fresh idempotency key for the next bill
                state.idempotencyKey = generateKey();

                if (window.ReceiptPreview) {
                    window.ReceiptPreview.openBill({
                        billNo: data.billNumber,
                        receiptUrl: data.receiptUrl,
                        parcelTokenUrl: data.parcelTokenUrl,
                        autoPrint: data.autoPrint,
                        paperSize: data.paperSize || config.paperSize || '80mm',
                        copies: data.copies || 1,
                        whatsAppText: data.whatsAppText,
                        customerPhone: data.customerPhone,
                        duplicate: false,
                        onNewOrder: resetForNewOrder
                    });
                }

                validateAndUpdateCta();
            })
            .catch(function (err) {
                state.settling = false;
                if (btnText) btnText.textContent = 'Settle & Print Receipt';
                if (err && err.sessionExpired) {
                    if (typeof Swal !== 'undefined') {
                        Swal.fire({
                            icon: 'warning',
                            title: 'Session Expired',
                            text: 'Your POS cart has been safely saved. Sign in again to resume billing without losing items.',
                            confirmButtonText: 'Sign In Again',
                            confirmButtonColor: '#3D1F6D'
                        }).then(function () {
                            window.location.href = '/Account/Login?ReturnUrl=' + encodeURIComponent(window.location.pathname + window.location.search);
                        });
                    }
                    validateAndUpdateCta();
                    return;
                }
                showToast('error', 'Network error while settling bill. Your order is safely preserved — please retry.');
                validateAndUpdateCta();
            });
    }

    function openDraftPreview() {
        if (state.items.length === 0) {
            showToast('warning', msgs.emptyItems || 'Add at least one item to preview.');
            return;
        }

        fetch(config.previewDraftUrl || '/Receipt/PreviewDraft', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken()
            },
            body: JSON.stringify(buildSettlePayload())
        })
            .then(function (resp) { return resp.text(); })
            .then(function (html) {
                if (window.ReceiptPreview) {
                    window.ReceiptPreview.openDraft(html, {
                        paperSize: config.paperSize || '80mm',
                        title: 'Draft Bill Preview'
                    });
                }
            })
            .catch(function () {
                showToast('error', 'Could not generate draft bill preview.');
            });
    }

    function escapeHtml(str) {
        return String(str || '')
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    function bindEvents() {
        // Delegated click events inside billingPanelRoot
        root.addEventListener('click', function (e) {
            var qtyBtn = e.target.closest('[data-qty-delta]');
            if (qtyBtn) {
                var idx = Number(qtyBtn.getAttribute('data-idx'));
                var delta = Number(qtyBtn.getAttribute('data-qty-delta'));
                if (state.items[idx]) {
                    state.items[idx].quantity += delta;
                    if (state.items[idx].quantity <= 0) {
                        state.items.splice(idx, 1);
                    }
                    renderItemsTable();
                }
                return;
            }

            var remBtn = e.target.closest('[data-remove-item]');
            if (remBtn) {
                var rIdx = Number(remBtn.getAttribute('data-remove-item'));
                state.items.splice(rIdx, 1);
                renderItemsTable();
                return;
            }

            var catAdd = e.target.closest('[data-catalog-add]');
            if (catAdd) {
                var id = Number(catAdd.getAttribute('data-catalog-add'));
                var dish = menuCatalog.find(function (m) { return Number(m.id) === id; });
                addMenuItem(dish);
                showToast('success', 'Added ' + dish.name);
                return;
            }

            var cashQuick = e.target.closest('.cash-quick-btn');
            if (cashQuick) {
                state.cashTouched = true;
                if (cashQuick.getAttribute('data-cash-action') === 'exact') {
                    state.cashReceived = state.totals.grandTotal;
                } else {
                    var addVal = Number(cashQuick.getAttribute('data-cash-add') || 0);
                    state.cashReceived = round2((Number(state.cashReceived) || 0) + addVal);
                }
                if (el('cashReceivedInput')) el('cashReceivedInput').value = state.cashReceived;
                updateModeDisplays();
                validateAndUpdateCta();
                return;
            }

            if (e.target.closest('#btnUpiPaymentReceived')) {
                state.upiConfirmed = !state.upiConfirmed;
                updateModeDisplays();
                validateAndUpdateCta();
                return;
            }

            if (e.target.closest('#btnAddSplitRow')) {
                var paid = round2(state.splitRows.reduce(function (acc, r) { return acc + (Number(r.amount) || 0); }, 0));
                var remaining = Math.max(0, round2(state.totals.grandTotal - paid));
                state.splitRows.push({ mode: 'Cash', amount: remaining, ref: '' });
                renderSplitRows();
                return;
            }

            var splitRemove = e.target.closest('[data-split-remove]');
            if (splitRemove) {
                var sIdx = Number(splitRemove.getAttribute('data-split-remove'));
                state.splitRows.splice(sIdx, 1);
                renderSplitRows();
                return;
            }

            var pickCust = e.target.closest('[data-pick-customer]');
            if (pickCust) {
                var cid = Number(pickCust.getAttribute('data-pick-customer'));
                var found = customersCache.find(function (c) { return Number(c.id) === cid; });
                if (found) selectCustomer(found);
                return;
            }

            if (e.target.closest('#btnClearCustomer')) {
                selectCustomer(null);
                return;
            }

            if (e.target.closest('#btnAddByShortCode')) {
                addByShortCodeInput();
                return;
            }

            if (e.target.closest('#btnPreviewDraftBill')) {
                openDraftPreview();
                return;
            }

            if (e.target.closest('#btnSettleBill')) {
                submitSettle();
            }
        });

        // Delegated change events
        root.addEventListener('change', function (e) {
            if (e.target.name === 'paymentMode') {
                switchPaymentMode(e.target.value);
                return;
            }

            if (e.target.name === 'billingOrderType') {
                state.orderType = e.target.value;
                var isDineIn = state.orderType === 'Dine-in';
                if (el('billingTableSelectorWrap')) el('billingTableSelectorWrap').classList.toggle('d-none', !isDineIn);
                if (el('billingParcelBadge')) el('billingParcelBadge').classList.toggle('d-none', isDineIn);
                return;
            }

            if (e.target.id === 'billingTableSelect') {
                state.tableCode = e.target.value;
                return;
            }

            if (e.target.id === 'billingGstSelect') {
                state.gstPercent = Number(e.target.value) || 0;
                recalculateTotals();
                return;
            }

            if (e.target.id === 'udhariBalanceModeSelect') {
                state.udhariBalanceMode = e.target.value;
                return;
            }

            if (e.target.id === 'udhariManagerOverrideCheck') {
                state.managerOverride = e.target.checked;
                updateModeDisplays();
                validateAndUpdateCta();
                return;
            }

            if (e.target.hasAttribute('data-split-mode')) {
                var idx = Number(e.target.getAttribute('data-split-mode'));
                if (state.splitRows[idx]) {
                    state.splitRows[idx].mode = e.target.value;
                    state.splitRows[idx].ref = '';
                    renderSplitRows();
                }
            }
        });

        // Delegated input events
        root.addEventListener('input', function (e) {
            var id = e.target.id;
            if (id === 'billingDiscountInput') {
                state.discount = Math.max(0, Number(e.target.value) || 0);
                recalculateTotals();
            } else if (id === 'billingPackingInput') {
                state.packingCharge = Math.max(0, Number(e.target.value) || 0);
                recalculateTotals();
            } else if (id === 'cashReceivedInput') {
                state.cashTouched = true;
                state.cashReceived = Number(e.target.value) || 0;
                updateModeDisplays();
                validateAndUpdateCta();
            } else if (id === 'upiRefInput') {
                state.upiRef = e.target.value || '';
                validateAndUpdateCta();
            } else if (id === 'cardLast4Input') {
                e.target.value = e.target.value.replace(/\D/g, '').slice(0, 4);
                state.cardLast4 = e.target.value;
                validateAndUpdateCta();
            } else if (id === 'cardApprovalInput') {
                state.cardApproval = e.target.value || '';
                validateAndUpdateCta();
            } else if (id === 'cardBankInput') {
                state.cardBank = e.target.value || '';
            } else if (id === 'udhariCreditAmountInput') {
                state.udhariTouched = true;
                state.udhariCreditAmount = Math.max(0, Number(e.target.value) || 0);
                updateModeDisplays();
                validateAndUpdateCta();
            } else if (id === 'udhariOverrideReasonInput') {
                state.managerOverrideReason = e.target.value || '';
                validateAndUpdateCta();
            } else if (id === 'udhariNoteInput') {
                state.udhariNote = e.target.value || '';
            } else if (id === 'catalogSearchInput') {
                renderCatalogGrid(e.target.value);
            } else if (id === 'customerSearchInput') {
                searchCustomers(e.target.value);
            } else if (e.target.hasAttribute('data-split-amount')) {
                var aIdx = Number(e.target.getAttribute('data-split-amount'));
                if (state.splitRows[aIdx]) {
                    state.splitRows[aIdx].amount = Math.max(0, Number(e.target.value) || 0);
                    updateModeDisplays();
                    validateAndUpdateCta();
                }
            } else if (e.target.hasAttribute('data-split-ref')) {
                var rIdx = Number(e.target.getAttribute('data-split-ref'));
                if (state.splitRows[rIdx]) {
                    state.splitRows[rIdx].ref = e.target.value || '';
                    validateAndUpdateCta();
                }
            }
        });

        var shortInput = el('billingShortCodeInput');
        if (shortInput) {
            shortInput.addEventListener('keydown', function (e) {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    addByShortCodeInput();
                }
            });
        }

        var custSearch = el('customerSearchInput');
        if (custSearch) {
            custSearch.addEventListener('focus', function () {
                searchCustomers(custSearch.value);
            });
        }

        document.addEventListener('click', function (e) {
            if (!e.target.closest('[data-customer-picker]')) {
                var resultsBox = el('customerSearchResults');
                if (resultsBox) resultsBox.classList.add('d-none');
            }
        });

        // Quick Add Customer Modal Save (modal lives inside _CustomerPicker)
        var btnSaveQuick = el('btnSaveQuickCustomer');
        if (btnSaveQuick) {
            btnSaveQuick.addEventListener('click', saveQuickCustomer);
        }
    }

    function addByShortCodeInput() {
        var input = el('billingShortCodeInput');
        if (!input) return;
        var q = input.value.trim().toLowerCase();
        if (!q) return;

        var match = menuCatalog.find(function (m) {
            return (m.shortCode || '').toLowerCase() === q || (m.name || '').toLowerCase().indexOf(q) >= 0;
        });

        if (match) {
            addMenuItem(match);
            input.value = '';
            input.focus();
        } else {
            showToast('warning', 'No dish matched "' + input.value.trim() + '". Try M01 or dish name.');
        }
    }

    function searchCustomers(term) {
        var q = (term || '').trim().toLowerCase();
        var filtered = customersCache.filter(function (c) {
            if (!q) return true;
            return (c.name || '').toLowerCase().indexOf(q) >= 0 || (c.mobile || '').indexOf(q) >= 0;
        });
        renderCustomerSearchResults(filtered);
    }

    function saveQuickCustomer() {
        var name = (el('quickCustomerName') && el('quickCustomerName').value || '').trim();
        var mobile = (el('quickCustomerMobile') && el('quickCustomerMobile').value || '').trim();
        var address = (el('quickCustomerAddress') && el('quickCustomerAddress').value || '').trim();
        var limit = Number(el('quickCustomerLimit') && el('quickCustomerLimit').value || 5000);
        var errBox = el('quickCustomerError');

        if (!name || !/^[6-9]\d{9}$/.test(mobile)) {
            if (errBox) {
                errBox.textContent = 'Enter full name and a valid 10-digit mobile number starting with 6-9.';
                errBox.classList.remove('d-none');
            }
            return;
        }

        if (errBox) errBox.classList.add('d-none');

        fetch(config.quickAddCustomerUrl || '/Billing/QuickAddCustomer', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken()
            },
            body: JSON.stringify({ name: name, mobile: mobile, address: address, creditLimit: limit, creditAllowed: true })
        })
            .then(function (r) { return r.json(); })
            .then(function (res) {
                if (!res || !res.isSuccess) {
                    if (errBox) {
                        errBox.textContent = (res && res.error && res.error.description) || 'Could not add customer.';
                        errBox.classList.remove('d-none');
                    }
                    return;
                }

                var c = res.value;
                var normalized = {
                    id: c.id,
                    name: c.name,
                    mobile: c.mobile,
                    address: c.address || '',
                    creditAllowed: c.creditAllowed,
                    creditLimit: c.creditLimit,
                    outstandingBalance: c.outstandingBalance
                };

                var exists = customersCache.findIndex(function (x) { return x.id === normalized.id; });
                if (exists >= 0) customersCache[exists] = normalized;
                else customersCache.unshift(normalized);

                selectCustomer(normalized);

                var modalEl = el('quickAddCustomerModal');
                if (modalEl && typeof bootstrap !== 'undefined') {
                    var modal = bootstrap.Modal.getInstance(modalEl);
                    if (modal) modal.hide();
                }

                if (el('quickCustomerName')) el('quickCustomerName').value = '';
                if (el('quickCustomerMobile')) el('quickCustomerMobile').value = '';
                if (el('quickCustomerAddress')) el('quickCustomerAddress').value = '';
                showToast('success', 'Customer ' + normalized.name + ' added & selected');
            })
            .catch(function () {
                if (errBox) {
                    errBox.textContent = 'Network error while saving customer.';
                    errBox.classList.remove('d-none');
                }
            });
    }

    // Boot
    restoreCartSnapshotIfNeeded();
    renderCatalogGrid('');
    renderItemsTable();
    switchPaymentMode('Cash');
    bindEvents();
})();
