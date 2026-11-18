(function () {
    'use strict';

    function initMenuItemForm() {
        var form = document.getElementById('itemForm');
        if (!form) return;

        var nameEnInput = document.getElementById('NameEn');
        var nameMrInput = document.getElementById('NameMr');
        var codeInput = document.getElementById('ItemCode');
        var catSelect = document.getElementById('CategoryId');
        var priceInput = document.getElementById('Price');
        var taxSelect = document.getElementById('TaxRate');
        var taxIncToggle = document.getElementById('IsTaxInclusive');
        var prepInput = document.getElementById('PrepTimeMinutes');
        var availToggle = document.getElementById('IsAvailable');
        var descInput = document.getElementById('Description');

        // Live price & preview elements
        var basePriceOut = document.getElementById('calcBasePrice');
        var cgstOut = document.getElementById('calcCgst');
        var sgstOut = document.getElementById('calcSgst');
        var finalPriceOut = document.getElementById('calcFinalPrice');

        var prevNameEn = document.getElementById('prevNameEn');
        var prevNameMr = document.getElementById('prevNameMr');
        var prevCode = document.getElementById('prevCode');
        var prevCat = document.getElementById('prevCategory');
        var prevPrice = document.getElementById('prevPrice');
        var prevPrep = document.getElementById('prevPrep');
        var prevDesc = document.getElementById('prevDesc');
        var prevDietIcon = document.getElementById('prevDietIcon');
        var prevAvailBadge = document.getElementById('prevAvailBadge');
        var prevTagsWrap = document.getElementById('prevTags');
        var prevVariantsWrap = document.getElementById('prevVariants');
        var prevImg = document.getElementById('prevDishImage');
        var prevImgPlaceholder = document.getElementById('prevDishImagePlaceholder');

        function recalculatePricingAndPreview() {
            var rawPrice = parseFloat((priceInput && priceInput.value) || '0') || 0;
            var taxRate = parseFloat((taxSelect && taxSelect.value) || '0') || 0;
            var isInclusive = !!(taxIncToggle && taxIncToggle.checked);

            var basePrice = isInclusive && taxRate > 0 ? rawPrice / (1 + taxRate / 100) : rawPrice;
            var totalTax = isInclusive ? rawPrice - basePrice : basePrice * (taxRate / 100);
            var halfTax = totalTax / 2;
            var finalPrice = isInclusive ? rawPrice : basePrice + totalTax;

            if (basePriceOut) basePriceOut.textContent = '₹' + basePrice.toFixed(2);
            if (cgstOut) cgstOut.textContent = '₹' + halfTax.toFixed(2);
            if (sgstOut) sgstOut.textContent = '₹' + halfTax.toFixed(2);
            if (finalPriceOut) finalPriceOut.textContent = '₹' + finalPrice.toFixed(2);

            if (prevNameEn) prevNameEn.textContent = (nameEnInput && nameEnInput.value.trim()) || 'Paneer Butter Masala';
            if (prevNameMr) prevNameMr.textContent = (nameMrInput && nameMrInput.value.trim()) || 'पनीर बटर मसाला';
            if (prevCode) prevCode.textContent = (codeInput && codeInput.value.trim()) || '101';
            if (prevCat && catSelect) {
                var opt = catSelect.options[catSelect.selectedIndex];
                prevCat.textContent = opt ? opt.text : 'Main Course';
            }
            if (prevPrice) prevPrice.textContent = '₹' + finalPrice.toFixed(0);
            if (prevPrep) prevPrep.textContent = ((prepInput && prepInput.value) || '15') + ' min';
            if (prevDesc) prevDesc.textContent = (descInput && descInput.value.trim()) || 'Rich creamy tomato gravy with soft cottage cheese cubes.';

            var diet = form.querySelector('input[name="DietaryType"]:checked');
            var dietVal = diet ? diet.value : 'Veg';
            var isVegHidden = document.getElementById('IsVegetarianHidden');
            if (isVegHidden) isVegHidden.value = dietVal === 'Veg' ? 'true' : 'false';

            if (prevDietIcon) {
                prevDietIcon.className = 'food-type-icon ' + (dietVal === 'NonVeg' ? 'nonveg' : dietVal === 'Egg' ? 'egg' : 'veg');
            }

            if (prevAvailBadge && availToggle) {
                if (availToggle.checked) {
                    prevAvailBadge.className = 'badge bg-teal text-white';
                    prevAvailBadge.textContent = 'Available';
                } else {
                    prevAvailBadge.className = 'badge bg-secondary text-white';
                    prevAvailBadge.textContent = 'Unavailable';
                }
            }

            if (prevTagsWrap) {
                var selectedTags = Array.prototype.slice.call(form.querySelectorAll('input[name="Tags"]:checked')).map(function (c) { return c.value; });
                prevTagsWrap.innerHTML = selectedTags.map(function (t) {
                    return '<span class="badge bg-saffron-subtle text-dark border border-warning" style="font-size:0.7rem;">' + t + '</span>';
                }).join(' ');
            }

            if (prevVariantsWrap) {
                var variantRows = form.querySelectorAll('#variantsTableBody tr');
                var pills = [];
                variantRows.forEach(function (tr) {
                    var vName = (tr.querySelector('.js-var-name')?.value || '').trim();
                    var vPrice = tr.querySelector('.js-var-price')?.value || '';
                    if (vName) {
                        pills.push('<span class="badge bg-light text-ink border" style="font-size:0.72rem;">' + vName + (vPrice ? ' · ₹' + vPrice : '') + '</span>');
                    }
                });
                prevVariantsWrap.innerHTML = pills.join(' ');
            }
        }

        // Dynamic Variants table
        var variantsBody = document.getElementById('variantsTableBody');
        var addVariantBtn = document.getElementById('addVariantRowBtn');
        if (addVariantBtn && variantsBody) {
            addVariantBtn.addEventListener('click', function () {
                var idx = variantsBody.querySelectorAll('tr').length;
                var tr = document.createElement('tr');
                tr.innerHTML =
                    '<td><input type="text" name="Variants[' + idx + '].Name" class="form-control form-control-sm js-var-name" placeholder="e.g. Family Pack" /></td>' +
                    '<td><div class="input-group input-group-sm"><input type="number" name="Variants[' + idx + '].Price" class="form-control text-end js-var-price" step="1" min="0" placeholder="0" /><span class="input-group-text">₹</span></div></td>' +
                    '<td class="text-center"><input type="radio" name="DefaultVariantIndex" value="' + idx + '" class="form-check-input" /></td>' +
                    '<td class="text-end"><button type="button" class="btn btn-sm btn-outline-danger js-del-row"><i class="bi bi-trash"></i></button></td>';
                variantsBody.appendChild(tr);
                tr.querySelector('.js-var-name').focus();
                recalculatePricingAndPreview();
            });
        }

        // Dynamic Add-ons table
        var addonsBody = document.getElementById('addonsTableBody');
        var addAddonBtn = document.getElementById('addAddonRowBtn');
        if (addAddonBtn && addonsBody) {
            addAddonBtn.addEventListener('click', function () {
                var idx = addonsBody.querySelectorAll('tr').length;
                var tr = document.createElement('tr');
                tr.innerHTML =
                    '<td><input type="text" name="Addons[' + idx + '].Name" class="form-control form-control-sm" placeholder="e.g. Roasted Papad" /></td>' +
                    '<td><div class="input-group input-group-sm"><input type="number" name="Addons[' + idx + '].Price" class="form-control text-end" step="1" min="0" placeholder="0" /><span class="input-group-text">₹</span></div></td>' +
                    '<td class="text-end"><button type="button" class="btn btn-sm btn-outline-danger js-del-row"><i class="bi bi-trash"></i></button></td>';
                addonsBody.appendChild(tr);
                tr.querySelector('input').focus();
            });
        }

        form.addEventListener('click', function (e) {
            var del = e.target.closest('.js-del-row');
            if (del) {
                var tr = del.closest('tr');
                if (tr) tr.remove();
                recalculatePricingAndPreview();
            }
        });

        // Quick Add Category modal save without leaving Menu Item form
        var quickCatBtn = document.getElementById('quickAddCategorySaveBtn');
        if (quickCatBtn && catSelect) {
            quickCatBtn.addEventListener('click', function () {
                var nameEn = document.getElementById('quickCatNameEn');
                var nameMr = document.getElementById('quickCatNameMr');
                if (!nameEn || !nameEn.value.trim()) {
                    if (window.AppForm) window.AppForm.showToast('Please enter category name.', 'warning');
                    return;
                }
                var url = quickCatBtn.getAttribute('data-url');
                var token = form.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
                var body = new URLSearchParams();
                body.append('__RequestVerificationToken', token);
                body.append('nameEn', nameEn.value.trim());
                body.append('nameMr', (nameMr && nameMr.value.trim()) || nameEn.value.trim());

                fetch(url, { method: 'POST', headers: { 'X-Requested-With': 'XMLHttpRequest' }, body: body })
                    .then(function (r) { return r.json(); })
                    .then(function (res) {
                        if (res.isSuccess && res.value) {
                            var opt = document.createElement('option');
                            opt.value = res.value.name;
                            opt.textContent = res.value.name;
                            opt.selected = true;
                            catSelect.appendChild(opt);
                            nameEn.value = '';
                            if (nameMr) nameMr.value = '';
                            var modalEl = document.getElementById('quickAddCategoryModal');
                            if (modalEl && window.bootstrap) window.bootstrap.Modal.getOrCreateInstance(modalEl).hide();
                            recalculatePricingAndPreview();
                            if (window.AppForm) window.AppForm.showToast('Category added and selected!', 'success');
                        }
                    });
            });
        }

        // Sync main dish image into live preview card
        document.addEventListener('app:main-image-changed', function (e) {
            var src = e.detail && e.detail.src;
            if (prevImg && prevImgPlaceholder) {
                if (src) {
                    prevImg.src = src;
                    prevImg.classList.remove('d-none');
                    prevImgPlaceholder.classList.add('d-none');
                } else {
                    prevImg.classList.add('d-none');
                    prevImgPlaceholder.classList.remove('d-none');
                }
            }
        });

        form.addEventListener('input', recalculatePricingAndPreview);
        form.addEventListener('change', recalculatePricingAndPreview);
        recalculatePricingAndPreview();
    }

    document.addEventListener('DOMContentLoaded', initMenuItemForm);
})();
