(function ($) {
    'use strict';

    const PaginationManager = {
        init: function (options = {}) {
            this.options = $.extend({
                containerSelector: '.pagination-container',
                itemSelector: '.pagination-item',
                activeClass: 'active',
                disabledClass: 'disabled'
            }, options);

            this.bindEvents();
        },

        bindEvents: function () {
            const self = this;

            // رویداد کلیک روی لینک‌های صفحه‌بندی
            $(document).on('click', '.pagination a[data-page]', function (e) {
                e.preventDefault();
                const page = parseInt($(this).data('page'));
                self.goToPage(page);
            });

            // رویداد تغییر تعداد آیتم در هر صفحه
            $('#page-size').on('change', function () {
                const pageSize = parseInt($(this).val());
                self.changePageSize(pageSize);
            });
        },

        goToPage: function (page) {
            // به‌روزرسانی URL
            const url = new URL(window.location);
            url.searchParams.set('page', page);
            window.location.href = url.toString();
        },

        changePageSize: function (pageSize) {
            // به‌روزرسانی URL
            const url = new URL(window.location);
            url.searchParams.set('page', 1); // بازگشت به صفحه اول
            url.searchParams.set('pageSize', pageSize);
            window.location.href = url.toString();
        },

        createPagination: function (paginationData) {
            const {
                currentPage,
                totalPages,
                pageSize,
                totalCount,
                hasNextPage,
                hasPreviousPage
            } = paginationData;

            let html = `
                <div class="pagination-container">
                    <div class="pagination-info">
                        نمایش ${(currentPage - 1) * pageSize + 1} تا 
                        ${Math.min(currentPage * pageSize, totalCount)} 
                        از ${totalCount.toLocaleString()} مورد
                    </div>
                    
                    <nav aria-label="Page navigation">
                        <ul class="pagination justify-content-center">
            `;

            // دکمه قبلی
            if (hasPreviousPage) {
                html += `
                    <li class="page-item">
                        <a class="page-link" href="#" data-page="${currentPage - 1}" aria-label="Previous">
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
            const startPage = Math.max(1, currentPage - 2);
            const endPage = Math.min(totalPages, currentPage + 2);

            if (startPage > 1) {
                html += `
                    <li class="page-item">
                        <a class="page-link" href="#" data-page="1">1</a>
                    </li>
                `;
                if (startPage > 2) {
                    html += `<li class="page-item disabled"><span class="page-link">...</span></li>`;
                }
            }

            for (let i = startPage; i <= endPage; i++) {
                if (i === currentPage) {
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

            if (endPage < totalPages) {
                if (endPage < totalPages - 1) {
                    html += `<li class="page-item disabled"><span class="page-link">...</span></li>`;
                }
                html += `
                    <li class="page-item">
                        <a class="page-link" href="#" data-page="${totalPages}">${totalPages}</a>
                    </li>
                `;
            }

            // دکمه بعدی
            if (hasNextPage) {
                html += `
                    <li class="page-item">
                        <a class="page-link" href="#" data-page="${currentPage + 1}" aria-label="Next">
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

            return html;
        },

        updatePaginationInfo: function (info) {
            const $info = $('.pagination-info');
            if ($info.length) {
                $info.text(info);
            }
        },

        scrollToTop: function () {
            $('html, body').animate({
                scrollTop: 0
            }, 'fast');
        },

        // تابع برای صفحه‌بندی سمت سرور (Server-side)
        paginateServerSide: function (options) {
            const defaults = {
                page: 1,
                pageSize: 10,
                url: window.location.href,
                containerSelector: '.data-container',
                paginationContainerSelector: '.pagination-container'
            };

            const settings = $.extend({}, defaults, options);

            return Utils.ajax({
                url: settings.url,
                data: {
                    page: settings.page,
                    pageSize: settings.pageSize
                }
            })
                .done(function (response) {
                    $(settings.containerSelector).html(response.data);
                    $(settings.paginationContainerSelector).html(response.pagination);
                });
        },

        // تابع برای صفحه‌بندی سمت کلاینت (Client-side)
        paginateClientSide: function (options) {
            const defaults = {
                items: [],
                page: 1,
                pageSize: 10,
                containerSelector: '.data-container',
                paginationContainerSelector: '.pagination-container'
            };

            const settings = $.extend({}, defaults, options);

            const totalPages = Math.ceil(settings.items.length / settings.pageSize);
            const startIndex = (settings.page - 1) * settings.pageSize;
            const endIndex = startIndex + settings.pageSize;
            const pageItems = settings.items.slice(startIndex, endIndex);

            // نمایش آیتم‌های صفحه فعلی
            this.displayItems(pageItems, settings.containerSelector);

            // ایجاد و نمایش صفحه‌بندی
            const paginationData = {
                currentPage: settings.page,
                totalPages: totalPages,
                pageSize: settings.pageSize,
                totalCount: settings.items.length,
                hasNextPage: settings.page < totalPages,
                hasPreviousPage: settings.page > 1
            };

            const paginationHtml = this.createPagination(paginationData);
            $(settings.paginationContainerSelector).html(paginationHtml);
        },

        displayItems: function (items, containerSelector) {
            const $container = $(containerSelector);
            let html = '';

            items.forEach(item => {
                // این بخش باید بر اساس ساختار داده‌های شما سفارشی شود
                html += `<div class="data-item">${JSON.stringify(item)}</div>`;
            });

            $container.html(html);
        }
    };

    // ========== Initialization ==========
    $(document).ready(function () {
        PaginationManager.init();
    });

    // ========== Expose ==========
    window.PaginationManager = PaginationManager;

})(jQuery);