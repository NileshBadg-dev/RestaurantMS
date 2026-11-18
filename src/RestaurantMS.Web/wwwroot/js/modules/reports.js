(function () {
    'use strict';

    function getToken() {
        var el = document.querySelector('input[name="__RequestVerificationToken"]');
        return el ? el.value : '';
    }

    function escapeHtml(s) {
        return String(s || '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
    }

    function showToast(icon, title) {
        if (typeof Swal !== 'undefined') {
            Swal.fire({ toast: true, position: 'top-end', icon: icon, title: title, showConfirmButton: false, timer: 2600 });
        }
    }

    function toggleFavoriteRequest(url, code, btn) {
        fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': getToken() },
            body: JSON.stringify({ code: code })
        })
            .then(function (r) { return r.json(); })
            .then(function (res) {
                if (!res || !res.isSuccess) return;
                var isFav = !!res.value.isFavorite;
                var icon = btn.querySelector('i');
                btn.classList.toggle('text-warning', isFav);
                btn.classList.toggle('text-muted', !isFav);
                if (icon) {
                    icon.classList.toggle('bi-star-fill', isFav);
                    icon.classList.toggle('bi-star', !isFav);
                }
                var cardCol = btn.closest('.js-report-card-col');
                if (cardCol) {
                    cardCol.setAttribute('data-favorite', isFav ? 'true' : 'false');
                    updateHubCountsAndVisibility();
                }
                showToast('success', isFav ? 'Added to Favourites' : 'Removed from Favourites');
            });
    }

    // ==================== 1. REPORTS HUB ====================
    var hubRoot = document.getElementById('reportsHubRoot');
    var activeHubCategory = 'All';

    function updateHubCountsAndVisibility() {
        if (!hubRoot) return;
        var searchTerm = (document.getElementById('reportsHubSearch')?.value || '').trim().toLowerCase();
        var cols = hubRoot.querySelectorAll('.js-report-card-col');
        var visibleCount = 0;
        var favCount = 0;

        cols.forEach(function (col) {
            var isFav = col.getAttribute('data-favorite') === 'true';
            if (isFav) favCount++;
            var cat = col.getAttribute('data-category') || '';
            var hay = col.getAttribute('data-search') || '';
            var catMatch = activeHubCategory === 'All' ||
                (activeHubCategory === 'Favourites' && isFav) ||
                cat.toLowerCase() === activeHubCategory.toLowerCase();
            var searchMatch = !searchTerm || hay.indexOf(searchTerm) >= 0;
            var show = catMatch && searchMatch;
            col.classList.toggle('d-none', !show);
            if (show) visibleCount++;
        });

        var favBadge = document.getElementById('hubFavCount');
        if (favBadge) favBadge.textContent = String(favCount);

        var emptyBox = document.getElementById('reportsHubEmpty');
        if (emptyBox) emptyBox.classList.toggle('d-none', visibleCount > 0);
    }

    if (hubRoot) {
        var favUrl = hubRoot.getAttribute('data-toggle-fav-url') || '/Reports/ToggleFavorite';
        var activeBtn = hubRoot.querySelector('.js-hub-cat-tab.btn-plum, .js-hub-cat-tab.btn-saffron');
        if (activeBtn) activeHubCategory = activeBtn.getAttribute('data-category') || 'All';

        hubRoot.addEventListener('click', function (e) {
            var tab = e.target.closest('.js-hub-cat-tab');
            if (tab) {
                activeHubCategory = tab.getAttribute('data-category') || 'All';
                hubRoot.querySelectorAll('.js-hub-cat-tab').forEach(function (b) {
                    b.classList.remove('btn-plum', 'btn-saffron');
                    b.classList.add('btn-light', 'border');
                });
                tab.classList.remove('btn-light', 'border');
                tab.classList.add(activeHubCategory === 'Favourites' ? 'btn-saffron' : 'btn-plum');
                updateHubCountsAndVisibility();
                return;
            }

            var favBtn = e.target.closest('.js-toggle-fav');
            if (favBtn) {
                toggleFavoriteRequest(favUrl, favBtn.getAttribute('data-report-code'), favBtn);
            }
        });

        var searchInput = document.getElementById('reportsHubSearch');
        if (searchInput) {
            searchInput.addEventListener('input', updateHubCountsAndVisibility);
        }

        updateHubCountsAndVisibility();
    }

    // ==================== 2. REPORT DETAIL VIEWER ====================
    var viewerRoot = document.getElementById('reportViewerRoot');
    if (!viewerRoot) return;

    var reportCode = viewerRoot.getAttribute('data-report-code') || 'RPT01';
    var dataUrl = viewerRoot.getAttribute('data-data-url') || '/Reports/Data';
    var exportUrl = viewerRoot.getAttribute('data-export-url') || '/Reports/ExportExcel';
    var printUrl = viewerRoot.getAttribute('data-print-url') || '/Reports/PrintReport';
    var toggleFavUrl = viewerRoot.getAttribute('data-toggle-fav-url') || '/Reports/ToggleFavorite';

    var currentData = null;
    var chartInstance = null;

    function parseInitialData() {
        var el = document.getElementById('reportInitialDataJson');
        if (!el) return null;
        try { return JSON.parse(el.textContent || ''); } catch (e) { return null; }
    }

    function setStateView(mode, errorMessage) {
        var skel = document.getElementById('reportLoadingSkeleton');
        var err = document.getElementById('reportErrorState');
        var empty = document.getElementById('reportEmptyState');
        var stage = document.getElementById('reportContentStage');

        if (skel) skel.classList.toggle('d-none', mode !== 'loading');
        if (err) err.classList.toggle('d-none', mode !== 'error');
        if (empty) empty.classList.toggle('d-none', mode !== 'empty');
        if (stage) stage.classList.toggle('d-none', mode !== 'ready');

        if (mode === 'error' && errorMessage) {
            var errText = document.getElementById('reportErrorText');
            if (errText) errText.textContent = errorMessage;
        }
    }

    function buildQueryParams(extra) {
        var params = new URLSearchParams();
        params.set('code', reportCode);
        params.set('datePreset', document.getElementById('rptDatePresetInput')?.value || 'ThisMonth');
        var fromVal = document.getElementById('rptFromDate')?.value;
        var toVal = document.getElementById('rptToDate')?.value;
        if (fromVal) params.set('from', fromVal);
        if (toVal) params.set('to', toVal);

        var fields = [
            ['shift', 'rptShift'],
            ['waiter', 'rptWaiter'],
            ['table', 'rptTable'],
            ['category', 'rptCategory'],
            ['paymentMode', 'rptPaymentMode'],
            ['orderType', 'rptOrderType']
        ];
        fields.forEach(function (pair) {
            var el = document.getElementById(pair[1]);
            if (el && el.value) params.set(pair[0], el.value);
        });

        params.set('page', document.getElementById('rptPageInput')?.value || '1');
        params.set('pageSize', document.getElementById('rptPageSizeSelect')?.value || '25');

        if (extra) {
            Object.keys(extra).forEach(function (k) { params.set(k, String(extra[k])); });
        }
        return params;
    }

    function syncFilterInputsFromResult(data) {
        var f = data.appliedFilter || {};
        if (document.getElementById('rptDatePresetInput')) document.getElementById('rptDatePresetInput').value = f.datePreset || 'ThisMonth';
        if (document.getElementById('rptFromDate') && f.fromDate) document.getElementById('rptFromDate').value = String(f.fromDate).slice(0, 10);
        if (document.getElementById('rptToDate') && f.toDate) document.getElementById('rptToDate').value = String(f.toDate).slice(0, 10);
        if (document.getElementById('rptShift')) document.getElementById('rptShift').value = f.shift || 'All';
        if (document.getElementById('rptWaiter')) document.getElementById('rptWaiter').value = f.waiter || 'All';
        if (document.getElementById('rptTable')) document.getElementById('rptTable').value = f.table || 'All';
        if (document.getElementById('rptCategory')) document.getElementById('rptCategory').value = f.category || 'All';
        if (document.getElementById('rptPaymentMode')) document.getElementById('rptPaymentMode').value = f.paymentMode || 'All';
        if (document.getElementById('rptOrderType')) document.getElementById('rptOrderType').value = f.orderType || 'All';
        if (document.getElementById('rptPageInput')) document.getElementById('rptPageInput').value = String(data.pageNumber || 1);
        if (document.getElementById('rptPageSizeSelect')) document.getElementById('rptPageSizeSelect').value = String(data.pageSize || 25);

        document.querySelectorAll('.js-date-preset').forEach(function (btn) {
            var active = btn.getAttribute('data-preset') === (f.datePreset || 'ThisMonth');
            btn.classList.toggle('btn-plum', active);
            btn.classList.toggle('btn-light', !active);
            btn.classList.toggle('border', !active);
        });
    }

    function renderReport(data) {
        currentData = data;
        syncFilterInputsFromResult(data);

        var summaryBar = document.getElementById('reportActiveFiltersBar');
        if (summaryBar && data.activeFilterSummary) {
            summaryBar.textContent = data.activeFilterSummary.join('  •  ');
        }

        if (!data.rows || data.rows.length === 0) {
            setStateView('empty');
            return;
        }

        setStateView('ready');
        renderKpis(data.kpis || []);
        renderChart(data.chart);
        renderTable(data);
        renderPagination(data);
    }

    function renderKpis(kpis) {
        var row = document.getElementById('reportKpiRow');
        if (!row) return;
        var toneMap = {
            plum: 'border-primary text-plum',
            teal: 'border-success text-teal',
            saffron: 'border-warning text-dark',
            chili: 'border-danger text-chili'
        };
        row.innerHTML = kpis.map(function (k) {
            var cls = toneMap[k.tone] || toneMap.plum;
            return '<div class="col-6 col-lg-3">' +
                '<div class="card border-0 shadow-sm rounded-3 bg-white p-3 h-100 border-start border-4 ' + cls.split(' ')[0] + '">' +
                '<div class="d-flex justify-content-between align-items-center mb-1">' +
                '<span class="small text-muted fw-semibold text-uppercase">' + escapeHtml(k.label) + '</span>' +
                '<i class="bi ' + escapeHtml(k.icon) + ' ' + cls.split(' ')[1] + ' fs-5"></i>' +
                '</div>' +
                '<div class="fs-4 fw-bold font-monospace ' + cls.split(' ')[1] + '">' + escapeHtml(k.value) + '</div>' +
                '<div class="small text-muted">' + escapeHtml(k.subtitle) + '</div>' +
                '</div></div>';
        }).join('');
    }

    function renderChart(chart) {
        var card = document.getElementById('reportChartCard');
        var canvas = document.getElementById('reportChartCanvas');
        if (!card || !canvas) return;

        if (chartInstance) {
            chartInstance.destroy();
            chartInstance = null;
        }

        if (!chart || !chart.labels || chart.labels.length === 0 || typeof Chart === 'undefined') {
            card.classList.add('d-none');
            return;
        }

        card.classList.remove('d-none');
        var titleEl = document.getElementById('reportChartTitle');
        if (titleEl) titleEl.textContent = chart.title || '';

        var palette = ['#3D1F6D', '#0B7A70', '#FF9F1C', '#C62839', '#522B8E', '#0E9A8D', '#FFB347', '#6B5F80'];
        var isDoughnut = chart.chartType === 'doughnut';

        chartInstance = new Chart(canvas.getContext('2d'), {
            type: chart.chartType || 'bar',
            data: {
                labels: chart.labels,
                datasets: [{
                    label: chart.seriesLabel || 'Value',
                    data: chart.values,
                    backgroundColor: isDoughnut ? palette : 'rgba(61, 31, 109, 0.82)',
                    borderColor: isDoughnut ? '#ffffff' : '#3D1F6D',
                    borderWidth: isDoughnut ? 2 : 1.5,
                    borderRadius: chart.chartType === 'bar' ? 6 : 0,
                    tension: 0.3,
                    fill: chart.chartType === 'line'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: isDoughnut, position: 'right' }
                }
            }
        });
    }

    function renderTable(data) {
        var thead = document.getElementById('reportTableHead');
        var tbody = document.getElementById('reportTableBody');
        var tfoot = document.getElementById('reportTableFoot');
        if (!thead || !tbody || !tfoot) return;

        thead.innerHTML = '<tr>' + data.headers.map(function (h) {
            return '<th>' + escapeHtml(h) + '</th>';
        }).join('') + '<th class="text-end" style="width:42px;"></th></tr>';

        var html = '';
        var lastGroup = null;
        var colSpan = data.headers.length + 1;

        data.rows.forEach(function (row, idx) {
            if (row.groupKey && row.groupKey !== lastGroup) {
                lastGroup = row.groupKey;
                html += '<tr class="table-secondary"><td colspan="' + colSpan + '" class="fw-bold text-plum small py-2">' +
                    '<i class="bi bi-folder2-open me-1"></i>' +
                    escapeHtml((data.groupByLabel ? data.groupByLabel + ': ' : '') + row.groupKey) +
                    '</td></tr>';
            }

            html += '<tr class="report-drill-row" role="button" data-drill-idx="' + idx + '">' +
                row.cells.map(function (c, cIdx) {
                    var cls = cIdx === 0 ? 'fw-semibold text-plum font-monospace' : '';
                    return '<td class="' + cls + '">' + escapeHtml(c) + '</td>';
                }).join('') +
                '<td class="text-end text-muted"><i class="bi bi-chevron-right"></i></td>' +
                '</tr>';
        });

        tbody.innerHTML = html;

        if (data.totalsRow && data.totalsRow.length > 0) {
            tfoot.innerHTML = '<tr>' + data.totalsRow.map(function (c) {
                return '<td>' + escapeHtml(c) + '</td>';
            }).join('') + '<td></td></tr>';
        } else {
            tfoot.innerHTML = '';
        }
    }

    function renderPagination(data) {
        var info = document.getElementById('reportPaginationInfo');
        var links = document.getElementById('reportPaginationLinks');
        if (!info || !links) return;

        var start = data.totalRows === 0 ? 0 : ((data.pageNumber - 1) * data.pageSize) + 1;
        var end = Math.min(data.totalRows, data.pageNumber * data.pageSize);
        info.textContent = 'Showing ' + start + '–' + end + ' of ' + data.totalRows + ' records (Page ' + data.pageNumber + ' of ' + data.totalPages + ')';

        var html = '';
        html += '<li class="page-item ' + (data.pageNumber <= 1 ? 'disabled' : '') + '">' +
            '<button type="button" class="page-link" data-rpt-page="' + (data.pageNumber - 1) + '">&laquo;</button></li>';

        for (var p = 1; p <= data.totalPages; p++) {
            html += '<li class="page-item ' + (p === data.pageNumber ? 'active' : '') + '">' +
                '<button type="button" class="page-link" data-rpt-page="' + p + '">' + p + '</button></li>';
        }

        html += '<li class="page-item ' + (data.pageNumber >= data.totalPages ? 'disabled' : '') + '">' +
            '<button type="button" class="page-link" data-rpt-page="' + (data.pageNumber + 1) + '">&raquo;</button></li>';
        links.innerHTML = html;
    }

    function fetchReportData(extra) {
        setStateView('loading');
        var params = buildQueryParams(extra);
        fetch(dataUrl + '?' + params.toString(), {
            headers: { 'X-Requested-With': 'XMLHttpRequest', 'Accept': 'application/json' }
        })
            .then(function (r) {
                if (!r.ok) throw new Error('HTTP ' + r.status);
                return r.json();
            })
            .then(function (res) {
                if (!res || !res.isSuccess) {
                    setStateView('error', (res && res.error && res.error.description) || 'Unable to load report data.');
                    return;
                }
                renderReport(res.value);
            })
            .catch(function () {
                setStateView('error', 'Failed to load report data. Check your connection and retry.');
            });
    }

    function openDrillDown(idx) {
        if (!currentData || !currentData.rows || !currentData.rows[idx]) return;
        var row = currentData.rows[idx];
        var drawerEl = document.getElementById('reportDrillDownDrawer');
        if (!drawerEl || typeof bootstrap === 'undefined') return;

        var badge = document.getElementById('reportDrillDownTypeBadge');
        var title = document.getElementById('reportDrillDownTitle');
        var list = document.getElementById('reportDrillDownList');
        var actions = document.getElementById('reportDrillDownActions');

        if (badge) badge.textContent = (row.drillDownType || 'RECORD').toUpperCase() + ' DRILL-DOWN';
        if (title) title.textContent = row.drillDownTitle || row.drillDownKey || 'Details';

        if (list) {
            var details = row.drillDownDetails || {};
            var keys = Object.keys(details);
            list.innerHTML = keys.map(function (k) {
                return '<div class="list-group-item d-flex justify-content-between align-items-start gap-2 py-2">' +
                    '<span class="small text-muted">' + escapeHtml(k) + '</span>' +
                    '<strong class="small text-ink text-end font-monospace">' + escapeHtml(details[k]) + '</strong>' +
                    '</div>';
            }).join('');
        }

        if (actions) {
            var btns = '';
            if (row.drillDownType === 'bill' && row.drillDownKey && row.drillDownKey.startsWith('B-')) {
                btns += '<button type="button" class="btn btn-sm btn-plum" data-reprint-bill="' + escapeHtml(row.drillDownKey) + '">' +
                    '<i class="bi bi-printer"></i> Reprint Receipt</button>';
            }
            if (row.drillDownType === 'table') {
                btns += '<a href="/Tables" class="btn btn-sm btn-outline-plum"><i class="bi bi-grid-3x3-gap"></i> Open Floor Plan</a>';
            }
            if (row.drillDownType === 'item') {
                btns += '<a href="/Inventory" class="btn btn-sm btn-outline-teal"><i class="bi bi-boxes"></i> View Stock</a>';
            }
            if (row.drillDownType === 'customer') {
                btns += '<a href="/Customers/Ledger" class="btn btn-sm btn-outline-plum"><i class="bi bi-people"></i> Open Customer Ledger</a>';
            }
            if (row.drillDownType === 'staff') {
                btns += '<a href="/Staff" class="btn btn-sm btn-outline-plum"><i class="bi bi-person-badge"></i> Open Staff Module</a>';
            }
            btns += '<button type="button" class="btn btn-sm btn-outline-secondary ms-auto" data-bs-dismiss="offcanvas">Close</button>';
            actions.innerHTML = btns;
        }

        bootstrap.Offcanvas.getOrCreateInstance(drawerEl).show();
    }

    function openPrintOrPdfPreview(autoPrint) {
        var params = buildQueryParams();
        var fullUrl = printUrl + '?' + params.toString();
        if (window.ReceiptPreview) {
            window.ReceiptPreview.openBill({
                billNo: reportCode,
                receiptUrl: fullUrl,
                paperSize: 'A4',
                autoPrint: autoPrint,
                duplicate: false
            });
        } else {
            window.open(fullUrl, '_blank', 'noopener');
        }
    }

    // Bind Viewer Events
    var filterForm = document.getElementById('reportFilterForm');
    if (filterForm) {
        filterForm.addEventListener('submit', function (e) {
            e.preventDefault();
            if (document.getElementById('rptPageInput')) document.getElementById('rptPageInput').value = '1';
            fetchReportData();
        });
    }

    viewerRoot.addEventListener('click', function (e) {
        var presetBtn = e.target.closest('.js-date-preset');
        if (presetBtn) {
            var preset = presetBtn.getAttribute('data-preset') || 'ThisMonth';
            if (document.getElementById('rptDatePresetInput')) document.getElementById('rptDatePresetInput').value = preset;
            if (document.getElementById('rptPageInput')) document.getElementById('rptPageInput').value = '1';
            if (preset !== 'Custom') {
                fetchReportData();
            } else {
                document.querySelectorAll('.js-date-preset').forEach(function (b) {
                    b.classList.toggle('btn-plum', b === presetBtn);
                    b.classList.toggle('btn-light', b !== presetBtn);
                    b.classList.toggle('border', b !== presetBtn);
                });
                document.getElementById('rptFromDate')?.focus();
            }
            return;
        }

        if (e.target.closest('#btnResetReportFilter') || e.target.closest('#btnEmptyResetFilters')) {
            if (document.getElementById('rptPageInput')) document.getElementById('rptPageInput').value = '1';
            fetchReportData({ reset: 'true' });
            return;
        }

        if (e.target.closest('#btnRetryReport')) {
            fetchReportData();
            return;
        }

        var pageBtn = e.target.closest('[data-rpt-page]');
        if (pageBtn) {
            var targetPage = Number(pageBtn.getAttribute('data-rpt-page') || 1);
            if (targetPage >= 1 && currentData && targetPage <= currentData.totalPages) {
                if (document.getElementById('rptPageInput')) document.getElementById('rptPageInput').value = String(targetPage);
                fetchReportData();
            }
            return;
        }

        var drillRow = e.target.closest('.report-drill-row');
        if (drillRow) {
            openDrillDown(Number(drillRow.getAttribute('data-drill-idx')));
            return;
        }

        var favBtn = e.target.closest('.js-toggle-fav');
        if (favBtn) {
            toggleFavoriteRequest(toggleFavUrl, favBtn.getAttribute('data-report-code'), favBtn);
            return;
        }

        if (e.target.closest('#btnExportReportExcel')) {
            var params = buildQueryParams();
            window.location.href = exportUrl + '?' + params.toString();
            return;
        }

        if (e.target.closest('#btnPrintReport')) {
            openPrintOrPdfPreview(true);
            return;
        }

        if (e.target.closest('#btnExportReportPdf')) {
            openPrintOrPdfPreview(false);
        }
    });

    var pageSizeSel = document.getElementById('rptPageSizeSelect');
    if (pageSizeSel) {
        pageSizeSel.addEventListener('change', function () {
            if (document.getElementById('rptPageInput')) document.getElementById('rptPageInput').value = '1';
            if (document.getElementById('rptPageSizeInput')) document.getElementById('rptPageSizeInput').value = pageSizeSel.value;
            fetchReportData();
        });
    }

    // Boot initial render from server-embedded JSON
    var initial = parseInitialData();
    if (initial) {
        renderReport(initial);
    }
})();
