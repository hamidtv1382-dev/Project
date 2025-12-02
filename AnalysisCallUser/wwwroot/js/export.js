(function ($) {
    'use strict';

    const ExportManager = {
        init: function (options = {}) {
            this.options = $.extend({
                modalSelector: '#exportModal',
                formSelector: '#export-form',
                buttonSelector: '#export-btn',
                typeSelector: '#export-type',
                countSelector: '#export-count',
                columnsSelector: '.export-columns-container input[type="checkbox"]'
            }, options);

            this.$modal = $(this.options.modalSelector);
            this.$form = $(this.options.formSelector);
            this.$button = $(this.options.buttonSelector);
            this.$typeSelect = $(this.options.typeSelector);
            this.$countSelect = $(this.options.countSelector);
            this.$columnCheckboxes = $(this.options.columnsSelector);

            this.bindEvents();
        },

        bindEvents: function () {
            const self = this;

            // رویداد کلیک روی دکمه خروجی
            this.$button.on('click', function (e) {
                e.preventDefault();
                self.performExport();
            });

            // تغییر نوع خروجی
            this.$typeSelect.on('change', function () {
                self.updateColumnVisibility();
            });
        },

        performExport: function () {
            const self = this;
            const exportType = this.$typeSelect.val();
            const exportCount = this.$countSelect.val();

            // دریافت ستون‌های انتخاب شده
            const selectedColumns = [];
            this.$columnCheckboxes.filter(':checked').each(function () {
                selectedColumns.push($(this).val());
            });

            if (selectedColumns.length === 0) {
                Utils.showToast('لطفاً حداقل یک ستون را برای خروجی انتخاب کنید.', 'warning');
                return;
            }

            // نمایش لودینگ
            Utils.setButtonLoading(this.$button, true);

            // دریافت فیلترهای فعلی از FilterManager
            const filterData = window.FilterManager ? window.FilterManager.currentFilters : {};

            // ارسال درخواست به سرور
            Utils.ajax({
                url: App.apiBaseUrl + 'export/request',
                data: JSON.stringify({
                    exportType: exportType,
                    exportCount: exportCount,
                    selectedColumns: selectedColumns,
                    filter: filterData
                })
            })
                .done(function (response) {
                    if (response.success) {
                        // شروع دانلود فایل
                        self.downloadFile(response.downloadUrl, response.fileName);
                        self.$modal.modal('hide');
                        Utils.showToast('خروجی با موفقیت آماده و دانلود شد.', 'success');
                    } else {
                        Utils.showToast(response.message || 'خطا در آماده‌سازی خروجی', 'danger');
                    }
                })
                .fail(function () {
                    Utils.showToast('خطا در ارتباط با سرور', 'danger');
                })
                .always(function () {
                    Utils.setButtonLoading(self.$button, false);
                });
        },

        downloadFile: function (url, fileName) {
            const link = document.createElement('a');
            link.href = url;
            link.download = fileName;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        },

        updateColumnVisibility: function () {
            const exportType = this.$typeSelect.val();

            // برای فرمت‌های خاص، ممکن است برخی ستون‌ها نامناسب باشند
            // اینجا می‌توان منطق خاصی برای هر نوع خروجی پیاده‌سازی کرد
            if (exportType === 'json') {
                // در JSON معمولاً همه ستون‌ها مناسب هستند
                this.$columnCheckboxes.prop('disabled', false);
            } else {
                // در CSV و Excel ممکن است بخواهیم برخی ستون‌ها را غیرفعال کنیم
                this.$columnCheckboxes.prop('disabled', false);
            }
        },

        // خروجی سریع از داده‌های موجود در جدول (بدون ارسال به سرور)
        quickExport: function (tableSelector, filename, type = 'csv') {
            const $table = $(tableSelector);
            if (!$table.length) {
                Utils.showToast('جدولی برای خروجی یافت نشد.', 'warning');
                return;
            }

            let data = this.extractTableData($table);

            if (type === 'csv') {
                this.downloadCSV(data, filename);
            } else if (type === 'json') {
                this.downloadJSON(data, filename);
            }
        },

        extractTableData: function ($table) {
            const headers = [];
            const rows = [];

            // استخراج هدرها
            $table.find('thead th').each(function () {
                headers.push($(this).text().trim());
            });

            // استخراج ردیف‌ها
            $table.find('tbody tr').each(function () {
                const row = [];
                $(this).find('td').each(function () {
                    row.push($(this).text().trim());
                });
                rows.push(row);
            });

            return { headers, rows };
        },

        downloadCSV: function (data, filename) {
            let csvContent = data.headers.join(',') + '\n';

            data.rows.forEach(function (row) {
                csvContent += row.map(cell => `"${cell}"`).join(',') + '\n';
            });

            const blob = new Blob(['\ufeff' + csvContent], { type: 'text/csv;charset=utf-8;' });
            this.downloadFile(URL.createObjectURL(blob), filename + '.csv');
        },

        downloadJSON: function (data, filename) {
            const jsonData = data.rows.map(row => {
                const obj = {};
                data.headers.forEach((header, index) => {
                    obj[header] = row[index];
                });
                return obj;
            });

            const blob = new Blob([JSON.stringify(jsonData, null, 2)], { type: 'application/json' });
            this.downloadFile(URL.createObjectURL(blob), filename + '.json');
        }
    };

    // ========== Initialization ==========
    $(document).ready(function () {
        // فقط در صفحاتی که مودال خروجی وجود دارد، خروجی منیجر را مقداردهی اولیه کن
        if ($('#exportModal').length) {
            ExportManager.init();
        }
    });

    // ========== Expose ==========
    window.ExportManager = ExportManager;

})(jQuery);