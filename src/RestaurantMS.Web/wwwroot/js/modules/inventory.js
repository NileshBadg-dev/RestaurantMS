(function () {
    'use strict';

    var root = document.getElementById('inventoryModuleRoot');
    if (!root) return;

    var quickAddUrl = root.getAttribute('data-quick-add-url') || '/Inventory/QuickAddMaster';

    function parseJson(id, fallback) {
        var el = document.getElementById(id);
        if (!el) return fallback;
        try { return JSON.parse(el.textContent || ''); } catch (e) { return fallback; }
    }

    var stockItems = parseJson('invStockItemsJson', []);
    var units = parseJson('invUnitsJson', []);
    var taxRates = parseJson('invTaxRatesJson', []);

    function getToken() {
        var el = document.querySelector('input[name="__RequestVerificationToken"]');
        return el ? el.value : '';
    }

    function showToast(icon, title) {
        if (typeof Swal !== 'undefined') {
            Swal.fire({ toast: true, position: 'top-end', icon: icon, title: title, showConfirmButton: false, timer: 3000 });
        }
    }

    function closeModal(id) {
        var el = document.getElementById(id);
        if (el && typeof bootstrap !== 'undefined') {
            var m = bootstrap.Modal.getInstance(el);
            if (m) m.hide();
        }
    }

    function escapeHtml(s) {
        return String(s || '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
    }

    // --- Purchase Entry Line Calculations ---
    function recalcPurchaseTotals() {
        var tbody = document.getElementById('invPurchaseLinesBody');
        if (!tbody) return;

        var subtotal = 0;
        var taxTotal = 0;

        tbody.querySelectorAll('tr').forEach(function (tr) {
            var qty = Number(tr.querySelector('.js-line-qty')?.value || 0);
            var rate = Number(tr.querySelector('.js-line-rate')?.value || 0);
            var taxPct = Number(tr.querySelector('.js-line-tax')?.value || 0);
            var baseAmt = Math.round(qty * rate * 100) / 100;
            var taxAmt = Math.round(baseAmt * (taxPct / 100) * 100) / 100;
            var total = Math.round((baseAmt + taxAmt) * 100) / 100;
            subtotal = Math.round((subtotal + baseAmt) * 100) / 100;
            taxTotal = Math.round((taxTotal + taxAmt) * 100) / 100;

            var totalInput = tr.querySelector('.js-line-total');
            if (totalInput) totalInput.value = total.toFixed(2);
        });

        var grandTotal = Math.round((subtotal + taxTotal) * 100) / 100;
        var subEl = document.getElementById('invPurSubtotal');
        var taxEl = document.getElementById('invPurTaxTotal');
        var grandEl = document.getElementById('invPurGrandTotal');
        if (subEl) subEl.textContent = '₹' + subtotal.toFixed(2);
        if (taxEl) taxEl.textContent = '₹' + taxTotal.toFixed(2);
        if (grandEl) grandEl.textContent = '₹' + grandTotal.toFixed(2);

        var hSub = document.getElementById('purSubtotalHidden');
        var hTax = document.getElementById('purTaxHidden');
        var hGrand = document.getElementById('purGrandTotalHidden');
        if (hSub) hSub.value = subtotal.toFixed(2);
        if (hTax) hTax.value = taxTotal.toFixed(2);
        if (hGrand) hGrand.value = grandTotal.toFixed(2);
    }

    function reindexPurchaseRows() {
        var tbody = document.getElementById('invPurchaseLinesBody');
        if (!tbody) return;
        tbody.querySelectorAll('tr').forEach(function (tr, idx) {
            tr.setAttribute('data-line-index', idx);
            tr.querySelectorAll('[name]').forEach(function (input) {
                input.name = input.name.replace(/Items\[\d+\]/, 'Items[' + idx + ']');
            });
        });
    }

    function addPurchaseLineRow() {
        var tbody = document.getElementById('invPurchaseLinesBody');
        if (!tbody) return;
        var idx = tbody.querySelectorAll('tr').length;

        var itemOpts = '<option value="">-- Select Stock Item --</option>' +
            stockItems.map(function (i) {
                return '<option value="' + i.id + '" data-name="' + escapeHtml(i.name) + '" data-unit="' + escapeHtml(i.unit) + '" data-rate="' + i.rate + '">' + escapeHtml(i.name) + '</option>';
            }).join('');

        var unitOpts = (units.length ? units : [{ symbol: 'kg' }, { symbol: 'L' }]).map(function (u) {
            return '<option value="' + escapeHtml(u.symbol) + '">' + escapeHtml(u.symbol) + '</option>';
        }).join('');

        var taxOpts = (taxRates.length ? taxRates : [{ rate: '0' }, { rate: '5' }, { rate: '18' }]).map(function (t) {
            return '<option value="' + escapeHtml(t.rate) + '"' + (t.rate === '5' ? ' selected' : '') + '>' + escapeHtml(t.rate) + '%</option>';
        }).join('');

        var tr = document.createElement('tr');
        tr.setAttribute('data-line-index', idx);
        tr.innerHTML =
            '<td>' +
            '<input type="hidden" name="Items[' + idx + '].StockItemId" value="0" class="js-line-item-id" />' +
            '<select class="form-select form-select-sm js-line-item-select">' + itemOpts + '</select>' +
            '<input type="hidden" name="Items[' + idx + '].ItemName" value="" class="js-line-item-name" />' +
            '</td>' +
            '<td><input type="number" name="Items[' + idx + '].Quantity" class="form-control form-control-sm text-end font-monospace js-line-qty" step="0.01" min="0.01" value="1" required /></td>' +
            '<td><select name="Items[' + idx + '].Unit" class="form-select form-select-sm js-master-unit-select js-line-unit">' + unitOpts + '</select></td>' +
            '<td><input type="number" name="Items[' + idx + '].Rate" class="form-control form-control-sm text-end font-monospace js-line-rate" step="0.01" min="0.01" value="0" required /></td>' +
            '<td><select name="Items[' + idx + '].TaxRate" class="form-select form-select-sm font-monospace js-master-tax-select js-line-tax">' + taxOpts + '</select></td>' +
            '<td><input type="number" name="Items[' + idx + '].Total" class="form-control form-control-sm text-end fw-bold font-monospace js-line-total" value="0.00" readonly /></td>' +
            '<td class="text-end"><button type="button" class="btn btn-sm btn-outline-danger js-remove-purchase-line" title="Remove"><i class="bi bi-trash"></i></button></td>';
        tbody.appendChild(tr);
        recalcPurchaseTotals();
    }

    function updatePaymentTypeUi() {
        var isCredit = document.getElementById('purTypeCredit')?.checked;
        var wrap = document.getElementById('purPayModeWrap');
        if (wrap) wrap.classList.toggle('d-none', !!isCredit);
    }

    // --- Live Stock Info for Adjustment / Wastage / Consumption ---
    function updateSelectedStockMeta() {
        var sel = document.getElementById('invMovementItemSelect');
        if (!sel) return;
        var opt = sel.options[sel.selectedIndex];
        var cur = Number(opt?.getAttribute('data-stock') || 0);
        var unit = opt?.getAttribute('data-unit') || '';
        var rate = Number(opt?.getAttribute('data-rate') || 0);

        var sysEl = document.getElementById('invSystemStockDisplay');
        if (sysEl) sysEl.textContent = cur.toFixed(2) + ' ' + unit;

        var rateEl = document.getElementById('invItemRateDisplay');
        if (rateEl) rateEl.textContent = '₹' + rate.toFixed(2) + ' / ' + unit;

        var physInput = document.getElementById('invPhysicalCountInput');
        var varEl = document.getElementById('invVariancePreview');
        if (physInput && varEl) {
            var diff = Math.round((Number(physInput.value || 0) - cur) * 1000) / 1000;
            var impact = Math.round(Math.abs(diff) * rate * 100) / 100;
            varEl.textContent = (diff >= 0 ? '+' : '') + diff.toFixed(2) + ' ' + unit + ' (₹' + impact.toFixed(2) + ')';
            varEl.className = 'font-monospace fw-bold ' + (diff < 0 ? 'text-chili' : 'text-teal');
        }

        var qtyInput = document.getElementById('invMovementQtyInput');
        var lossEl = document.getElementById('invLossPreview');
        if (qtyInput && lossEl) {
            var q = Number(qtyInput.value || 0);
            var loss = Math.round(q * rate * 100) / 100;
            lossEl.textContent = '₹' + loss.toFixed(2);
        }
    }

    // --- Quick Add AJAX Helpers ---
    function postQuickAdd(payload, errorElId, onSuccess) {
        var errBox = document.getElementById(errorElId);
        if (errBox) errBox.classList.add('d-none');

        fetch(quickAddUrl, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getToken()
            },
            body: JSON.stringify(payload)
        })
            .then(function (r) { return r.json(); })
            .then(function (res) {
                if (!res || !res.isSuccess) {
                    if (errBox) {
                        errBox.textContent = (res && res.error && res.error.description) || 'Could not save entry.';
                        errBox.classList.remove('d-none');
                    }
                    return;
                }
                onSuccess(res.value);
            })
            .catch(function () {
                if (errBox) {
                    errBox.textContent = 'Network error while saving.';
                    errBox.classList.remove('d-none');
                }
            });
    }

    function bindEvents() {
        document.addEventListener('click', function (e) {
            if (e.target.closest('#btnAddPurchaseLine')) {
                addPurchaseLineRow();
                return;
            }

            var remBtn = e.target.closest('.js-remove-purchase-line');
            if (remBtn) {
                var tbody = document.getElementById('invPurchaseLinesBody');
                if (tbody && tbody.querySelectorAll('tr').length > 1) {
                    remBtn.closest('tr').remove();
                    reindexPurchaseRows();
                    recalcPurchaseTotals();
                } else {
                    showToast('warning', 'At least one purchase item row is required.');
                }
                return;
            }

            var qaTaxUnitBtn = e.target.closest('[data-open-quick-tax-unit]');
            if (qaTaxUnitBtn) {
                var kind = qaTaxUnitBtn.getAttribute('data-open-quick-tax-unit');
                document.getElementById('qaTaxUnitKind').value = kind;
                document.getElementById('qaTaxUnitTitle').textContent = kind === 'tax' ? 'Quick Add Tax Rate (Masters)' : 'Quick Add Unit (Masters)';
                document.getElementById('qaTaxUnitValueLabel').textContent = kind === 'tax' ? 'Tax Rate (%) *' : 'Unit Symbol *';
                document.getElementById('qaTaxUnitName').value = '';
                document.getElementById('qaTaxUnitValue').value = '';
                var modalEl = document.getElementById('quickAddTaxUnitModal');
                if (modalEl && typeof bootstrap !== 'undefined') {
                    bootstrap.Modal.getOrCreateInstance(modalEl).show();
                }
                return;
            }

            if (e.target.closest('#btnSaveQuickSupplier')) {
                var name = (document.getElementById('qaSupplierName')?.value || '').trim();
                if (!name) {
                    var err = document.getElementById('qaSupplierError');
                    if (err) { err.textContent = 'Supplier name is required.'; err.classList.remove('d-none'); }
                    return;
                }
                postQuickAdd({
                    kind: 'supplier',
                    name: name,
                    contactPerson: document.getElementById('qaSupplierContact')?.value || '',
                    phone: document.getElementById('qaSupplierPhone')?.value || '',
                    gstin: document.getElementById('qaSupplierGstin')?.value || '',
                    category: document.getElementById('qaSupplierCategory')?.value || 'Grocery'
                }, 'qaSupplierError', function (val) {
                    var sel = document.getElementById('purchaseSupplierSelect');
                    if (sel) {
                        var opt = document.createElement('option');
                        opt.value = val.id;
                        opt.textContent = val.name;
                        opt.selected = true;
                        sel.appendChild(opt);
                    }
                    closeModal('quickAddSupplierModal');
                    showToast('success', 'Supplier "' + val.name + '" added to Masters & selected.');
                });
                return;
            }

            if (e.target.closest('#btnSaveQuickTaxUnit')) {
                var k = document.getElementById('qaTaxUnitKind')?.value || 'unit';
                var n = (document.getElementById('qaTaxUnitName')?.value || '').trim();
                var v = (document.getElementById('qaTaxUnitValue')?.value || '').trim();
                if (!n || !v) {
                    var errTu = document.getElementById('qaTaxUnitError');
                    if (errTu) { errTu.textContent = 'Name and symbol/rate are required.'; errTu.classList.remove('d-none'); }
                    return;
                }
                postQuickAdd({ kind: k, name: n, codeOrRate: v }, 'qaTaxUnitError', function (val) {
                    if (k === 'unit') {
                        units.push({ name: val.name, symbol: val.value });
                        document.querySelectorAll('.js-master-unit-select').forEach(function (sel) {
                            var opt = document.createElement('option');
                            opt.value = val.value;
                            opt.textContent = val.value;
                            sel.appendChild(opt);
                        });
                    } else {
                        var cleanRate = String(val.value || '').replace('%', '').trim();
                        taxRates.push({ name: val.name, rate: cleanRate });
                        document.querySelectorAll('.js-master-tax-select').forEach(function (sel) {
                            var opt = document.createElement('option');
                            opt.value = cleanRate;
                            opt.textContent = cleanRate + '%';
                            sel.appendChild(opt);
                        });
                    }
                    closeModal('quickAddTaxUnitModal');
                    showToast('success', '"' + val.name + '" added to Masters > Tax & Units.');
                });
                return;
            }

            if (e.target.closest('#btnSaveQuickStockItem')) {
                var itemName = (document.getElementById('qaItemName')?.value || '').trim();
                if (!itemName) {
                    var errIt = document.getElementById('qaStockItemError');
                    if (errIt) { errIt.textContent = 'Stock item name is required.'; errIt.classList.remove('d-none'); }
                    return;
                }
                postQuickAdd({
                    kind: 'stockItem',
                    name: itemName,
                    category: document.getElementById('qaItemCategory')?.value || 'General',
                    unit: document.getElementById('qaItemUnit')?.value || 'kg',
                    openingStock: Number(document.getElementById('qaItemOpening')?.value || 0),
                    reorderLevel: Number(document.getElementById('qaItemReorder')?.value || 10),
                    lastPurchaseRate: Number(document.getElementById('qaItemRate')?.value || 0),
                    expiryDate: document.getElementById('qaItemExpiry')?.value || null
                }, 'qaStockItemError', function (item) {
                    stockItems.unshift({ id: item.id, name: item.name, unit: item.unit, rate: item.lastPurchaseRate });
                    document.querySelectorAll('.js-line-item-select').forEach(function (sel) {
                        var opt = document.createElement('option');
                        opt.value = item.id;
                        opt.textContent = item.name + ' (' + item.code + ')';
                        opt.setAttribute('data-name', item.name);
                        opt.setAttribute('data-unit', item.unit);
                        opt.setAttribute('data-rate', item.lastPurchaseRate);
                        sel.appendChild(opt);
                    });
                    closeModal('quickAddStockItemModal');
                    showToast('success', 'Stock item "' + item.name + '" created.');
                    if (!document.getElementById('invPurchaseLinesTable')) {
                        setTimeout(function () { window.location.reload(); }, 600);
                    }
                });
            }
        });

        document.addEventListener('change', function (e) {
            if (e.target.classList.contains('js-line-item-select')) {
                var tr = e.target.closest('tr');
                var opt = e.target.options[e.target.selectedIndex];
                if (tr && opt) {
                    var idEl = tr.querySelector('.js-line-item-id');
                    var nameEl = tr.querySelector('.js-line-item-name');
                    var unitEl = tr.querySelector('.js-line-unit');
                    var rateEl = tr.querySelector('.js-line-rate');
                    if (idEl) idEl.value = opt.value || '0';
                    if (nameEl) nameEl.value = opt.getAttribute('data-name') || '';
                    if (unitEl && opt.getAttribute('data-unit')) unitEl.value = opt.getAttribute('data-unit');
                    if (rateEl && opt.getAttribute('data-rate')) rateEl.value = opt.getAttribute('data-rate');
                    recalcPurchaseTotals();
                }
            }

            if (e.target.classList.contains('js-line-tax')) {
                recalcPurchaseTotals();
            }

            if (e.target.name === 'PaymentType') {
                updatePaymentTypeUi();
            }

            if (e.target.id === 'invMovementItemSelect') {
                updateSelectedStockMeta();
            }
        });

        document.addEventListener('input', function (e) {
            if (e.target.classList.contains('js-line-qty') || e.target.classList.contains('js-line-rate')) {
                recalcPurchaseTotals();
            }
            if (e.target.id === 'invPhysicalCountInput' || e.target.id === 'invMovementQtyInput') {
                updateSelectedStockMeta();
            }
        });
    }

    recalcPurchaseTotals();
    updatePaymentTypeUi();
    updateSelectedStockMeta();
    bindEvents();
})();
