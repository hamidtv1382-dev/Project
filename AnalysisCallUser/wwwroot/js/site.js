(function ($) {
    'use strict';

    // ========== Global Variables ==========
    window.App = {
        baseUrl: '/',
        apiBaseUrl: '/api/',
        isRTL: $('html').attr('dir') === 'rtl',
        theme: localStorage.getItem('theme') || 'light'
    };

    // ========== Utility Functions ==========
    const Utils = {
        // نمایش یک پیام به صورت Toast
        showToast: function (message, type = 'info', duration = 5000) {
            if (typeof showNotification === 'function') {
                showNotification(message, type, duration);
            } else {
                alert(message); // Fallback
            }
        },

        // فرمت‌بندی اعداد با جداکننده هزار
        formatNumber: function (num) {
            return num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ',');
        },

        // تبدیل تاریخ به فرمت محلی
        formatDate: function (date, format = 'YYYY-MM-DD') {
            const d = new Date(date);
            const year = d.getFullYear();
            const month = String(d.getMonth() + 1).padStart(2, '0');
            const day = String(d.getDate()).padStart(2, '0');

            return format
                .replace('YYYY', year)
                .replace('MM', month)
                .replace('DD', day);
        },

        // دریافت مقدار یک کوکی
        getCookie: function (name) {
            const value = `; ${document.cookie}`;
            const parts = value.split(`; ${name}=`);
            if (parts.length === 2) return parts.pop().split(';').shift();
            return null;
        },

        // تنظیم یک کوکی
        setCookie: function (name, value, days) {
            const date = new Date();
            date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
            const expires = `expires=${date.toUTCString()}`;
            document.cookie = `${name}=${value};${expires};path=/`;
        },

        // ارسال درخواست AJAX با مدیریت خطا
        ajax: function (options) {
            const defaults = {
                type: 'POST',
                dataType: 'json',
                contentType: 'application/json',
                headers: {
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                },
                error: function (xhr, status, error) {
                    console.error('AJAX Error:', xhr.responseText);
                    Utils.showToast('خطا در ارتباط با سرور. لطفاً دوباره تلاش کنید.', 'danger');
                }
            };

            return $.ajax($.extend(true, defaults, options));
        },

        // غیرفعال/فعال کردن یک دکمه با لودینگ
        setButtonLoading: function (button, loading = true) {
            const $btn = $(button);
            if (loading) {
                $btn.data('original-text', $btn.html());
                $btn.html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> در حال پردازش...');
                $btn.prop('disabled', true);
            } else {
                $btn.html($btn.data('original-text'));
                $btn.prop('disabled', false);
            }
        }
    };

    // ========== DOM Ready ==========
    $(document).ready(function () {
        // اعمال تم ذخیره شده
        if (App.theme === 'dark') {
            $('body').addClass('dark-theme');
        }

        // مدیریت منوی کناری (Sidebar)
        const $sidebarToggle = $('#sidebar-toggle');
        const $sidebar = $('.sidebar');
        const $content = $('.content-wrapper');

        if ($sidebarToggle.length) {
            $sidebarToggle.on('click', function (e) {
                e.preventDefault();
                $sidebar.toggleClass('collapsed');
                $content.toggleClass('full-width');
            });
        }

        // مدیریت منوی در موبایل
        const $mobileMenuToggle = $('.mobile-menu-toggle');
        if ($mobileMenuToggle.length) {
            $mobileMenuToggle.on('click', function (e) {
                e.preventDefault();
                $sidebar.toggleClass('show');
            });
        }

        // بستن منوی کناری با کلیک روی محتوا در موبایل
        $(document).on('click', function (e) {
            if ($(window).width() < 992 &&
                !$sidebar.is(e.target) &&
                $sidebar.has(e.target).length === 0 &&
                !$sidebarToggle.is(e.target) &&
                $sidebarToggle.has(e.target).length === 0) {
                $sidebar.removeClass('show');
            }
        });

        // مدیریت تب‌ها
        $('[data-toggle="tab"]').on('click', function (e) {
            e.preventDefault();
            const $this = $(this);
            const $target = $($this.attr('href') || $this.data('target'));

            $this.parent().find('.active').removeClass('active');
            $this.addClass('active');

            $target.parent().find('.tab-pane').removeClass('active show');
            $target.addClass('active show');
        });

        // مدیریت مودال‌ها
        $('.modal').on('show.bs.modal', function () {
            $('body').addClass('modal-open');
        });

        $('.modal').on('hidden.bs.modal', function () {
            $('body').removeClass('modal-open');
        });

        // مدیریت توولتیپ‌ها
        $('[data-toggle="tooltip"]').tooltip();

        // مدیریت پاپ‌آورها
        $('[data-toggle="popover"]').popover();

        // مدیریت دکمه‌های بازگشت به بالا
        const $backToTop = $('#back-to-top');
        if ($backToTop.length) {
            $(window).on('scroll', function () {
                if ($(this).scrollTop() > 200) {
                    $backToTop.fadeIn();
                } else {
                    $backToTop.fadeOut();
                }
            });

            $backToTop.on('click', function (e) {
                e.preventDefault();
                $('html, body').animate({ scrollTop: 0 }, 'fast');
            });
        }

        // مدیریت لینک‌های خارجی
        $('a[href^="http"]').attr('target', '_blank').attr('rel', 'noopener noreferrer');

        // اعمال کلاس‌های انیمیشن به عناصر ورودی
        $('.animated').each(function () {
            const $el = $(this);
            const animationClass = $el.data('animation') || 'fadeIn';
            const delay = $el.data('delay') || 0;

            setTimeout(function () {
                $el.addClass(animationClass);
            }, delay);
        });
    });

    // ========== Window Events ==========
    $(window).on('resize', function () {
        // بستن منوی کناری در حالت دسکتاپ اگر باز بود
        if ($(window).width() >= 992) {
            $('.sidebar').removeClass('show');
        }
    });

    // ========== Expose Utilities ==========
    window.Utils = Utils;

})(jQuery);