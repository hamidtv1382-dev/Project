(function ($) {
    'use strict';

    const RealtimeManager = {
        connection: null,
        isPaused: false,
        charts: {},
        notifications: [],

        init: function (options = {}) {
            this.options = $.extend({
                hubUrl: '/liveDataHub',
                autoStart: true
            }, options);

            if (this.options.autoStart) {
                this.startConnection();
            }

            this.bindEvents();
        },

        startConnection: function () {
            const self = this;

            this.connection = new signalR.HubConnectionBuilder()
                .withUrl(this.options.hubUrl)
                .withAutomaticReconnect()
                .configureLogging(signalR.LogLevel.Information)
                .build();

            // رویداد دریافت به‌روزرسانی تماس
            this.connection.on("ReceiveCallUpdate", (callData) => {
                if (self.isPaused) return;
                self.handleCallUpdate(callData);
            });

            // رویداد دریافت به‌روزرسانی آمار
            this.connection.on("ReceiveStatsUpdate", (stats) => {
                if (self.isPaused) return;
                self.handleStatsUpdate(stats);
            });

            // رویداد دریافت هشدار
            this.connection.on("ReceiveAlert", (alert) => {
                if (self.isPaused) return;
                self.handleAlert(alert);
            });

            // رویداد تغییر وضعیت اتصال
            this.connection.onreconnecting(error => {
                console.warn('Connection lost, reconnecting...', error);
                self.updateConnectionStatus('reconnecting');
            });

            this.connection.onreconnected(connectionId => {
                console.log('Connection reconnected with ID:', connectionId);
                self.updateConnectionStatus('connected');
            });

            this.connection.onclose(error => {
                console.error('Connection closed due to error. Try refreshing the page.', error);
                self.updateConnectionStatus('disconnected');
            });

            // شروع اتصال
            this.connection.start()
                .then(() => {
                    console.log('SignalR Connected.');
                    self.updateConnectionStatus('connected');
                    self.requestInitialData();
                })
                .catch(err => {
                    console.error('Error starting connection:', err);
                    self.updateConnectionStatus('error');
                });
        },

        bindEvents: function () {
            const self = this;

            // دکمه توقف/ادامه به‌روزرسانی
            $('#pause-updates').on('click', function () {
                self.togglePause();
            });

            // دکمه تمام صفحه
            $('#fullscreen-dashboard').on('click', function () {
                self.toggleFullscreen();
            });
        },

        handleCallUpdate: function (callData) {
            // به‌روزرسانی آمار تماس‌ها
            this.updateCallStats(callData);

            // به‌روزرسانی نمودار جریان تماس‌ها
            this.updateCallChart(callData);

            // افزودن به جدول تماس‌های اخیر
            this.addRecentCall(callData);

            // به‌روزرسانی نقشه زنده
            if (window.MapManager) {
                MapManager.addLiveCall(callData);
            }
        },

        handleStatsUpdate: function (stats) {
            // به‌روزرسانی کارت‌های آمار
            $('.stat-value#active-calls-count').text(stats.activeCalls);
            $('.stat-value#answered-calls-count').text(stats.answeredCalls);
            $('.stat-value#missed-calls-count').text(stats.missedCalls);
            $('.stat-value#avg-duration').text(stats.avgDuration);
        },

        handleAlert: function (alert) {
            this.showNotification(alert.message, alert.type);
            this.addAlertToList(alert);
        },

        updateCallStats: function (callData) {
            let totalAnswered = parseInt($('.stat-value#answered-calls-count').text() || 0);
            let totalMissed = parseInt($('.stat-value#missed-calls-count').text() || 0);

            if (callData.answered) {
                totalAnswered++;
            } else {
                totalMissed++;
            }

            $('.stat-value#answered-calls-count').text(totalAnswered);
            $('.stat-value#missed-calls-count').text(totalMissed);

            const totalCalls = totalAnswered + totalMissed;
            const avgDuration = callData.totalDuration / totalCalls;
            $('.stat-value#avg-duration').text(avgDuration.toFixed(1));
        },

        updateCallChart: function (callData) {
            if (!this.charts.liveCallsChart) return;

            const now = new Date();
            const timeLabel = now.getHours().toString().padStart(2, '0') + ':' +
                now.getMinutes().toString().padStart(2, '0') + ':' +
                now.getSeconds().toString().padStart(2, '0');

            const chart = this.charts.liveCallsChart;
            chart.data.labels.push(timeLabel);
            chart.data.datasets[0].data.push(callData.activeCalls);

            // نگه داشتن حداکثر 20 نقطه
            if (chart.data.labels.length > 20) {
                chart.data.labels.shift();
                chart.data.datasets[0].data.shift();
            }

            chart.update('none'); // به‌روزرسانی بدون انیمیشن
        },

        addRecentCall: function (callData) {
            const tbody = $('#recent-calls-tbody');
            const newRow = tbody.find('tr:first').clone();

            newRow.find('td:eq(0)').text(callData.time);
            newRow.find('td:eq(1)').text(callData.aNumber);
            newRow.find('td:eq(2)').text(callData.bNumber);
            newRow.find('td:eq(3)').text(callData.originCountry);
            newRow.find('td:eq(4)').text(callData.destCountry);
            newRow.find('td:eq(5)').text(callData.duration);

            const statusBadge = callData.answered ?
                '<span class="badge bg-success">پاسخ داده شده</span>' :
                '<span class="badge bg-danger">پاسخ داده نشده</span>';
            newRow.find('td:eq(6)').html(statusBadge);

            // افزودن انیمیشن
            newRow.addClass('highlight-new-row');
            setTimeout(() => {
                newRow.removeClass('highlight-new-row');
            }, 2000);

            tbody.prepend(newRow);

            // حذف ردیف آخر اگر بیش از 10 ردیف داشتیم
            if (tbody.find('tr').length > 10) {
                tbody.find('tr:last').remove();
            }
        },

        showNotification: function (message, type = 'info') {
            if (typeof showNotification === 'function') {
                showNotification(message, type, 5000);
            }
        },

        addAlertToList: function (alert) {
            const alertsContainer = $('#live-alerts');
            const alertElement = $(`
                <div class="alert alert-${alert.type} alert-dismissible fade show">
                    <strong>${alert.title}</strong><br>
                    <small>${alert.message}</small>
                    <button type="button" class="close" data-dismiss="alert" aria-label="Close">
                        <span aria-hidden="true">&times;</span>
                    </button>
                </div>
            `);

            alertsContainer.prepend(alertElement);

            // حذف خودکار پس از 10 ثانیه
            setTimeout(() => {
                alertElement.alert('close');
            }, 10000);
        },

        updateConnectionStatus: function (status) {
            const statusIndicator = $('.status-indicator');
            const statusText = $('.status-text');

            statusIndicator.removeClass('connected disconnected reconnecting error');
            statusText.removeClass('text-success text-warning text-danger');

            switch (status) {
                case 'connected':
                    statusIndicator.addClass('connected');
                    statusText.text('متصل');
                    break;
                case 'disconnected':
                    statusIndicator.addClass('disconnected');
                    statusText.text('قطع ارتباط');
                    statusText.addClass('text-danger');
                    break;
                case 'reconnecting':
                    statusIndicator.addClass('reconnecting');
                    statusText.text('در حال اتصال مجدد...');
                    statusText.addClass('text-warning');
                    break;
                case 'error':
                    statusIndicator.addClass('error');
                    statusText.text('خطا در اتصال');
                    statusText.addClass('text-danger');
                    break;
            }
        },

        togglePause: function () {
            this.isPaused = !this.isPaused;
            const $button = $('#pause-updates');

            if (this.isPaused) {
                $button.html('<i class="fas fa-play"></i> ادامه به‌روزرسانی');
                $button.removeClass('btn-outline-warning').addClass('btn-outline-success');
            } else {
                $button.html('<i class="fas fa-pause"></i> توقف به‌روزرسانی');
                $button.removeClass('btn-outline-success').addClass('btn-outline-warning');
            }
        },

        toggleFullscreen: function () {
            const elem = $('.live-dashboard-container')[0];

            if (!document.fullscreenElement) {
                elem.requestFullscreen().then(() => {
                    $('#fullscreen-dashboard').html('<i class="fas fa-compress"></i> خروج از تمام صفحه');
                }).catch(err => {
                    console.error(`Error attempting to enable fullscreen: ${err.message}`);
                });
            } else {
                document.exitFullscreen().then(() => {
                    $('#fullscreen-dashboard').html('<i class="fas fa-expand"></i> تمام صفحه');
                });
            }
        },

        requestInitialData: function () {
            const self = this;

            this.connection.invoke("GetInitialData")
                .then(data => {
                    self.handleStatsUpdate(data.stats);
                    self.updateCharts(data.charts);
                })
                .catch(err => console.error('Error getting initial data:', err));
        },

        updateCharts: function (chartsData) {
            // ایجاد یا به‌روزرسانی نمودار زنده تماس‌ها
            if (!this.charts.liveCallsChart && chartsData.callVolume) {
                this.createLiveCallsChart(chartsData.callVolume);
            }

            // ایجاد یا به‌روزرسانی نمودار نرخ پاسخگویی
            if (!this.charts.answerRateChart && chartsData.answerRate) {
                this.createAnswerRateChart(chartsData.answerRate);
            }
        },

        createLiveCallsChart: function (data) {
            const ctx = document.getElementById('live-calls-chart');
            if (!ctx) return;

            this.charts.liveCallsChart = new Chart(ctx, {
                type: 'line',
                data: {
                    labels: data.labels,
                    datasets: [{
                        label: 'تماس‌های فعال',
                        data: data.data,
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
                    animation: {
                        duration: 0 // غیرفعال کردن انیمیشن برای به‌روزرسانی سریع
                    }
                }
            });
        },

        createAnswerRateChart: function (data) {
            const ctx = document.getElementById('answer-rate-chart');
            if (!ctx) return;

            this.charts.answerRateChart = new Chart(ctx, {
                type: 'doughnut',
                data: {
                    labels: ['پاسخ داده شده', 'پاسخ داده نشده'],
                    datasets: [{
                        data: data,
                        backgroundColor: [
                            'rgba(75, 192, 192, 0.2)',
                            'rgba(255, 99, 132, 0.2)'
                        ],
                        borderColor: [
                            'rgba(75, 192, 192, 1)',
                            'rgba(255, 99, 132, 1)'
                        ],
                        borderWidth: 1
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    animation: {
                        duration: 0
                    }
                }
            });
        },

        stopConnection: function () {
            if (this.connection) {
                this.connection.stop();
            }
        }
    };

    // ========== Initialization ==========
    $(document).ready(function () {
        // فقط در صفحات داشبورد زنده، مدیریت‌کننده زنده را مقداردهی اولیه کن
        if ($('.live-dashboard-container').length) {
            RealtimeManager.init();
        }
    });

    // ========== Expose ==========
    window.RealtimeManager = RealtimeManager;

})(jQuery);