(function () {
    'use strict';

    function initEmployeeSearch() {
        var searchInput = document.getElementById('advEmployeeSearch');
        if (!searchInput) return;

        searchInput.addEventListener('input', function () {
            var q = (searchInput.value || '').trim().toLowerCase();
            document.querySelectorAll('.js-emp-picker-item').forEach(function (item) {
                var hay = item.getAttribute('data-search') || '';
                item.classList.toggle('d-none', q.length > 0 && hay.indexOf(q) === -1);
            });
        });
    }

    function initAutoRecoverSwitch() {
        document.querySelectorAll('.js-adv-autorecover').forEach(function (el) {
            el.addEventListener('change', function () {
                if (el.form) el.form.submit();
            });
        });
    }

    function initGiveAdvanceDrawer() {
        var root = document.getElementById('employeeAdvancesRoot');
        var staffSelect = document.getElementById('advStaffSelect');
        var amountInput = document.getElementById('advAmountInput');
        var planSelect = document.getElementById('advRecoveryPlanSelect');
        var instRow = document.getElementById('advInstalmentRow');
        var instAmount = document.getElementById('advInstalmentAmountInput');
        var startMonth = document.getElementById('advStartMonthInput');
        var previewBody = document.getElementById('advSchedulePreviewBody');
        var capAlert = document.getElementById('advSalaryCapAlert');
        var capText = document.getElementById('advSalaryCapText');
        var inactiveWarn = document.getElementById('advInactiveWarning');
        var submitBtn = document.getElementById('advGiveSubmitBtn');

        if (!root || !staffSelect || !amountInput || !planSelect) return;

        var maxPercent = parseFloat(root.getAttribute('data-max-percent') || '100');
        var previewUrl = root.getAttribute('data-preview-url');

        function evaluateRules() {
            var opt = staffSelect.options[staffSelect.selectedIndex];
            if (!opt) return;

            var salary = parseFloat(opt.getAttribute('data-salary') || '0');
            var outstanding = parseFloat(opt.getAttribute('data-outstanding') || '0');
            var isActive = opt.getAttribute('data-active') === 'true';
            var amount = parseFloat(amountInput.value || '0');

            if (inactiveWarn) inactiveWarn.classList.toggle('d-none', isActive);
            if (submitBtn) submitBtn.disabled = !isActive;

            var maxAllowed = Math.round(salary * (maxPercent / 100));
            var projected = outstanding + amount;
            var exceeds = amount > 0 && projected > maxAllowed;

            if (capAlert) {
                capAlert.classList.toggle('d-none', !exceeds);
                if (exceeds && capText) {
                    capText.textContent = 'Projected outstanding ₹' + projected.toLocaleString('en-IN') +
                        ' exceeds ' + maxPercent + '% salary limit (₹' + maxAllowed.toLocaleString('en-IN') + ').';
                }
            }
        }

        function updateSchedulePreview() {
            var amount = parseFloat(amountInput.value || '0');
            var plan = planSelect.value;
            var isMonthly = plan === 'MonthlyInstalment';

            if (instRow) instRow.classList.toggle('d-none', !isMonthly);
            if (!previewBody || !previewUrl) return;

            if (amount <= 0) {
                previewBody.innerHTML = '<span class="text-muted">Enter amount to preview recovery schedule.</span>';
                return;
            }

            var params = new URLSearchParams();
            params.append('amount', amount.toString());
            params.append('recoveryPlan', plan);
            if (isMonthly && instAmount && instAmount.value) {
                params.append('monthlyInstalmentAmount', instAmount.value);
            }
            if (startMonth && startMonth.value) {
                params.append('startMonth', startMonth.value);
            }

            fetch(previewUrl + '?' + params.toString(), { headers: { 'X-Requested-With': 'XMLHttpRequest' } })
                .then(function (r) { return r.json(); })
                .then(function (res) {
                    if (!res.isSuccess || !res.value || res.value.length === 0) {
                        previewBody.innerHTML = '<span class="text-muted">No instalments to preview.</span>';
                        return;
                    }
                    var html = '<table class="table table-sm table-borderless mb-0"><thead><tr class="text-muted border-bottom"><th>#</th><th>Month</th><th class="text-end">Deduction</th><th class="text-end">Balance</th></tr></thead><tbody>';
                    res.value.forEach(function (row) {
                        html += '<tr><td>' + row.instalmentNumber + '</td><td>' + row.monthLabel + '</td><td class="text-end font-monospace fw-semibold">₹' + Number(row.instalmentAmount).toLocaleString('en-IN') + '</td><td class="text-end font-monospace text-muted">₹' + Number(row.remainingBalance).toLocaleString('en-IN') + '</td></tr>';
                    });
                    html += '</tbody></table>';
                    previewBody.innerHTML = html;
                })
                .catch(function () {
                    previewBody.innerHTML = '<span class="text-muted">Unable to load preview.</span>';
                });
        }

        staffSelect.addEventListener('change', evaluateRules);
        amountInput.addEventListener('input', function () { evaluateRules(); updateSchedulePreview(); });
        planSelect.addEventListener('change', updateSchedulePreview);
        if (instAmount) instAmount.addEventListener('input', updateSchedulePreview);
        if (startMonth) startMonth.addEventListener('change', updateSchedulePreview);

        evaluateRules();
    }

    function initReportHelpers() {
        document.querySelectorAll('.js-report-filter').forEach(function (el) {
            el.addEventListener('change', function () {
                if (el.form) el.form.submit();
            });
        });
        document.querySelectorAll('.js-print-report').forEach(function (btn) {
            btn.addEventListener('click', function () { window.print(); });
        });
    }

    document.addEventListener('DOMContentLoaded', function () {
        initEmployeeSearch();
        initAutoRecoverSwitch();
        initGiveAdvanceDrawer();
        initReportHelpers();
    });
})();
