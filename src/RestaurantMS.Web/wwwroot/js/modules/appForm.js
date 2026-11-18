(function () {
    'use strict';

    var activeDirtyForm = null;

    function showToast(message, icon) {
        if (!message) return;
        if (window.Swal) {
            window.Swal.fire({
                toast: true,
                position: 'top-end',
                icon: icon || 'success',
                title: message,
                showConfirmButton: false,
                timer: 3200,
                timerProgressBar: true
            });
        }
    }

    function highlightMatchingRow(needle) {
        if (!needle) return;
        var lower = needle.toLowerCase();
        var rows = document.querySelectorAll('table tbody tr');
        for (var i = 0; i < rows.length; i++) {
            var row = rows[i];
            var key = (row.getAttribute('data-row-key') || row.textContent || '').toLowerCase();
            if (key.indexOf(lower) !== -1) {
                row.classList.add('app-row-highlight');
                row.scrollIntoView({ behavior: 'smooth', block: 'center' });
                break;
            }
        }
    }

    function initGlobalToastsAndHighlight() {
        var hook = document.getElementById('appToastHook');
        if (!hook) return;
        var ok = hook.getAttribute('data-toast-success');
        var err = hook.getAttribute('data-toast-error');
        var highlight = hook.getAttribute('data-highlight-row');

        if (ok) showToast(ok, 'success');
        else if (err) showToast(err, 'error');

        if (highlight) {
            setTimeout(function () { highlightMatchingRow(highlight); }, 180);
        }
    }

    function getFieldLabel(field) {
        var form = field.form || document;
        var label = field.id ? form.querySelector('label[for="' + field.id + '"]') : null;
        if (label) return label.textContent.replace('*', '').trim();
        return (field.name || 'This field').replace(/([A-Z])/g, ' $1').trim();
    }

    function getValidationMessage(field) {
        if (field.validity.valid) return '';
        var label = getFieldLabel(field);
        if (field.validity.valueMissing) return field.getAttribute('data-msg-required') || (label + ' is required.');
        if (field.validity.typeMismatch) {
            if (field.type === 'email') return 'Enter a valid email address (e.g. name@example.com).';
            if (field.type === 'tel') return 'Enter a valid 10-digit mobile number starting with 6, 7, 8, or 9.';
            return 'Enter a valid ' + (field.type || 'value') + ' for ' + label + '.';
        }
        if (field.validity.patternMismatch) return field.getAttribute('data-msg-pattern') || (label + ' does not match the required format.');
        if (field.validity.rangeUnderflow) return label + ' must be at least ' + field.min + '.';
        if (field.validity.rangeOverflow) return label + ' cannot exceed ' + field.max + '.';
        if (field.validity.stepMismatch) return label + ' has too many decimal places or an unsupported step.';
        if (field.validity.tooShort) return label + ' must be at least ' + field.minLength + ' characters.';
        if (field.validity.tooLong) return label + ' must not exceed ' + field.maxLength + ' characters.';
        return field.validationMessage || ('Please check ' + label + '.');
    }

    function applyServerFieldErrors(form, errorsMap) {
        if (!form || !errorsMap) return;
        var firstErrField = null;
        Object.keys(errorsMap).forEach(function (fieldName) {
            var field = form.querySelector('[name="' + fieldName + '"]') || form.querySelector('#' + fieldName);
            if (field) {
                field.classList.add('is-invalid');
                var container = field.closest('.app-form-field') || field.parentElement;
                var errSpan = container ? container.querySelector('.js-field-error, [data-valmsg-for]') : null;
                if (errSpan) errSpan.textContent = errorsMap[fieldName];
                if (!firstErrField) firstErrField = field;
            }
        });
        if (firstErrField) {
            firstErrField.focus();
            firstErrField.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }
    }

    function validateField(field) {
        if (!field || field.disabled || field.type === 'hidden' || field.type === 'submit' || field.type === 'button') {
            return true;
        }
        var msg = getValidationMessage(field);
        var container = field.closest('.app-form-field') || field.parentElement;
        var errSpan = container ? container.querySelector('.js-field-error, [data-valmsg-for]') : null;

        if (msg) {
            field.classList.add('is-invalid');
            if (errSpan) errSpan.textContent = msg;
            return false;
        } else {
            field.classList.remove('is-invalid');
            if (errSpan) errSpan.textContent = '';
            return true;
        }
    }

    function validateForm(form) {
        var controls = form.querySelectorAll('input, select, textarea');
        var firstInvalid = null;
        var errors = [];

        controls.forEach(function (el) {
            if (!validateField(el)) {
                if (!firstInvalid) firstInvalid = el;
                var label = form.querySelector('label[for="' + el.id + '"]');
                var labelText = label ? label.textContent.replace('*', '').trim() : (el.name || 'Field');
                errors.push(labelText + ': ' + getValidationMessage(el));
            }
        });

        var summary = form.querySelector('.js-form-error-summary');
        if (summary) {
            if (errors.length > 0) {
                summary.innerHTML = '<div class="fw-bold mb-1"><i class="bi bi-exclamation-triangle-fill me-1"></i>Please fix the following before saving:</div><ul class="mb-0 ps-3">' +
                    errors.map(function (e) { return '<li>' + e + '</li>'; }).join('') + '</ul>';
                summary.classList.remove('d-none');
            } else {
                summary.classList.add('d-none');
            }
        }

        if (firstInvalid) {
            firstInvalid.focus();
            firstInvalid.scrollIntoView({ behavior: 'smooth', block: 'center' });
            return false;
        }
        return true;
    }

    function initSteppers(scope) {
        scope.querySelectorAll('.app-stepper-group').forEach(function (grp) {
            var input = grp.querySelector('input[type="number"]');
            var dec = grp.querySelector('.js-stepper-dec');
            var inc = grp.querySelector('.js-stepper-inc');
            if (!input || grp.dataset.stepperBound) return;
            grp.dataset.stepperBound = '1';

            function adjust(delta) {
                var step = parseFloat(input.step || '1');
                var min = input.min !== '' ? parseFloat(input.min) : -Infinity;
                var max = input.max !== '' ? parseFloat(input.max) : Infinity;
                var cur = parseFloat(input.value || '0');
                var next = Math.min(max, Math.max(min, cur + delta * step));
                input.value = next;
                input.dispatchEvent(new Event('input', { bubbles: true }));
                input.dispatchEvent(new Event('change', { bubbles: true }));
            }

            if (dec) dec.addEventListener('click', function () { adjust(-1); });
            if (inc) inc.addEventListener('click', function () { adjust(1); });
        });
    }

    function initImageUploaders(scope) {
        scope.querySelectorAll('.js-image-uploader').forEach(function (card) {
            if (card.dataset.uploaderBound) return;
            card.dataset.uploaderBound = '1';

            var dropzone = card.querySelector('.js-dropzone');
            var fileInput = card.querySelector('.js-image-file-input');
            var previews = card.querySelector('.js-image-previews');
            var mainIdxInput = card.querySelector('.js-main-image-index');
            var maxFiles = parseInt(card.getAttribute('data-max-images') || '3', 10);
            var maxSizeBytes = parseInt(card.getAttribute('data-max-size') || '2', 10) * 1024 * 1024;

            function refreshMainBadges() {
                var items = previews.querySelectorAll('.js-preview-item');
                var mainSrc = '';
                var foundMain = false;

                items.forEach(function (item, idx) {
                    item.setAttribute('data-index', idx);
                    var isMain = item.classList.contains('is-main');
                    if (isMain) {
                        foundMain = true;
                        if (mainIdxInput) mainIdxInput.value = idx;
                        var img = item.querySelector('.js-preview-img');
                        if (img) mainSrc = img.src;
                    }
                });

                if (!foundMain && items.length > 0) {
                    items[0].classList.add('is-main');
                    if (mainIdxInput) mainIdxInput.value = '0';
                    var firstImg = items[0].querySelector('.js-preview-img');
                    if (firstImg) mainSrc = firstImg.src;
                }

                items.forEach(function (item) {
                    var btn = item.querySelector('.js-set-main-btn');
                    var isMain = item.classList.contains('is-main');
                    if (btn) {
                        btn.className = 'btn btn-sm position-absolute bottom-0 start-0 m-2 py-0 px-2 rounded-pill small fw-semibold js-set-main-btn ' +
                            (isMain ? 'btn-saffron text-dark' : 'btn-light border');
                        btn.innerHTML = '<i class="bi ' + (isMain ? 'bi-star-fill' : 'bi-star') + ' me-1"></i>' + (isMain ? 'Main' : 'Set Main');
                    }
                });

                document.dispatchEvent(new CustomEvent('app:main-image-changed', { detail: { src: mainSrc } }));
            }

            previews.addEventListener('click', function (e) {
                var removeBtn = e.target.closest('.js-remove-img-btn');
                if (removeBtn) {
                    var item = removeBtn.closest('.js-preview-item');
                    if (item) item.remove();
                    refreshMainBadges();
                    return;
                }
                var mainBtn = e.target.closest('.js-set-main-btn');
                if (mainBtn) {
                    previews.querySelectorAll('.js-preview-item').forEach(function (el) { el.classList.remove('is-main'); });
                    mainBtn.closest('.js-preview-item').classList.add('is-main');
                    refreshMainBadges();
                }
            });

            function handleFiles(fileList) {
                var files = Array.prototype.slice.call(fileList || []);
                var existing = previews.querySelectorAll('.js-preview-item').length;
                if (existing + files.length > maxFiles) {
                    showToast('Maximum ' + maxFiles + ' images allowed.', 'warning');
                    files = files.slice(0, Math.max(0, maxFiles - existing));
                }

                files.forEach(function (file) {
                    if (['image/jpeg', 'image/png', 'image/webp'].indexOf(file.type) === -1) {
                        showToast(file.name + ' must be JPG, PNG, or WEBP.', 'error');
                        return;
                    }
                    if (file.size > maxSizeBytes) {
                        showToast(file.name + ' exceeds ' + (maxSizeBytes / (1024 * 1024)) + ' MB.', 'error');
                        return;
                    }
                    var reader = new FileReader();
                    reader.onload = function (ev) {
                        var isFirst = previews.querySelectorAll('.js-preview-item').length === 0;
                        var col = document.createElement('div');
                        col.className = 'col-4 position-relative preview-item js-preview-item' + (isFirst ? ' is-main' : '');
                        col.innerHTML =
                            '<img src="' + ev.target.result + '" class="img-thumbnail rounded-3 w-100 js-preview-img" style="height: 100px; object-fit: cover;" alt="Preview" />' +
                            '<button type="button" class="btn btn-sm btn-light border position-absolute bottom-0 start-0 m-2 py-0 px-2 rounded-pill small fw-semibold js-set-main-btn" style="font-size: 0.7rem;"><i class="bi bi-star me-1"></i>Set Main</button>' +
                            '<button type="button" class="btn btn-sm btn-danger position-absolute top-0 end-0 m-2 rounded-circle p-1 js-remove-img-btn" style="line-height: 1; width: 24px; height: 24px;"><i class="bi bi-x"></i></button>';
                        previews.appendChild(col);
                        refreshMainBadges();
                    };
                    reader.readAsDataURL(file);
                });
            }

            if (fileInput) {
                fileInput.addEventListener('change', function () { handleFiles(fileInput.files); });
            }

            if (dropzone) {
                ['dragenter', 'dragover'].forEach(function (evt) {
                    dropzone.addEventListener(evt, function (e) { e.preventDefault(); dropzone.classList.add('is-dragover'); });
                });
                ['dragleave', 'drop'].forEach(function (evt) {
                    dropzone.addEventListener(evt, function (e) { e.preventDefault(); dropzone.classList.remove('is-dragover'); });
                });
                dropzone.addEventListener('drop', function (e) {
                    if (e.dataTransfer && e.dataTransfer.files) handleFiles(e.dataTransfer.files);
                });
            }
        });
    }

    function enhanceForm(form) {
        if (!form || form.dataset.appFormBound) return;
        form.dataset.appFormBound = '1';

        var isDirty = false;
        var isSubmitting = false;
        var dirtyBadge = form.querySelector('.js-dirty-badge');

        function markDirty() {
            if (!isDirty) {
                isDirty = true;
                activeDirtyForm = form;
                if (dirtyBadge) dirtyBadge.classList.remove('d-none');
            }
        }

        function clearDirty() {
            isDirty = false;
            if (activeDirtyForm === form) activeDirtyForm = null;
            if (dirtyBadge) dirtyBadge.classList.add('d-none');
        }

        // Inline blur validation & dirty tracking
        form.addEventListener('input', markDirty);
        form.addEventListener('change', function (e) {
            markDirty();
            validateField(e.target);
        });
        form.addEventListener('focusout', function (e) {
            if (e.target && (e.target.tagName === 'INPUT' || e.target.tagName === 'SELECT' || e.target.tagName === 'TEXTAREA')) {
                validateField(e.target);
            }
        });

        // Enter key moves to next field (not inside textarea or buttons)
        form.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' && e.target.tagName !== 'TEXTAREA' && e.target.type !== 'submit' && e.target.type !== 'button') {
                e.preventDefault();
                var focusables = Array.prototype.slice.call(
                    form.querySelectorAll('input:not([type="hidden"]):not([disabled]):not([readonly]), select:not([disabled]), textarea:not([disabled])')
                ).filter(function (el) { return el.offsetParent !== null; });
                var idx = focusables.indexOf(e.target);
                if (idx >= 0 && idx < focusables.length - 1) {
                    focusables[idx + 1].focus();
                } else {
                    var saveBtn = form.querySelector('.js-submit-save, button[type="submit"]');
                    if (saveBtn) saveBtn.focus();
                }
            }
        });

        // Submit validation + double-submit protection
        form.addEventListener('submit', function (e) {
            if (isSubmitting) {
                e.preventDefault();
                return;
            }
            if (!validateForm(form)) {
                e.preventDefault();
                return;
            }
            isSubmitting = true;
            clearDirty();
            if (form.getAttribute('data-ajax-drawer') !== 'true') {
                form.querySelectorAll('button[type="submit"]').forEach(function (btn) {
                    btn.disabled = true;
                });
            }
        });

        // Unsaved changes guard on Cancel link
        var cancelBtn = form.querySelector('.js-form-cancel');
        if (cancelBtn && cancelBtn.tagName === 'A') {
            cancelBtn.addEventListener('click', function (e) {
                if (!isDirty || isSubmitting) return;
                e.preventDefault();
                var href = cancelBtn.getAttribute('href');
                if (window.Swal) {
                    window.Swal.fire({
                        title: 'Discard unsaved changes?',
                        text: 'You have unsaved edits in this form.',
                        icon: 'warning',
                        showCancelButton: true,
                        confirmButtonColor: '#C62839',
                        cancelButtonColor: '#6B5F80',
                        confirmButtonText: 'Discard',
                        cancelButtonText: 'Keep Editing'
                    }).then(function (r) {
                        if (r.isConfirmed) {
                            clearDirty();
                            window.location.href = href;
                        }
                    });
                } else {
                    clearDirty();
                    window.location.href = href;
                }
            });
        }

        // Highlight & focus first server-side invalid field on page load (preserve entered data)
        form.querySelectorAll('[data-valmsg-for]').forEach(function (span) {
            if (span.textContent && span.textContent.trim().length > 0) {
                var targetName = span.getAttribute('data-valmsg-for');
                var targetEl = targetName ? form.querySelector('[name="' + targetName + '"]') : null;
                if (targetEl) targetEl.classList.add('is-invalid');
            }
        });
        var firstServerInvalid = form.querySelector('.input-validation-error, .is-invalid');
        if (firstServerInvalid) {
            firstServerInvalid.classList.add('is-invalid');
            firstServerInvalid.focus();
            firstServerInvalid.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }

        initSteppers(form);
        initImageUploaders(form);
    }

    function openDrawer(url, title, subtitle, icon, onSuccess) {
        var drawerEl = document.getElementById('appGlobalDrawer');
        if (!drawerEl || !window.bootstrap) return;

        var titleEl = drawerEl.querySelector('.js-drawer-title');
        var subEl = drawerEl.querySelector('.js-drawer-subtitle');
        var iconEl = drawerEl.querySelector('.js-drawer-icon');
        var bodyEl = drawerEl.querySelector('.js-drawer-body');

        if (titleEl && title) titleEl.textContent = title;
        if (subEl) subEl.textContent = subtitle || '';
        if (iconEl && icon) iconEl.className = 'bi ' + icon + ' text-saffron fs-5 js-drawer-icon';
        bodyEl.innerHTML = '<div class="p-4 text-center text-muted small"><div class="spinner-border spinner-border-sm text-plum me-2"></div>Loading...</div>';

        var instance = window.bootstrap.Offcanvas.getOrCreateInstance(drawerEl);
        instance.show();

        fetch(url, { headers: { 'X-Requested-With': 'XMLHttpRequest' } })
            .then(function (r) { return r.text(); })
            .then(function (html) {
                bodyEl.innerHTML = html;
                var form = bodyEl.querySelector('form');
                if (form) {
                    enhanceForm(form);
                    var firstInput = form.querySelector('input:not([type="hidden"]), select, textarea');
                    if (firstInput) firstInput.focus();

                    form.addEventListener('submit', function (e) {
                        if (e.defaultPrevented) return;
                        if (form.getAttribute('data-ajax-drawer') !== 'true') return;
                        e.preventDefault();

                        var submitter = e.submitter;
                        var submitAction = submitter ? (submitter.value || 'save') : 'save';
                        var formData = new FormData(form);
                        formData.set('submitAction', submitAction);

                        fetch(form.action, {
                            method: 'POST',
                            headers: { 'X-Requested-With': 'XMLHttpRequest' },
                            body: formData
                        })
                        .then(function (resp) { return resp.json(); })
                        .then(function (res) {
                            if (res.isSuccess) {
                                showToast(res.message || 'Saved successfully!', 'success');
                                if (typeof onSuccess === 'function') {
                                    onSuccess(res);
                                }
                                if (submitAction === 'saveAndNew') {
                                    form.reset();
                                    var first = form.querySelector('input:not([type="hidden"]), select');
                                    if (first) first.focus();
                                } else {
                                    instance.hide();
                                }
                            } else {
                                if (res.errors) {
                                    applyServerFieldErrors(form, res.errors);
                                }
                                var summary = form.querySelector('.js-form-error-summary');
                                if (summary) {
                                    summary.textContent = res.error || 'Please correct the highlighted fields and try again.';
                                    summary.classList.remove('d-none');
                                }
                                showToast(res.error || 'Please fix the highlighted validation errors.', 'error');
                            }
                        })
                        .catch(function () {
                            showToast('Network error — your entered form data is preserved. Check your connection and try again.', 'error');
                        });
                    });
                }
            });
    }

    // Global Ctrl+S handler for active form (drawer or full page)
    document.addEventListener('keydown', function (e) {
        if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 's') {
            var openDrawerForm = document.querySelector('.offcanvas.show form[data-app-form="true"]');
            var pageForm = document.querySelector('form[data-app-form="true"]');
            var target = openDrawerForm || pageForm;
            if (target) {
                e.preventDefault();
                var btn = target.querySelector('.js-submit-save, button[type="submit"]');
                if (btn) btn.click();
                else target.requestSubmit();
            }
        }
    });

    // Browser beforeunload warning when form is dirty
    window.addEventListener('beforeunload', function (e) {
        if (activeDirtyForm) {
            e.preventDefault();
            e.returnValue = '';
        }
    });

    // Global Offline / Online network failure toasts
    window.addEventListener('offline', function () {
        showToast('You are offline. Unsaved changes are kept in the form until your connection returns.', 'warning');
    });
    window.addEventListener('online', function () {
        showToast('Network connection restored. You are back online.', 'success');
    });

    document.addEventListener('DOMContentLoaded', function () {
        initGlobalToastsAndHighlight();
        document.querySelectorAll('form[data-app-form="true"]').forEach(enhanceForm);
        initSteppers(document);
        initImageUploaders(document);

        document.addEventListener('click', function (e) {
            var trigger = e.target.closest('[data-drawer-url]');
            if (!trigger) return;
            e.preventDefault();
            openDrawer(
                trigger.getAttribute('data-drawer-url'),
                trigger.getAttribute('data-drawer-title'),
                trigger.getAttribute('data-drawer-subtitle'),
                trigger.getAttribute('data-drawer-icon'),
                function (res) {
                    if (res.highlightKey) {
                        sessionStorage.setItem('app_highlight_next', res.highlightKey);
                    }
                    if (window.AppMasters && typeof window.AppMasters.reloadActiveTab === 'function' && res.tab) {
                        window.AppMasters.reloadActiveTab(res.tab, res.highlightKey);
                    } else if (res.redirectUrl) {
                        window.location.href = res.redirectUrl;
                    } else {
                        window.location.reload();
                    }
                }
            );
        });

        var pendingHighlight = sessionStorage.getItem('app_highlight_next');
        if (pendingHighlight) {
            sessionStorage.removeItem('app_highlight_next');
            setTimeout(function () { highlightMatchingRow(pendingHighlight); }, 250);
        }
    });

    window.AppForm = {
        enhance: enhanceForm,
        openDrawer: openDrawer,
        showToast: showToast,
        highlightRow: highlightMatchingRow
    };
})();
