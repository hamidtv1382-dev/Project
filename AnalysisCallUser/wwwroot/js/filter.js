(function ($) {
    'use strict';

    const FilterManager = {
        init: function (options = {}) {
            this.options = $.extend({
                formSelector: '#filter-form',
                resultsContainerSelector: '#search-results',
                activeFiltersContainerSelector: '#active-filters-list',
                paginationSelector: '.pagination-container',
                resetButtonSelector: '#reset-filter',
                applyButtonSelector: '#apply-filter',
                exportButtonSelector: '#export-filtered'
            }, options);

            this.$form = $(this.options.formSelector);
            this.$resultsContainer = $(this.options.resultsContainerSelector);
            this.$activeFiltersContainer = $(this.options.activeFiltersContainerSelector);
            this.$paginationContainer = $(this.options.paginationSelector);
            this.$resetButton = $(this.options.resetButtonSelector);
            this.$applyButton = $(this.options.applyButtonSelector);
            this.$exportButton = $(this.options.exportButtonSelector);

            this.currentFilters = {};
            this.bindEvents();
        },

        bindEvents: function () {
            const self = this;

            // اعمال فیلتر
            this.$applyButton.on('click', function (e) {
                e.preventDefault();
                self.applyFilters();
            });

            // بازنشانی فیلترها
            this.$resetButton.on('click', function (e) {
                e.preventDefault();
                self.resetFilters();
            });

            // خروجی داده‌های فیلتر شده
            this.$exportButton.on('click', function (e) {
                e.preventDefault();
                self.showExportModal();
            });

            // تغییر در فیلترهای کشور و شهر
            $('#origin-country, #dest-country').on('change', function () {
                const countryId = $(this).val();
                const isOrigin = $(this).attr('id') === 'origin-country';
                const citySelectId = isOrigin ? '#origin-city' : '#dest-city';

                self.updateCitySelect(citySelectId, countryId);
            });

            // حذف یک فیلتر فعال
            $(document).on('click', '.filter-tag .btn-close', function () {
                const filterName = $(this).closest('.filter-tag').data('filter-name');
                self.removeFilter(filterName);
            });
        },

        applyFilters: function () {
            const self = this;

            // جمع‌آوری مقادیر فیلترها
            this.currentFilters = this.$form.serializeArray().reduce((obj, item) => {
                if (item.value) {
                    obj[item.name] = item.value;
                }
                return obj;
            }, {});

            // نمایش لودینگ
            Utils.setButtonLoading(this.$applyButton, true);
            this.$resultsContainer.html(`
                <div class="text-center p-4">
                    <div class="spinner-border" role="status">
                        <span class="sr-only">در حال بارگذاری...</span>
                    </div>
                    <p class="mt-2">در حال جستجو...</p>
                </div>
            `);

            // ارسال درخواست AJAX
            Utils.ajax({
                url: App.apiBaseUrl + 'call/search',
                data: JSON.stringify(this.currentFilters)
            })
                .done(function (response) {
                    self.displayResults(response);
                    self.displayActiveFilters();
                    self.displayPagination(response.pagination);
                })
                .fail(function () {
                    self.$resultsContainer.html(`
                    <div class="alert alert-danger">
                        خطا در دریافت نتایج جستجو
                    </div>
                `);
                })
                .always(function () {
                    Utils.setButtonLoading(self.$applyButton, false);
                });
        },

        resetFilters: function () {
            this.$form[0].reset();
            this.currentFilters = {};
            this.$resultsContainer.html(`
                <div class="text-center p-4">
                    <i class="fas fa-search fa-3x text-muted mb-3"></i>
                    <p class="text-muted">برای جستجوی تماس‌ها، فیلترهای مورد نظر را انتخاب کرده و روی دکمه جستجو کلیک کنید.</p>
                </div>
            `);
            this.$activeFiltersContainer.empty();
            this.$paginationContainer.empty();
        },

        displayResults: function (data) {
            if (!data.items || data.items.length === 0) {
                this.$resultsContainer.html(`
                    <div class="text-center p-4">
                        <i class="fas fa-search fa-2x text-muted mb-3"></i>
                        <p class="text-muted">هیچ تماسی با فیلترهای انتخاب شده یافت نشد.</p>
                    </div>
                `);
                return;
            }

            let html = `
                <div class="table-responsive">
                    <table class="table table-striped table-hover">
                        <thead>
                            <tr>
                                <th>شماره مبدأ</th>
                                <th>شماره مقصد</th>
                                <th>تاریخ</th>
                                <th>زمان</th>
                                <th>مدت</th>
                                <th>کشور مبدأ</th>
                                <th>کشور مقصد</th>
                                <th>وضعیت</th>
                                <th>عملیات</th>
                            </tr>
                        </thead>
                        <tbody>
            `;

            data.items.forEach(item => {
                const statusBadge = item.answered ?
                    '<span class="badge bg-success">پاسخ داده شده</span>' :
                    '<span class="badge bg-danger">پاسخ داده نشده</span>';

                html += `
                    <tr>
                        <td>${item.aNumber}</td>
                        <td>${item.bNumber}</td>
                        <td>${Utils.formatDate(item.date)}</td>
                        <td>${item.time}</td>
                        <td>${item.duration}</td>
                        <td>${item.originCountry}</td>
                        <td>${item.destCountry}</td>
                        <td>${statusBadge}</td>
                        <td>
                            <a href="/call/details/${item.id}" class="btn btn-sm btn-outline-primary">
                                <i class="fas fa-eye"></i>
                            </a>
                        </td>
                    </tr>
                `;
            });

            html += `
                        </tbody>
                    </table>
                </div>
            `;

            this.$resultsContainer.html(html);
        },

        displayActiveFilters: function () {
            this.$activeFiltersContainer.empty();

            if (Object.keys(this.currentFilters).length === 0) {
                return;
            }

            const filterLabels = {
                'StartDate': 'از تاریخ',
                'EndDate': 'تا تاریخ',
                'StartTime': 'از ساعت',
                'EndTime': 'تا ساعت',
                'ANumber': 'شماره مبدأ',
                'BNumber': 'شماره مقصد',
                'OriginCountryID': 'کشور مبدأ',
                'DestCountryID': 'کشور مقصد',
                'OriginCityID': 'شهر مبدأ',
                'DestCityID': 'شهر مقصد',
                'OriginOperatorID': 'اپراتور مبدأ',
                'DestOperatorID': 'اپراتور مقصد',
                'TypeID': 'نوع تماس',
                'Answer': 'وضعیت پاسخ'
            };

            let html = '';
            for (const [key, value] of Object.entries(this.currentFilters)) {
                const label = filterLabels[key] || key;
                let displayValue = value;

                // تبدیل مقادیر خاص به متن قابل فهم
                if (key === 'Answer') {
                    displayValue = value === 'true' ? 'پاسخ داده شده' : 'پاسخ داده نشده';
                } else if (key.endsWith('CountryID') || key.endsWith('CityID') || key.endsWith('OperatorID') || key === 'TypeID') {
                    // اینجا باید متن مربوط به ID را از دیتابیس یا یک آبجکت بگیریم
                    displayValue = `ID: ${value}`;
                }

                html += `
                    <span class="filter-tag" data-filter-name="${key}">
                        ${label}: ${displayValue}
                        <button type="button" class="btn-close btn-close-sm" aria-label="Close"></button>
                    </span>
                `;
            }

            this.$activeFiltersContainer.html(html);
        },

        displayPagination: function (pagination) {
            if (!pagination) {
                this.$paginationContainer.empty();
                return;
            }

            let html = `
                <div class="pagination-container">
                    <div class="pagination-info">
                        نمایش ${(pagination.page - 1) * pagination.pageSize + 1} تا 
                        ${Math.min(pagination.page * pagination.pageSize, pagination.totalCount)} 
                        از ${Utils.formatNumber(pagination.totalCount)} مورد
                    </div>
                    
                    <nav aria-label="Page navigation">
                        <ul class="pagination justify-content-center">
            `;

            // دکمه قبلی
            if (pagination.hasPreviousPage) {
                html += `
                    <li class="page-item">
                        <a class="page-link" href="#" data-page="${pagination.page - 1}" aria-label="Previous">
                            <span aria-hidden="true">&laquo;</span>
                        </a>
                    </li>
                `;
            } else {
                html += `
                    <li class="page-item disabled">
                        <span class="page-link" aria-hidden="true">&laquo;</span>
                    </li>
                `;
            }

            // شماره صفحات
            const startPage = Math.max(1, pagination.page - 2);
            const endPage = Math.min(pagination.totalPages, pagination.page + 2);

            for (let i = startPage; i <= endPage; i++) {
                if (i === pagination.page) {
                    html += `
                        <li class="page-item active">
                            <span class="page-link">${i}</span>
                        </li>
                    `;
                } else {
                    html += `
                        <li class="page-item">
                            <a class="page-link" href="#" data-page="${i}">${i}</a>
                        </li>
                    `;
                }
            }

            // دکمه بعدی
            if (pagination.hasNextPage) {
                html += `
                    <li class="page-item">
                        <a class="page-link" href="#" data-page="${pagination.page + 1}" aria-label="Next">
                            <span aria-hidden="true">&raquo;</span>
                        </a>
                    </li>
                `;
            } else {
                html += `
                    <li class="page-item disabled">
                        <span class="page-link" aria-hidden="true">&raquo;</span>
                    </li>
                `;
            }

            html += `
                        </ul>
                    </nav>
                </div>
            `;

            this.$paginationContainer.html(html);
        },

        removeFilter: function (filterName) {
            delete this.currentFilters[filterName];

            // حذف مقدار از فرم
            const $input = this.$form.find(`[name="${filterName}"]`);
            if ($input.is(':checkbox')) {
                $input.prop('checked', false);
            } else {
                $input.val('');
            }

            // اعمال مجدد فیلترها
            this.applyFilters();
        },

        updateCitySelect: function (citySelectId, countryId) {
            const $citySelect = $(citySelectId);

            if (!countryId) {
                $citySelect.html('<option value="">انتخاب کنید</option>');
                $citySelect.prop('disabled', true);
                return;
            }

            Utils.ajax({
                url: App.apiBaseUrl + 'location/cities/' + countryId,
                type: 'GET'
            })
                .done(function (cities) {
                    let html = '<option value="">انتخاب کنید</option>';
                    cities.forEach(city => {
                        html += `<option value="${city.id}">${city.name}</option>`;
                    });
                    $citySelect.html(html);
                    $citySelect.prop('disabled', false);
                })
                .fail(function () {
                    $citySelect.html('<option value="">خطا در بارگذاری</option>');
                    $citySelect.prop('disabled', true);
                });
        },

        showExportModal: function () {
            $('#exportModal').modal('show');
        }
    };

    // ========== Initialization ==========
    $(document).ready(function () {
        // فقط در صفحاتی که فرم فیلتر وجود دارد، فیلتر منیجر را مقداردهی اولیه کن
        if ($('#filter-form').length) {
            FilterManager.init();
        }

        // مدیریت رویداد کلیک روی پیجینیشن
        $(document).on('click', '.pagination a[data-page]', function (e) {
            e.preventDefault();
            const page = $(this).data('page');
            FilterManager.currentFilters.Page = page;
            FilterManager.applyFilters();
        });
    });

    // ========== Expose ==========
    window.FilterManager = FilterManager;

})(jQuery);