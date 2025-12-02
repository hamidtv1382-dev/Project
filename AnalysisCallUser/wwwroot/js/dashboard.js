(function ($) {
    'use strict';

    const DashboardManager = {
        charts: {},
        refreshInterval: null,
        autoRefreshEnabled: true,

        init: function (options = {}) {
            this.options = $.extend({
                refreshInterval: 60000, // 1 دقیقه
                chartsContainer: '.chart-container'
            }, options);

            this.bindEvents();
            this.initializeCharts();
            this.startAutoRefresh();
        },

        bindEvents: function () {
            const self = this;

            // دکمه به‌روزرسانی دستی
            $('#refresh-dashboard').on('click', function () {
                self.refreshData();
            });

            // تغییر دوره زمانی در نمودارها
            $('[data-period]').on('click', function () {
                $('[data-period]').removeClass('active');
                $(this).addClass('active');
                self.updateCharts($(this).data('period'));
            });

            // تغییر اندازه پنجره
            $(window).on('resize', function () {
                self.resizeCharts();
            });
        },

        initializeCharts: function () {
            // نمودار حجم تماس‌ها
            this.charts.callVolumeChart = this.createLineChart('call-volume-chart', {
                labels: this.generateTimeLabels(24),
                data: this.generateRandomData(24, 50, 200)
            });

            // نمودار کشورها
            this.charts.countriesChart = this.createBarChart('countries-chart', {
                labels: ['ایران', 'آمریکا', 'آلمان', 'چین', 'ژاپن', 'انگلستان'],
                data: this.generateRandomData(6, 100, 500)
            });

            // نمودار نرخ پاسخگویی
            this.charts.answerRateChart = this.createDoughnutChart('answer-rate-chart', {
                labels: ['پاسخ داده شده', 'پاسخ داده نشده'],
                data: [75, 25]
            });
        },

        createLineChart: function (canvasId, options) {
            const ctx = document.getElementById(canvasId);
            if (!ctx) return null;

            return new Chart(ctx, {
                type: 'line',
                data: {
                    labels: options.labels,
                    datasets: [{
                        label: options.label || 'تعداد تماس‌ها',
                        data: options.data,
                        borderColor: 'rgba(74, 108, 247, 1)',
                        backgroundColor: 'rgba(74, 108, 247, 0.2)',
                        borderWidth: 2,
                        tension: 0.4,
                        fill: true
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    scales: {
                        y: {
                            beginAtZero: true
                        }
                    },
                    plugins: {
                        legend: {
                            display: false
                        }
                    }
                }
            });
        },

        createBarChart: function (canvasId, options) {
            const ctx = document.getElementById(canvasId);
            if (!ctx) return null;

            return new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: options.labels,
                    datasets: [{
                        label: options.label || 'تعداد تماس‌ها',
                        data: options.data,
                        backgroundColor: 'rgba(23, 162, 184, 0.2)',
                        borderColor: 'rgba(23, 162, 184, 1)',
                        borderWidth: 1
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    scales: {
                        y: {
                            beginAtZero: true
                        }
                    },
                    plugins: {
                        legend: {
                            display: false
                        }
                    }
                }
            });
        },

        createDoughnutChart: function (canvasId, options) {
            const ctx = document.getElementById(canvasId);
            if (!ctx) return null;

            return new Chart(ctx, {
                type: 'doughnut',
                data: {
                    labels: options.labels,
                    datasets: [{
                        data: options.data,
                        backgroundColor: [
                            'rgba(40, 167, 69, 0.2)',
                            'rgba(220, 53, 69, 0.2)'
                        ],
                        borderColor: [
                            'rgba(40, 167, 69, 1)',
                            'rgba(220, 53, 69, 1)'
                        ],
                        borderWidth: 1
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            position: 'bottom'
                        }
                    }
                }
            });
        },

        refreshData: function () {
            const self = this;

            Utils.ajax({
                url: App.apiBaseUrl + 'dashboard/refresh',
                type: 'GET'
            })
                .done(function (data) {
                    self.updateStats(data.stats);
                    self.updateChartsData(data.charts);
                    Utils.showToast('داده‌های داشبورد با موفقیت به‌روزرسانی شد.', 'success');
                })
                .fail(function () {
                    Utils.showToast('خطا در به‌روزرسانی داده‌های داشبورد.', 'danger');
                });
        },

        updateStats: function (stats) {
            $('.stat-value#total-calls').text(Utils.formatNumber(stats.totalCalls));
            $('.stat-value#answered-calls').text(Utils.formatNumber(stats.answeredCalls));
            $('.stat-value#avg-duration').text(stats.avgDuration + 's');
            $('.stat-value#answer-rate').text(stats.answerRate + '%');
        },

        updateChartsData: function (chartsData) {
            // به‌روزرسانی نمودار حجم تماس‌ها
            if (this.charts.callVolumeChart && chartsData.callVolume) {
                this.charts.callVolumeChart.data.datasets[0].data = chartsData.callVolume;
                this.charts.callVolumeChart.update();
            }

            // به‌روزرسانی نمودار کشورها
            if (this.charts.countriesChart && chartsData.countries) {
                this.charts.countriesChart.data.datasets[0].data = chartsData.countries;
                this.charts.countriesChart.update();
            }

            // به‌روزرسانی نمودار نرخ پاسخگویی
            if (this.charts.answerRateChart && chartsData.answerRate) {
                this.charts.answerRateChart.data.datasets[0].data = chartsData.answerRate;
                this.charts.answerRateChart.update();
            }
        },

        updateCharts: function (period) {
            // اینجا می‌توان بر اساس دوره زمانی، داده‌های جدید را از سرور گرفت
            console.log('Updating charts for period:', period);
        },

        resizeCharts: function () {
            Object.values(this.charts).forEach(chart => {
                if (chart) {
                    chart.resize();
                }
            });
        },

        startAutoRefresh: function () {
            if (!this.autoRefreshEnabled) return;

            this.refreshInterval = setInterval(() => {
                this.refreshData();
            }, this.options.refreshInterval);
        },

        stopAutoRefresh: function () {
            if (this.refreshInterval) {
                clearInterval(this.refreshInterval);
                this.refreshInterval = null;
            }
        },

        toggleAutoRefresh: function () {
            if (this.refreshInterval) {
                this.stopAutoRefresh();
            } else {
                this.startAutoRefresh();
            }
        },

        // توابع کمکی برای تولید داده‌های آزمایشی
        generateTimeLabels: function (count) {
            const labels = [];
            const now = new Date();
            for (let i = count - 1; i >= 0; i--) {
                const time = new Date(now - i * 60 * 60 * 1000);
                labels.push(time.getHours() + ':00');
            }
            return labels;
        },

        generateRandomData: function (count, min, max) {
            const data = [];
            for (let i = 0; i < count; i++) {
                data.push(Math.floor(Math.random() * (max - min + 1)) + min);
            }
            return data;
        }
    };

    // ========== Initialization ==========
    $(document).ready(function () {
        // فقط در صفحه داشبورد، داشبورد منیجر را مقداردهی اولیه کن
        if ($('.dashboard-container').length) {
            DashboardManager.init();
        }
    });

    // ========== Expose ==========
    window.DashboardManager = DashboardManager;

})(jQuery);