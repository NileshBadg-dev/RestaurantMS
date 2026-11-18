(function () {
    'use strict';

    function switchMasterTab(tabId, category, highlightKey) {
        var buttons = document.querySelectorAll('.master-tab-btn');
        buttons.forEach(function (b) { b.classList.remove('active'); });
        var activeBtn = document.getElementById('tab-btn-' + tabId);
        if (activeBtn) {
            activeBtn.classList.add('active');
            activeBtn.scrollIntoView({ behavior: 'smooth', block: 'nearest', inline: 'center' });
        }

        var newUrl = '/Masters?tab=' + encodeURIComponent(tabId);
        if (category && category !== 'All') {
            newUrl += '&category=' + encodeURIComponent(category);
        }
        window.history.pushState({ tab: tabId, category: category }, '', newUrl);

        localStorage.setItem('masters_active_tab', tabId);
        document.cookie = 'masters_active_tab=' + encodeURIComponent(tabId) + '; path=/; max-age=' + (30 * 24 * 60 * 60);

        var container = document.getElementById('masterTabContentContainer');
        if (!container) return;
        container.style.opacity = '0.5';

        var fetchUrl = '/Masters/TabContent?tab=' + encodeURIComponent(tabId);
        if (category) {
            fetchUrl += '&category=' + encodeURIComponent(category);
        }

        fetch(fetchUrl, { headers: { 'X-Requested-With': 'XMLHttpRequest' } })
            .then(function (r) { return r.text(); })
            .then(function (html) {
                container.innerHTML = html;
                container.style.opacity = '1';
                if (highlightKey && window.AppForm) {
                    window.AppForm.highlightRow(highlightKey);
                }
            })
            .catch(function () {
                container.style.opacity = '1';
            });
    }

    document.addEventListener('DOMContentLoaded', function () {
        document.addEventListener('click', function (e) {
            var tabBtn = e.target.closest('.js-master-tab-trigger');
            if (tabBtn) {
                e.preventDefault();
                switchMasterTab(tabBtn.getAttribute('data-tab'));
                return;
            }

            var chipBtn = e.target.closest('.js-category-chip');
            if (chipBtn) {
                e.preventDefault();
                switchMasterTab('menu-items', chipBtn.getAttribute('data-category'));
                return;
            }

            var delBtn = e.target.closest('.js-master-delete');
            if (delBtn && window.Swal) {
                var name = delBtn.getAttribute('data-name') || 'this record';
                window.Swal.fire({
                    title: 'Confirm Delete',
                    text: 'Are you sure you want to remove "' + name + '"?',
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonColor: '#C62839',
                    cancelButtonColor: '#6B5F80',
                    confirmButtonText: 'Yes, delete'
                }).then(function (r) {
                    if (r.isConfirmed && window.AppForm) {
                        window.AppForm.showToast('Record deleted.', 'success');
                    }
                });
            }
        });

        window.addEventListener('popstate', function () {
            var params = new URLSearchParams(window.location.search);
            switchMasterTab(params.get('tab') || 'menu-items', params.get('category'));
        });

        var root = document.getElementById('mastersHubRoot');
        var params = new URLSearchParams(window.location.search);
        if (root && !params.has('tab')) {
            var saved = localStorage.getItem('masters_active_tab');
            var current = root.getAttribute('data-active-tab');
            if (saved && saved !== current && document.getElementById('tab-btn-' + saved)) {
                switchMasterTab(saved);
            }
        }
    });

    window.AppMasters = {
        reloadActiveTab: function (tabId, highlightKey) {
            switchMasterTab(tabId, null, highlightKey);
        }
    };
})();
