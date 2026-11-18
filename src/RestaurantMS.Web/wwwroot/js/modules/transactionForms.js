(function () {
    'use strict';

    function initVoucherForm() {
        var table = document.getElementById('voucherLinesTable');
        if (!table) return;

        var tbody = table.querySelector('tbody');
        var addBtn = document.getElementById('addVoucherLineBtn');
        var totalDrEl = document.getElementById('totalDebitCell');
        var totalCrEl = document.getElementById('totalCreditCell');
        var badgeEl = document.getElementById('balanceStatusBadge');
        var totalDrInput = document.getElementById('TotalDebitHidden');
        var totalCrInput = document.getElementById('TotalCreditHidden');

        function recalc() {
            var dr = 0;
            var cr = 0;
            tbody.querySelectorAll('tr').forEach(function (tr) {
                dr += parseFloat(tr.querySelector('.js-vch-dr')?.value || '0') || 0;
                cr += parseFloat(tr.querySelector('.js-vch-cr')?.value || '0') || 0;
            });
            if (totalDrEl) totalDrEl.textContent = '₹' + dr.toFixed(2);
            if (totalCrEl) totalCrEl.textContent = '₹' + cr.toFixed(2);
            if (totalDrInput) totalDrInput.value = dr.toFixed(2);
            if (totalCrInput) totalCrInput.value = cr.toFixed(2);

            if (badgeEl) {
                var balanced = dr > 0 && Math.abs(dr - cr) < 0.01;
                badgeEl.className = 'badge ' + (balanced ? 'bg-teal text-white' : 'bg-chili text-white');
                badgeEl.textContent = balanced ? 'Balanced' : ('Diff: ₹' + Math.abs(dr - cr).toFixed(2));
            }
        }

        if (addBtn) {
            addBtn.addEventListener('click', function () {
                var idx = tbody.querySelectorAll('tr').length;
                var tr = document.createElement('tr');
                tr.innerHTML =
                    '<td><input type="text" name="Lines[' + idx + '].AccountName" class="form-control form-control-sm" placeholder="Ledger account name" required /></td>' +
                    '<td><div class="input-group input-group-sm"><input type="number" name="Lines[' + idx + '].Debit" class="form-control text-end js-vch-dr" step="0.01" min="0" value="0" /><span class="input-group-text">₹</span></div></td>' +
                    '<td><div class="input-group input-group-sm"><input type="number" name="Lines[' + idx + '].Credit" class="form-control text-end js-vch-cr" step="0.01" min="0" value="0" /><span class="input-group-text">₹</span></div></td>' +
                    '<td><input type="text" name="Lines[' + idx + '].Narration" class="form-control form-control-sm" placeholder="Line memo" /></td>' +
                    '<td class="text-end"><button type="button" class="btn btn-sm btn-outline-danger js-del-line"><i class="bi bi-trash"></i></button></td>';
                tbody.appendChild(tr);
                tr.querySelector('input').focus();
                recalc();
            });
        }

        tbody.addEventListener('input', recalc);
        tbody.addEventListener('click', function (e) {
            var del = e.target.closest('.js-del-line');
            if (del && tbody.querySelectorAll('tr').length > 2) {
                del.closest('tr').remove();
                recalc();
            }
        });
        recalc();
    }

    function initPurchaseForm() {
        var table = document.getElementById('purchaseItemsTable');
        if (!table) return;

        var tbody = table.querySelector('tbody');
        var addBtn = document.getElementById('addPurchaseItemBtn');
        var subEl = document.getElementById('purchaseSubtotal');
        var taxEl = document.getElementById('purchaseTaxTotal');
        var grandEl = document.getElementById('purchaseGrandTotal');
        var grandInput = document.getElementById('GrandTotalHidden');

        function recalc() {
            var subtotal = 0;
            var taxTotal = 0;
            tbody.querySelectorAll('tr').forEach(function (tr) {
                var qty = parseFloat(tr.querySelector('.js-pur-qty')?.value || '0') || 0;
                var rate = parseFloat(tr.querySelector('.js-pur-rate')?.value || '0') || 0;
                var taxPct = parseFloat(tr.querySelector('.js-pur-tax')?.value || '0') || 0;
                var lineSub = qty * rate;
                var lineTax = lineSub * (taxPct / 100);
                var lineTotal = lineSub + lineTax;
                var totalInput = tr.querySelector('.js-pur-total');
                if (totalInput) totalInput.value = lineTotal.toFixed(2);
                subtotal += lineSub;
                taxTotal += lineTax;
            });
            var grand = subtotal + taxTotal;
            if (subEl) subEl.textContent = '₹' + subtotal.toFixed(2);
            if (taxEl) taxEl.textContent = '₹' + taxTotal.toFixed(2);
            if (grandEl) grandEl.textContent = '₹' + grand.toFixed(2);
            if (grandInput) grandInput.value = grand.toFixed(2);
        }

        if (addBtn) {
            addBtn.addEventListener('click', function () {
                var idx = tbody.querySelectorAll('tr').length;
                var tr = document.createElement('tr');
                tr.innerHTML =
                    '<td><input type="text" name="Items[' + idx + '].ItemName" class="form-control form-control-sm" placeholder="Raw material name" required /></td>' +
                    '<td><input type="number" name="Items[' + idx + '].Quantity" class="form-control form-control-sm text-end js-pur-qty" step="0.01" min="0.01" value="1" /></td>' +
                    '<td><select name="Items[' + idx + '].Unit" class="form-select form-select-sm"><option value="Kg">Kg</option><option value="Ltr">Ltr</option><option value="Pkt">Pkt</option><option value="Nos">Nos</option></select></td>' +
                    '<td><div class="input-group input-group-sm"><input type="number" name="Items[' + idx + '].Rate" class="form-control text-end js-pur-rate" step="0.01" min="0" value="0" /><span class="input-group-text">₹</span></div></td>' +
                    '<td><div class="input-group input-group-sm"><input type="number" name="Items[' + idx + '].TaxRate" class="form-control text-end js-pur-tax" step="0.5" min="0" value="5" /><span class="input-group-text">%</span></div></td>' +
                    '<td><input type="number" name="Items[' + idx + '].Total" class="form-control form-control-sm text-end fw-bold font-monospace js-pur-total" value="0" readonly /></td>' +
                    '<td class="text-end"><button type="button" class="btn btn-sm btn-outline-danger js-del-line"><i class="bi bi-trash"></i></button></td>';
                tbody.appendChild(tr);
                tr.querySelector('input').focus();
                recalc();
            });
        }

        tbody.addEventListener('input', recalc);
        tbody.addEventListener('click', function (e) {
            var del = e.target.closest('.js-del-line');
            if (del && tbody.querySelectorAll('tr').length > 1) {
                del.closest('tr').remove();
                recalc();
            }
        });
        recalc();
    }

    document.addEventListener('DOMContentLoaded', function () {
        initVoucherForm();
        initPurchaseForm();
    });
})();
