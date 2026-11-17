/**
 * AppGrid: Shared DataTables integration module for RestaurantMS
 */
(function (window, $) {
    'use strict';

    window.AppGrid = {
        init: function (selector, options) {
            var $table = $(selector);
            if (!$table.length) return null;

            var defaultOptions = {
                processing: true,
                serverSide: true,
                responsive: true,
                pageLength: 10,
                lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
                language: {
                    search: "_INPUT_",
                    searchPlaceholder: "Search...",
                    lengthMenu: "Show _MENU_ entries",
                    info: "Showing _START_ to _END_ of _TOTAL_ entries",
                    infoEmpty: "No entries to show",
                    emptyTable: "No data available in table",
                    zeroRecords: "No matching records found",
                    paginate: {
                        first: "«",
                        previous: "‹",
                        next: "›",
                        last: "»"
                    }
                },
                dom: "<'row mb-3 align-items-center'<'col-sm-12 col-md-6 d-flex align-items-center gap-2'l<'app-grid-tools'>><'col-sm-12 col-md-6 d-flex justify-content-md-end gap-2'fB>>" +
                     "<'row'<'col-sm-12'tr>>" +
                     "<'row mt-3 align-items-center'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7 d-flex justify-content-md-end'p>>",
                buttons: [
                    {
                        text: '<i class="bi bi-file-earmark-excel me-1"></i> Excel',
                        className: 'btn btn-sm btn-outline-secondary',
                        action: function (e, dt, node, config) {
                            if (window.Swal) {
                                Swal.fire({
                                    icon: 'info',
                                    title: 'Exporting Excel',
                                    text: 'Generating report export...',
                                    timer: 1500,
                                    showConfirmButton: false
                                });
                            } else {
                                alert('Exporting Excel report...');
                            }
                        }
                    },
                    {
                        text: '<i class="bi bi-file-earmark-pdf me-1"></i> PDF',
                        className: 'btn btn-sm btn-outline-secondary',
                        action: function (e, dt, node, config) {
                            if (window.Swal) {
                                Swal.fire({
                                    icon: 'info',
                                    title: 'Exporting PDF',
                                    text: 'Generating PDF export...',
                                    timer: 1500,
                                    showConfirmButton: false
                                });
                            } else {
                                alert('Exporting PDF document...');
                            }
                        }
                    }
                ]
            };

            var finalOptions = $.extend(true, {}, defaultOptions, options);
            var dt = $table.DataTable(finalOptions);

            // Density toggle helper
            var $tools = $table.closest('.dataTables_wrapper').find('.app-grid-tools');
            if ($tools.length) {
                var $densityBtn = $('<button type="button" class="btn btn-sm btn-outline-secondary ms-2" title="Toggle Compact Density"><i class="bi bi-arrows-collapse"></i></button>');
                $densityBtn.on('click', function () {
                    $table.toggleClass('table-sm');
                });
                $tools.append($densityBtn);
            }

            return dt;
        }
    };
})(window, jQuery);
