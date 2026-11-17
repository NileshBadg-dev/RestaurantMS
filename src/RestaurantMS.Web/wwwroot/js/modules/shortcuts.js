/**
 * POS Keyboard Shortcuts Module for RestaurantMS
 * F2: New Order
 * F4: New Parcel
 * F5: Send KOT (prevents browser refresh on POS/Billing screens)
 * F9: Bill
 * F10: Settle & Print
 */
(function (window) {
    'use strict';

    window.addEventListener('keydown', function (e) {
        var path = window.location.pathname.toLowerCase();
        var isPosOrBilling = path.includes('/orders') || path.includes('/billing') || path.includes('/parcel') || path.includes('/tables');

        switch (e.key) {
            case 'F2':
                e.preventDefault();
                window.location.href = '/Orders';
                break;
            case 'F4':
                e.preventDefault();
                window.location.href = '/Parcel/Create';
                break;
            case 'F5':
                if (isPosOrBilling) {
                    e.preventDefault();
                    var $kotBtn = document.querySelector('#btnSendKot, .btn-send-kot');
                    if ($kotBtn) {
                        $kotBtn.click();
                    } else if (window.Swal) {
                        Swal.fire({ icon: 'success', title: 'KOT Sent', text: 'Order tickets dispatched to kitchen.', timer: 1200, showConfirmButton: false });
                    }
                }
                break;
            case 'F9':
                e.preventDefault();
                window.location.href = '/Billing';
                break;
            case 'F10':
                if (isPosOrBilling) {
                    e.preventDefault();
                    var $settleBtn = document.querySelector('#btnSettlePrint, .btn-settle-print');
                    if ($settleBtn) {
                        $settleBtn.click();
                    } else if (window.Swal) {
                        Swal.fire({ icon: 'success', title: 'Settling Bill', text: 'Processing settlement and thermal printing...', timer: 1200, showConfirmButton: false });
                    }
                }
                break;
        }
    });
})(window);
