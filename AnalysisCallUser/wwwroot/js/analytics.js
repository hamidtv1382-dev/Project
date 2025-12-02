(function ($) {
    'use strict';

    const AnalyticsManager = {
        charts: {},
        currentPeriod: 'day',

        init: function () {
            this.bindEvents();
            this.initializeCharts();
        },

        bindEvents: function () {
            const self = this;

            // رویداد تغییر دوره زمانی
            $('[data-period]').on('click', function () {
                $('[data-period]').removeClass('active');
                $(this).addClass('active');
                self.changePeriod($(this).data('period'));
            });

            // رویداد تولید پیش‌بینی
            $('#generate-prediction').on('click', function () {
                self.generatePrediction();
            });

            // رویداد محاسبه همبستگی
            $('#calculate-correlation').on('click', function () {
                self.calculateCorrelation();
            });

            // رویداد به‌روزرسانی تحلیل
            $('#refresh-analysis').on('click', function () {
                self.refreshAnalysis();
            });
        },

        initializeCharts: function () {
            // نمودار حجم تماس‌ها در طول زمان
            this.charts.timeAnalysisChart = this.createTimeAnalysisChart();

            // نمودار تحلیل فصلی
            this.charts.seasonalChart = this.createSeasonalChart();

            // نمودار پیش‌بینی
            this.charts.predictionChart = this.createPredictionChart();

            // نمودار همبستگی
            this.charts.correlationChart = this.createCorrelationChart();
        },

        createTimeAnalysisChart: function () {
            const ctx = document.getElementById('time-analysis-chart');
            if (!ctx) return null;

            return new Chart(ctx, {
                type: 'line',
                data: {
                    labels: this.generateTimeLabels(24),
                    datasets: [{
                        label: 'تعداد تماس‌ها',
                        data: this.generateRandomData(24, 50, 200),
                        backgroundColor: 'rgba(54, 162, 235, 0.2)',
                        borderColor: 'rgba(54, 162, 235, 1)',
                        borderWidth: 1,
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

        createSeasonalChart: function () {
            const ctx = document.getElementById('seasonal-analysis-chart');
            if (!ctx) return null;

            return new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: ['بهار', 'تابستان', 'پاییز', 'زمستان'],
                    datasets: [{
                        label: 'میانگین تماس‌ها',
                        data: this.generateRandomData(4, 1000, 2000),
                        backgroundColor: [
                            'rgba(75, 192, 192, 0.2)',
                            'rgba(255, 206, 86, 0.2)',
                            'rgba(255, 159, 64, 0.2)',
                            'rgba(54, 162, 235, 0.2)'
                        ],
                        borderColor: [
                            'rgba(75, 192, 192, 1)',
                            'rgba(255, 206, 86, 1)',
                            'rgba(255, 159, 64, 1)',
                            'rgba(54, 162, 235, 1)'
                        ],
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

        createPredictionChart: function () {
            const ctx = document.getElementById('prediction-chart');
            if (!ctx) return null;

            return new Chart(ctx, {
                type: 'line',
                data: {
                    labels: [],
                    datasets: [{
                        label: 'داده‌های واقعی',
                        data: [],
                        backgroundColor: 'rgba(54, 162, 235, 0.2)',
                        borderColor: 'rgba(54, 162, 235, 1)',
                        borderWidth: 1,
                        tension: 0.4,
                        fill: true
                    }, {
                        label: 'پیش‌بینی',
                        data: [],
                        backgroundColor: 'rgba(255, 99, 132, 0.2)',
                        borderColor: 'rgba(255, 99, 132, 1)',
                        borderWidth: 1,
                        borderDash: [5, 5],
                        tension: 0.4,
                        fill: false
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    scales: {
                        y: {
                            beginAtZero: true
                        }
                    }
                }
            });
        },

        createCorrelationChart: function () {
            const ctx = document.getElementById('correlation-chart');
            if (!ctx) return null;

            return new Chart(ctx, {
                type: 'scatter',
                data: {
                    datasets: [{
                        label: 'نقاط داده',
                        data: this.generateScatterData(50),
                        backgroundColor: 'rgba(54, 162, 235, 0.5)',
                        borderColor: 'rgba(54, 162, 235, 1)',
                        borderWidth: 1
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    scales: {
                        x: {
                            type: 'linear',
                            position: 'bottom',
                            title: {
                                display: true,
                                text: 'زمان (t)'
                            }
                        },
                        y: {
                            title: {
                                display: true,
                                text: 'زمان (t + lag)'
                            }
                        }
                    }
                }
            });
        },

        changePeriod: function (period) {
            this.currentPeriod = period;

            Utils.setButtonLoading('#refresh-analysis', true);

            Utils.ajax({
                url: App.apiBaseUrl + 'analytics/updateTimeAnalysis',
                data: JSON.stringify({ period: period })
            })
                .done((data) => {
                    this.updateTimeAnalysisChart(data);
                    this.updatePatternAnalysis(data);
                })
                .fail(() => {
                    Utils.showToast('خطا در به‌روزرسانی تحلیل زمانی', 'danger');
                })
                .always(() => {
                    Utils.setButtonLoading('#refresh-analysis', false);
                });
        },

        updateTimeAnalysisChart: function (data) {
            if (this.charts.timeAnalysisChart) {
                this.charts.timeAnalysisChart.data.labels = data.labels;
                this.charts.timeAnalysisChart.data.datasets[0].data = data.data;
                this.charts.timeAnalysisChart.update();
            }
        },

        updatePatternAnalysis: function (data) {
            if (data.data && data.data.length > 0) {
                const maxValue = Math.max(...data.data);
                const maxIndex = data.data.indexOf(maxValue);
                const peakTime = data.labels[maxIndex];

                const minValue = Math.min(...data.data);
                const minIndex = data.data.indexOf(minValue);
                const lowTime = data.labels[minIndex];

                $('#peak-time').text(peakTime);
                $('#low-time').text(lowTime);

                if (data.data.length > 1) {
                    const firstValue = data.data[0];
                    const lastValue = data.data[data.data.length - 1];
                    const growthRate = ((lastValue - firstValue) / firstValue * 100).toFixed(1);
                    $('#growth-trend').text(growthRate + '%');
                }
            }
        },

        generatePrediction: function () {
            const period = parseInt($('#prediction-period').val());
            const model = $('#prediction-model').val();

            Utils.setButtonLoading('#generate-prediction', true);

            Utils.ajax({
                url: App.apiBaseUrl + 'analytics/generateTimePrediction',
                data: JSON.stringify({
                    period: period,
                    model: model
                })
            })
                .done((data) => {
                    if (this.charts.predictionChart) {
                        this.charts.predictionChart.data.labels = data.labels;
                        this.charts.predictionChart.data.datasets[0].data = data.actualData;
                        this.charts.predictionChart.data.datasets[1].data = data.predictionData;
                        this.charts.predictionChart.update();
                    }
                })
                .fail(() => {
                    Utils.showToast('خطا در تولید پیش‌بینی', 'danger');
                })
                .always(() => {
                    Utils.setButtonLoading('#generate-prediction', false);
                });
        },

        calculateCorrelation: function () {
            const metric = $('#correlation-metric').val();
            const lag = parseInt($('#correlation-lag').val());

            Utils.setButtonLoading('#calculate-correlation', true);

            Utils.ajax({
                url: App.apiBaseUrl + 'analytics/calculateTimeCorrelation',
                data: JSON.stringify({
                    metric: metric,
                    lag: lag
                })
            })
                .done((data) => {
                    if (this.charts.correlationChart) {
                        this.charts.correlationChart.data.datasets[0].data = data.scatterData;
                        this.charts.correlationChart.update();

                        // نمایش مقدار همبستگی
                        const correlationValue = data.correlationValue.toFixed(3);
                        this.charts.correlationChart.options.plugins.title = {
                            display: true,
                            text: `ضریب همبستگی: ${correlationValue}`
                        };
                        this.charts.correlationChart.update();
                    }
                })
                .fail(() => {
                    Utils.showToast('خطا در محاسبه همبستگی', 'danger');
                })
                .always(() => {
                    Utils.setButtonLoading('#calculate-correlation', false);
                });
        },

        refreshAnalysis: function () {
            this.changePeriod(this.currentPeriod);
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
        },

        generateScatterData: function (count) {
            const data = [];
            for (let i = 0; i < count; i++) {
                data.push({
                    x: Math.random() * 100,
                    y: Math.random() * 100
                });
            }
            return data;
        }
    };

    // ========== Initialization ==========
    $(document).ready(function () {
        // فقط در صفحات تحلیل، مدیریت‌کننده تحلیل را مقداردهی اولیه کن
        if ($('.analytics-container').length) {
            AnalyticsManager.init();
        }
    });

    // ========== Expose ==========
    window.AnalyticsManager = AnalyticsManager;

})(jQuery);