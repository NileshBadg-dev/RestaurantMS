(function () {
    'use strict';

    function initAutoSubmit() {
        document.querySelectorAll('.js-autosubmit').forEach(function (el) {
            el.addEventListener('change', function () {
                if (el.form) {
                    el.form.submit();
                }
            });
        });
    }

    function initAttendanceRowSave() {
        var root = document.getElementById('staffHubRoot');
        if (!root) return;

        var saveUrl = root.getAttribute('data-save-attendance-url');
        var tokenEl = root.querySelector('input[name="__RequestVerificationToken"]');
        var token = tokenEl ? tokenEl.value : '';

        document.querySelectorAll('.js-save-att').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var row = btn.closest('.js-attendance-row');
                if (!row || !saveUrl) return;

                var staffId = row.getAttribute('data-staff-id');
                var date = row.getAttribute('data-date');
                var status = row.querySelector('.js-att-status').value;
                var inTime = row.querySelector('.js-att-in').value;
                var outTime = row.querySelector('.js-att-out').value;
                var ot = row.querySelector('.js-att-ot').value || '0';
                var remarks = row.querySelector('.js-att-remarks').value || '';

                var body = new URLSearchParams();
                body.append('__RequestVerificationToken', token);
                body.append('staffId', staffId);
                body.append('date', date);
                body.append('status', status);
                body.append('inTime', inTime);
                body.append('outTime', outTime);
                body.append('overtimeHours', ot);
                body.append('remarks', remarks);

                btn.disabled = true;
                fetch(saveUrl, {
                    method: 'POST',
                    headers: { 'X-Requested-With': 'XMLHttpRequest' },
                    body: body
                })
                .then(function (r) { return r.json(); })
                .then(function (res) {
                    btn.disabled = false;
                    if (res.isSuccess) {
                        btn.classList.remove('btn-outline-plum');
                        btn.classList.add('btn-teal', 'text-white');
                        setTimeout(function () {
                            btn.classList.remove('btn-teal', 'text-white');
                            btn.classList.add('btn-outline-plum');
                        }, 1200);
                    } else if (window.Swal) {
                        window.Swal.fire('Cannot Save', res.error || 'Failed to save attendance.', 'error');
                    }
                })
                .catch(function () {
                    btn.disabled = false;
                });
            });
        });
    }

    function initPrintPayslip() {
        document.querySelectorAll('.js-print-payslip').forEach(function (btn) {
            btn.addEventListener('click', function () {
                window.print();
            });
        });
    }

    document.addEventListener('DOMContentLoaded', function () {
        initAutoSubmit();
        initAttendanceRowSave();
        initPrintPayslip();
    });
})();
