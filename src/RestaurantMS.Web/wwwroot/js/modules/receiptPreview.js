(function () {
    'use strict';

    var state = {
        mode: 'bill', // 'bill' | 'kot' | 'draft'
        billNo: null,
        kotNo: null,
        baseReceiptUrl: null,
        baseParcelUrl: null,
        activeSlip: 'bill', // 'bill' | 'parcel'
        paperSize: '80mm',
        duplicate: false,
        autoPrintPending: false,
        copies: 1,
        whatsAppText: '',
        customerPhone: '',
        draftHtml: null,
        onNewOrder: null
    };

    function getEl(id) {
        return document.getElementById(id);
    }

    function showFallback(message) {
        var alertEl = getEl('receiptPrintFallbackAlert');
        var msgEl = getEl('receiptPrintFallbackMessage');
        if (alertEl && msgEl) {
            if (message) {
                msgEl.textContent = message;
            }
            alertEl.classList.remove('d-none');
        }
    }

    function hideFallback() {
        var alertEl = getEl('receiptPrintFallbackAlert');
        if (alertEl) {
            alertEl.classList.add('d-none');
        }
    }

    function setPaperClass(paper) {
        var iframe = getEl('receiptPreviewIframe');
        if (!iframe) return;
        iframe.classList.remove('paper-58mm', 'paper-80mm', 'paper-A4');
        var normalized = paper === '58mm' || paper === 'A4' ? paper : '80mm';
        iframe.classList.add('paper-' + normalized);

        document.querySelectorAll('.receipt-paper-btn').forEach(function (btn) {
            btn.classList.toggle('active', btn.getAttribute('data-paper') === normalized);
        });
    }

    function buildUrlWithQuery(baseUrl, params) {
        if (!baseUrl) return '';
        var url = new URL(baseUrl, window.location.origin);
        Object.keys(params).forEach(function (k) {
            if (params[k] !== null && params[k] !== undefined && params[k] !== '') {
                url.searchParams.set(k, String(params[k]));
            }
        });
        return url.pathname + url.search;
    }

    function triggerIframePrint() {
        var iframe = getEl('receiptPreviewIframe');
        if (!iframe || !iframe.contentWindow) {
            showFallback('Printer preview is not ready. Please click Print Again or Download PDF.');
            return false;
        }

        try {
            iframe.contentWindow.focus();
            iframe.contentWindow.print();
            return true;
        } catch (err) {
            showFallback('Printing is unavailable in this browser session. You can still view or download the receipt below.');
            return false;
        }
    }

    function loadCurrentFrame(shouldAutoPrint) {
        var iframe = getEl('receiptPreviewIframe');
        if (!iframe) return;

        hideFallback();
        setPaperClass(state.paperSize);
        state.autoPrintPending = !!shouldAutoPrint;

        if (state.mode === 'draft' && state.draftHtml) {
            iframe.removeAttribute('src');
            iframe.srcdoc = state.draftHtml;
            return;
        }

        iframe.removeAttribute('srcdoc');
        var targetUrl = '';
        if (state.mode === 'kot') {
            targetUrl = buildUrlWithQuery('/Receipt/Kot/' + encodeURIComponent(state.kotNo || 'KOT'), {
                paper: state.paperSize,
                reprint: state.duplicate ? 'true' : 'false'
            });
        } else if (state.activeSlip === 'parcel' && state.baseParcelUrl) {
            targetUrl = buildUrlWithQuery(state.baseParcelUrl, { paper: state.paperSize });
        } else {
            var base = state.baseReceiptUrl || ('/Receipt/Bill/' + encodeURIComponent(state.billNo || ''));
            targetUrl = buildUrlWithQuery(base, {
                paper: state.paperSize,
                duplicate: state.duplicate ? 'true' : 'false'
            });
        }

        iframe.src = targetUrl;
    }

    function openModal() {
        var modalEl = getEl('receiptPreviewModal');
        if (!modalEl || typeof bootstrap === 'undefined') return;
        var modal = bootstrap.Modal.getOrCreateInstance(modalEl);
        modal.show();
    }

    function updateSlipSwitcher() {
        var switcher = getEl('receiptSlipSwitcher');
        var btnBill = getEl('btnShowTaxReceipt');
        var btnParcel = getEl('btnShowParcelToken');
        if (!switcher) return;

        if (state.mode === 'bill' && state.baseParcelUrl) {
            switcher.classList.remove('d-none');
            if (btnBill) {
                btnBill.classList.toggle('btn-plum', state.activeSlip === 'bill');
                btnBill.classList.toggle('btn-outline-plum', state.activeSlip !== 'bill');
            }
            if (btnParcel) {
                btnParcel.classList.toggle('btn-plum', state.activeSlip === 'parcel');
                btnParcel.classList.toggle('btn-outline-plum', state.activeSlip !== 'parcel');
            }
        } else {
            switcher.classList.add('d-none');
        }
    }

    function initEvents() {
        var iframe = getEl('receiptPreviewIframe');
        if (iframe) {
            iframe.addEventListener('load', function () {
                if (state.autoPrintPending) {
                    state.autoPrintPending = false;
                    setTimeout(function () {
                        triggerIframePrint();
                    }, 180);
                }
            });
            iframe.addEventListener('error', function () {
                showFallback('Could not load receipt document. Please try again.');
            });
        }

        document.addEventListener('click', function (e) {
            var paperBtn = e.target.closest('.receipt-paper-btn');
            if (paperBtn) {
                state.paperSize = paperBtn.getAttribute('data-paper') || '80mm';
                loadCurrentFrame(false);
                return;
            }

            if (e.target.closest('#btnReceiptPrintAgain')) {
                triggerIframePrint();
                return;
            }

            if (e.target.closest('#btnReceiptDownloadPdf')) {
                var iframeEl = getEl('receiptPreviewIframe');
                if (iframeEl && iframeEl.contentDocument && iframeEl.contentDocument.documentElement) {
                    try {
                        var htmlContent = '<!DOCTYPE html>\n' + iframeEl.contentDocument.documentElement.outerHTML;
                        var blob = new Blob([htmlContent], { type: 'text/html;charset=utf-8' });
                        var downloadUrl = URL.createObjectURL(blob);
                        var link = document.createElement('a');
                        link.href = downloadUrl;
                        link.download = (state.billNo || state.kotNo || 'receipt') + '-' + state.paperSize + '.html';
                        document.body.appendChild(link);
                        link.click();
                        document.body.removeChild(link);
                        setTimeout(function () { URL.revokeObjectURL(downloadUrl); }, 2000);
                    } catch (err) {
                        triggerIframePrint();
                    }
                } else {
                    triggerIframePrint();
                }
                return;
            }

            if (e.target.closest('#btnShowTaxReceipt')) {
                state.activeSlip = 'bill';
                updateSlipSwitcher();
                loadCurrentFrame(false);
                return;
            }

            if (e.target.closest('#btnShowParcelToken') || e.target.closest('#btnPrintBothSlips')) {
                var auto = !!e.target.closest('#btnPrintBothSlips');
                state.activeSlip = 'parcel';
                updateSlipSwitcher();
                loadCurrentFrame(auto);
                return;
            }

            if (e.target.closest('#btnReceiptShareWhatsApp')) {
                e.preventDefault();
                var phone = (state.customerPhone || '').replace(/\D/g, '');
                if (phone.length === 10) phone = '91' + phone;
                var text = encodeURIComponent(state.whatsAppText || ('Receipt #' + (state.billNo || '')));
                var waUrl = phone ? ('https://wa.me/' + phone + '?text=' + text) : ('https://wa.me/?text=' + text);
                window.open(waUrl, '_blank', 'noopener');
                return;
            }

            if (e.target.closest('#btnReceiptShareSms')) {
                e.preventDefault();
                var smsPhone = (state.customerPhone || '').replace(/\D/g, '');
                var body = encodeURIComponent(state.whatsAppText || ('Receipt #' + (state.billNo || '')));
                window.location.href = 'sms:' + smsPhone + '?body=' + body;
                return;
            }

            if (e.target.closest('#btnReceiptNewOrder')) {
                var modalEl = getEl('receiptPreviewModal');
                if (modalEl && typeof bootstrap !== 'undefined') {
                    var modal = bootstrap.Modal.getInstance(modalEl);
                    if (modal) modal.hide();
                }
                if (typeof state.onNewOrder === 'function') {
                    state.onNewOrder();
                } else {
                    window.location.href = '/Billing';
                }
                return;
            }

            var kotBtn = e.target.closest('[data-print-kot]');
            if (kotBtn) {
                e.preventDefault();
                var kotNo = kotBtn.getAttribute('data-print-kot');
                window.ReceiptPreview.openKot(kotNo, { autoPrint: false, reprint: true });
                return;
            }

            var reprintBtn = e.target.closest('[data-reprint-bill]');
            if (reprintBtn) {
                e.preventDefault();
                var billNo = reprintBtn.getAttribute('data-reprint-bill');
                var amountText = reprintBtn.getAttribute('data-bill-amount') || '';
                window.ReceiptPreview.openBill({
                    billNo: billNo,
                    duplicate: true,
                    autoPrint: false,
                    whatsAppText: 'Duplicate copy of Bill #' + billNo + (amountText ? ' (' + amountText + ')' : '')
                });
            }
        });

        // Auto-open KOT preview if TempData hook triggered it after SendKot
        var kotHook = getEl('kotAutoPrintHook');
        if (kotHook) {
            var kotNo = kotHook.getAttribute('data-kot-number');
            var autoPrint = kotHook.getAttribute('data-kot-autoprint') === 'true';
            if (kotNo && autoPrint) {
                window.ReceiptPreview.openKot(kotNo, { autoPrint: true, reprint: false });
            }
        }
    }

    window.ReceiptPreview = {
        openBill: function (opts) {
            opts = opts || {};
            state.mode = 'bill';
            state.billNo = opts.billNo || '';
            state.baseReceiptUrl = opts.receiptUrl || ('/Receipt/Bill/' + encodeURIComponent(state.billNo));
            state.baseParcelUrl = opts.parcelTokenUrl || null;
            state.activeSlip = 'bill';
            state.paperSize = opts.paperSize || '80mm';
            state.duplicate = !!opts.duplicate;
            state.copies = opts.copies || 1;
            state.whatsAppText = opts.whatsAppText || ('Receipt #' + state.billNo);
            state.customerPhone = opts.customerPhone || '';
            state.draftHtml = null;
            state.onNewOrder = opts.onNewOrder || null;

            var titleEl = getEl('receiptPreviewModalTitle');
            var badgeEl = getEl('receiptPreviewBadge');
            var shareGroup = getEl('receiptShareGroup');
            var newOrderBtn = getEl('btnReceiptNewOrder');

            if (titleEl) titleEl.textContent = 'Receipt Preview — ' + (state.billNo || 'Invoice');
            if (badgeEl) {
                if (state.duplicate) {
                    badgeEl.textContent = 'DUPLICATE';
                    badgeEl.classList.remove('d-none');
                } else {
                    badgeEl.classList.add('d-none');
                }
            }
            if (shareGroup) shareGroup.classList.remove('d-none');
            if (newOrderBtn) newOrderBtn.classList.remove('d-none');

            updateSlipSwitcher();
            openModal();
            loadCurrentFrame(!!opts.autoPrint);
        },

        openDraft: function (html, opts) {
            opts = opts || {};
            state.mode = 'draft';
            state.billNo = 'DRAFT';
            state.baseReceiptUrl = null;
            state.baseParcelUrl = null;
            state.paperSize = opts.paperSize || '80mm';
            state.duplicate = false;
            state.draftHtml = html || '';
            state.onNewOrder = null;

            var titleEl = getEl('receiptPreviewModalTitle');
            var badgeEl = getEl('receiptPreviewBadge');
            var shareGroup = getEl('receiptShareGroup');
            var newOrderBtn = getEl('btnReceiptNewOrder');

            if (titleEl) titleEl.textContent = opts.title || 'Draft Bill Preview';
            if (badgeEl) {
                badgeEl.textContent = 'DRAFT';
                badgeEl.classList.remove('d-none');
            }
            if (shareGroup) shareGroup.classList.add('d-none');
            if (newOrderBtn) newOrderBtn.classList.add('d-none');

            updateSlipSwitcher();
            openModal();
            loadCurrentFrame(false);
        },

        openKot: function (kotNo, opts) {
            opts = opts || {};
            state.mode = 'kot';
            state.kotNo = kotNo || 'KOT';
            state.baseReceiptUrl = null;
            state.baseParcelUrl = null;
            state.paperSize = opts.paperSize || '80mm';
            state.duplicate = !!opts.reprint;
            state.draftHtml = null;
            state.onNewOrder = null;

            var titleEl = getEl('receiptPreviewModalTitle');
            var badgeEl = getEl('receiptPreviewBadge');
            var shareGroup = getEl('receiptShareGroup');
            var newOrderBtn = getEl('btnReceiptNewOrder');

            if (titleEl) titleEl.textContent = 'KOT Preview — ' + state.kotNo;
            if (badgeEl) {
                badgeEl.textContent = opts.reprint ? 'KOT REPRINT' : 'KITCHEN TICKET';
                badgeEl.classList.remove('d-none');
            }
            if (shareGroup) shareGroup.classList.add('d-none');
            if (newOrderBtn) newOrderBtn.classList.add('d-none');

            updateSlipSwitcher();
            openModal();
            loadCurrentFrame(!!opts.autoPrint);
        }
    };

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initEvents);
    } else {
        initEvents();
    }
})();
