(function ($) {
    'use strict';

    const MapManager = {
        map: null,
        markers: [],
        heatLayer: null,
        currentLayer: null,

        init: function (options = {}) {
            this.options = $.extend({
                mapId: 'world-map',
                defaultCenter: [30, 0],
                defaultZoom: 2,
                tileLayer: 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png'
            }, options);

            this.initializeMap();
            this.bindEvents();
        },

        initializeMap: function () {
            this.map = L.map(this.options.mapId).setView(
                this.options.defaultCenter,
                this.options.defaultZoom
            );

            L.tileLayer(this.options.tileLayer, {
                attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
            }).addTo(this.map);
        },

        bindEvents: function () {
            const self = this;

            // کنترل زوم
            $('#zoom-in').on('click', function () {
                self.map.zoomIn();
            });

            $('#zoom-out').on('click', function () {
                self.map.zoomOut();
            });

            $('#zoom-fit').on('click', function () {
                self.fitBounds();
            });

            $('#reset-map').on('click', function () {
                self.resetView();
            });

            // تغییر نوع نمایش
            $('#visualization-type').on('change', function () {
                const type = $(this).val();
                self.changeVisualization(type);
            });

            // اعمال فیلترها
            $('#apply-filters').on('click', function () {
                self.applyFilters();
            });
        },

        changeVisualization: function (type) {
            // حذف لایه فعلی
            if (this.currentLayer) {
                this.map.removeLayer(this.currentLayer);
            }

            switch (type) {
                case 'heat':
                    this.showHeatMap();
                    break;
                case 'bubble':
                    this.showBubbleMap();
                    break;
                case 'choropleth':
                    this.showChoroplethMap();
                    break;
                case 'flow':
                    this.showFlowMap();
                    break;
            }
        },

        showBubbleMap: function (data) {
            const self = this;
            data = data || this.getMapData();

            this.currentLayer = L.layerGroup();

            data.forEach(point => {
                const color = this.getColor(point.callCount);
                const radius = this.getRadius(point.callCount);

                L.circle([point.lat, point.lng], {
                    color: color,
                    fillColor: color,
                    fillOpacity: 0.7,
                    radius: radius
                })
                    .bindPopup(`<strong>${point.country}</strong><br>تعداد تماس‌ها: ${point.callCount.toLocaleString()}`)
                    .on('click', function () {
                        self.showRegionDetails(point);
                    })
                    .addTo(this.currentLayer);
            });

            this.currentLayer.addTo(this.map);
        },

        showHeatMap: function (data) {
            data = data || this.getMapData();

            const heatData = data.map(point => [point.lat, point.lng, point.callCount]);

            this.currentLayer = L.heatLayer(heatData, {
                radius: 25,
                blur: 15,
                maxZoom: 17,
                max: Math.max(...data.map(p => p.callCount))
            });

            this.currentLayer.addTo(this.map);
        },

        showChoroplethMap: function (data) {
            // این بخش نیاز به GeoJSON برای مرزهای کشورها دارد
            data = data || this.getMapData();

            this.currentLayer = L.layerGroup();

            data.forEach(point => {
                L.circleMarker([point.lat, point.lng], {
                    radius: 10,
                    fillColor: this.getColor(point.callCount),
                    color: '#fff',
                    weight: 1,
                    opacity: 1,
                    fillOpacity: 0.8
                })
                    .bindPopup(`<strong>${point.country}</strong><br>تعداد تماس‌ها: ${point.callCount.toLocaleString()}`)
                    .addTo(this.currentLayer);
            });

            this.currentLayer.addTo(this.map);
        },

        showFlowMap: function (data) {
            // این بخش نیاز به داده‌های جریان بین مناطق دارد
            data = data || this.getMapData();

            this.currentLayer = L.layerGroup();

            data.forEach(point => {
                L.circle([point.lat, point.lng], {
                    radius: 8,
                    fillColor: '#3498db',
                    color: '#fff',
                    weight: 1,
                    opacity: 1,
                    fillOpacity: 0.8
                })
                    .bindPopup(`<strong>${point.country}</strong><br>تعداد تماس‌ها: ${point.callCount.toLocaleString()}`)
                    .addTo(this.currentLayer);
            });

            this.currentLayer.addTo(this.map);
        },

        applyFilters: function () {
            const self = this;
            const filters = {
                startDate: $('#start-date').val(),
                endDate: $('#end-date').val(),
                callType: $('#call-type').val(),
                visualizationType: $('#visualization-type').val()
            };

            Utils.setButtonLoading('#apply-filters', true);

            Utils.ajax({
                url: App.apiBaseUrl + 'map/update',
                data: JSON.stringify(filters)
            })
                .done(function (data) {
                    self.changeVisualization(filters.visualizationType);
                    self.updateRegionStats(data.stats);
                    Utils.showToast('فیلترها با موفقیت اعمال شدند.', 'success');
                })
                .fail(function () {
                    Utils.showToast('خطا در اعمال فیلترها.', 'danger');
                })
                .always(function () {
                    Utils.setButtonLoading('#apply-filters', false);
                });
        },

        updateRegionStats: function (stats) {
            $('#active-countries').text(stats.activeCountries);
            $('#active-cities').text(stats.activeCities);
            $('#top-country').text(stats.topCountry);
            $('#top-city').text(stats.topCity);
        },

        showRegionDetails: function (region) {
            // نمایش مودال جزئیات منطقه
            $('#regionDetailsModal .modal-title').text(region.country);
            $('#region-details-content').html(`
                <p><strong>کشور:</strong> ${region.country}</p>
                <p><strong>تعداد تماس‌ها:</strong> ${region.callCount.toLocaleString()}</p>
                <p><strong>میانگین مدت:</strong> ${region.avgDuration} ثانیه</p>
                <p><strong>نرخ پاسخگویی:</strong> ${region.answerRate}%</p>
            `);
            $('#regionDetailsModal').modal('show');
        },

        fitBounds: function () {
            if (this.currentLayer && this.currentLayer.getBounds) {
                this.map.fitBounds(this.currentLayer.getBounds());
            }
        },

        resetView: function () {
            this.map.setView(this.options.defaultCenter, this.options.defaultZoom);
        },

        getColor: function (callCount) {
            return callCount > 50000 ? '#b10026' :
                callCount > 10000 ? '#e31a1c' :
                    callCount > 5000 ? '#fc4e2a' :
                        callCount > 1000 ? '#fd8d3c' :
                            callCount > 500 ? '#feb24c' :
                                callCount > 100 ? '#fed976' :
                                    '#ffeda0';
        },

        getRadius: function (callCount) {
            return Math.sqrt(callCount) * 1000;
        },

        getMapData: function () {
            // این تابع باید داده‌های فعلی نقشه را برگرداند
            // در یک پیاده‌سازی واقعی، این داده‌ها از سرور گرفته می‌شوند
            return [];
        },

        exportMap: function () {
            html2canvas(document.getElementById(this.options.mapId)).then(canvas => {
                const link = document.createElement('a');
                link.download = 'map-export.png';
                link.href = canvas.toDataURL();
                link.click();
            });
        }
    };

    // ========== Initialization ==========
    $(document).ready(function () {
        // فقط در صفحاتی که نقشه وجود دارد، نقشه منیجر را مقداردهی اولیه کن
        if ($('#world-map').length) {
            MapManager.init();
        }
    });

    // ========== Expose ==========
    window.MapManager = MapManager;

})(jQuery);