(function ($) {
    'use strict';

    const ThemeManager = {
        currentTheme: 'light',
        storageKey: 'app-theme',

        init: function () {
            this.loadTheme();
            this.bindEvents();
            this.applyTheme();
        },

        bindEvents: function () {
            const self = this;

            // رویداد تغییر تم
            $('#theme-switch').on('change', function () {
                self.toggleTheme();
            });

            // رویداد کلیک روی دکمه تغییر تم (اگر وجود داشته باشد)
            $('#theme-toggle-btn').on('click', function (e) {
                e.preventDefault();
                self.toggleTheme();
            });

            // شناسایی تغییرات تم سیستم
            if (window.matchMedia) {
                const darkModeQuery = window.matchMedia('(prefers-color-scheme: dark)');
                darkModeQuery.addListener((e) => {
                    if (!localStorage.getItem(self.storageKey)) {
                        self.currentTheme = e.matches ? 'dark' : 'light';
                        self.applyTheme();
                    }
                });
            }
        },

        loadTheme: function () {
            // بارگذاری تم ذخیره شده
            const savedTheme = localStorage.getItem(this.storageKey);

            if (savedTheme) {
                this.currentTheme = savedTheme;
            } else {
                // اگر تم ذخیره نشده، از تنظیمات سیستم استفاده کن
                if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
                    this.currentTheme = 'dark';
                }
            }
        },

        toggleTheme: function () {
            this.currentTheme = this.currentTheme === 'light' ? 'dark' : 'light';
            this.saveTheme();
            this.applyTheme();
            this.notifyThemeChange();
        },

        setTheme: function (theme) {
            if (theme === 'light' || theme === 'dark') {
                this.currentTheme = theme;
                this.saveTheme();
                this.applyTheme();
            }
        },

        saveTheme: function () {
            localStorage.setItem(this.storageKey, this.currentTheme);
        },

        applyTheme: function () {
            const $body = $('body');
            const $themeSwitch = $('#theme-switch');
            const $themeIcon = $('#theme-icon');
            const $themeText = $('#theme-text');

            // حذف کلاس‌های تم
            $body.removeClass('light-theme dark-theme');

            // افزودن کلاس تم فعلی
            $body.addClass(`${this.currentTheme}-theme`);

            // به‌روزرسانی وضعیت سوئیچ تم
            if ($themeSwitch.length) {
                $themeSwitch.prop('checked', this.currentTheme === 'dark');
            }

            // به‌روزرسانی آیکون و متن
            if ($themeIcon.length) {
                if (this.currentTheme === 'dark') {
                    $themeIcon.removeClass('fa-moon').addClass('fa-sun');
                } else {
                    $themeIcon.removeClass('fa-sun').addClass('fa-moon');
                }
            }

            if ($themeText.length) {
                $themeText.text(this.currentTheme === 'dark' ? 'تم روشن' : 'تم تیره');
            }

            // به‌روزرسانی متغیرهای CSS
            this.updateCSSVariables();

            // اطلاع‌رسانی به کامپوننت‌های دیگر
            $(document).trigger('themeChanged', this.currentTheme);
        },

        updateCSSVariables: function () {
            const root = document.documentElement;

            if (this.currentTheme === 'dark') {
                root.style.setProperty('--bs-body-bg', '#1a1d23');
                root.style.setProperty('--bs-body-color', '#e9ecef');
                // سایر متغیرهای تم تیره...
            } else {
                root.style.setProperty('--bs-body-bg', '#f8f9fa');
                root.style.setProperty('--bs-body-color', '#212529');
                // سایر متغیرهای تم روشن...
            }
        },

        notifyThemeChange: function () {
            const message = this.currentTheme === 'dark' ?
                'تم تیره فعال شد' :
                'تم روشن فعال شد';

            // نمایش اعلان کوتاه
            if (typeof Utils !== 'undefined' && Utils.showToast) {
                Utils.showToast(message, 'info', 2000);
            }
        },

        getSystemTheme: function () {
            if (window.matchMedia) {
                return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
            }
            return 'light';
        },

        resetToSystemTheme: function () {
            localStorage.removeItem(this.storageKey);
            this.currentTheme = this.getSystemTheme();
            this.applyTheme();
        },

        // تابع برای به‌روزرسانی تم نمودارها
        updateChartTheme: function (chart) {
            if (!chart) return;

            const isDark = this.currentTheme === 'dark';
            const textColor = isDark ? '#e9ecef' : '#212529';
            const gridColor = isDark ? '#495057' : '#dee2e6';

            // به‌روزرسانی گزینه‌های نمودار
            if (chart.options.scales) {
                Object.keys(chart.options.scales).forEach(key => {
                    const scale = chart.options.scales[key];
                    scale.ticks = scale.ticks || {};
                    scale.ticks.color = textColor;
                    scale.grid = scale.grid || {};
                    scale.grid.color = gridColor;
                });
            }

            if (chart.options.plugins && chart.options.plugins.legend) {
                chart.options.plugins.legend.labels = chart.options.plugins.legend.labels || {};
                chart.options.plugins.legend.labels.color = textColor;
            }

            chart.update();
        }
    };

    // ========== Initialization ==========
    $(document).ready(function () {
        ThemeManager.init();
    });

    // ========== Expose ==========
    window.ThemeManager = ThemeManager;

})(jQuery);