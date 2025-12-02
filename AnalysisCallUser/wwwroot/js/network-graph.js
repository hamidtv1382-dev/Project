(function ($) {
    'use strict';

    const NetworkGraphManager = {
        network: null,
        nodes: null,
        edges: null,
        container: null,

        init: function (options = {}) {
            this.options = $.extend({
                containerId: 'network-graph',
                nodes: [],
                edges: []
            }, options);

            this.container = document.getElementById(this.options.containerId);
            if (!this.container) {
                console.error('Network graph container not found');
                return;
            }

            this.nodes = new vis.DataSet(this.options.nodes);
            this.edges = new vis.DataSet(this.options.edges);

            this.initializeNetwork();
            this.bindEvents();
        },

        initializeNetwork: function () {
            const data = {
                nodes: this.nodes,
                edges: this.edges
            };

            const options = {
                nodes: {
                    shape: 'dot',
                    size: 16,
                    font: {
                        size: 12,
                        color: '#333'
                    },
                    borderWidth: 2,
                    shadow: true
                },
                edges: {
                    width: 2,
                    color: { inherit: 'from' },
                    smooth: {
                        type: 'continuous'
                    },
                    shadow: true
                },
                physics: {
                    stabilization: false,
                    barnesHut: {
                        gravitationalConstant: -2000,
                        centralGravity: 0.3,
                        springLength: 95,
                        springConstant: 0.04,
                        damping: 0.09
                    }
                },
                interaction: {
                    hover: true,
                    tooltipDelay: 200,
                    navigationButtons: true,
                    keyboard: true
                }
            };

            this.network = new vis.Network(this.container, data, options);
        },

        bindEvents: function () {
            const self = this;

            // رویداد کلیک روی گره
            this.network.on('click', function (params) {
                if (params.nodes.length > 0) {
                    const nodeId = params.nodes[0];
                    self.onNodeClick(nodeId);
                }
            });

            // رویداد دابل کلیک روی گره
            this.network.on('doubleClick', function (params) {
                if (params.nodes.length > 0) {
                    const nodeId = params.nodes[0];
                    self.onNodeDoubleClick(nodeId);
                }
            });

            // رویداد کلیک راست روی گره
            this.network.on('oncontext', function (params) {
                params.event.preventDefault();
                if (params.nodes.length > 0) {
                    const nodeId = params.nodes[0];
                    self.onNodeRightClick(nodeId, params.event);
                }
            });

            // کنترل‌های زوم
            $('#zoom-in').on('click', function () {
                const scale = self.network.getScale();
                self.network.moveTo({ scale: scale * 1.2 });
            });

            $('#zoom-out').on('click', function () {
                const scale = self.network.getScale();
                self.network.moveTo({ scale: scale * 0.8 });
            });

            $('#zoom-fit').on('click', function () {
                self.network.fit();
            });

            $('#toggle-physics').on('click', function () {
                const physicsEnabled = self.network.physics.physicsEnabled;
                self.network.setOptions({ physics: { enabled: !physicsEnabled } });
                $(this).toggleClass('active');
            });

            // رویداد به‌روزرسانی شبکه
            $('#update-network').on('click', function () {
                self.updateNetwork();
            });

            // رویداد خروجی شبکه
            $('#export-network').on('click', function () {
                self.exportNetwork();
            });
        },

        onNodeClick: function (nodeId) {
            const node = this.nodes.get(nodeId);
            this.showNodeDetails(node);
        },

        onNodeDoubleClick: function (nodeId) {
            this.expandNetwork(nodeId);
        },

        onNodeRightClick: function (nodeId, event) {
            this.showContextMenu(nodeId, event);
        },

        showNodeDetails: function (node) {
            const connectedNodes = this.network.getConnectedNodes(nodeId);
            const connectedEdges = this.network.getConnectedEdges(nodeId);

            $('#node-details-content').html(`
                <div class="row">
                    <div class="col-md-6">
                        <table class="table table-borderless">
                            <tr>
                                <th width="40%">شناسه:</th>
                                <td>${node.id}</td>
                            </tr>
                            <tr>
                                <th>برچسب:</th>
                                <td>${node.label}</td>
                            </tr>
                            <tr>
                                <th>اندازه:</th>
                                <td>${node.size}</td>
                            </tr>
                            <tr>
                                <th>رنگ:</th>
                                <td><span style="display:inline-block;width:20px;height:20px;background-color:${node.color};"></span></td>
                            </tr>
                        </table>
                    </div>
                    <div class="col-md-6">
                        <table class="table table-borderless">
                            <tr>
                                <th width="40%">تعداد ارتباطات:</th>
                                <td>${connectedNodes.length}</td>
                            </tr>
                            <tr>
                                <th>درجه ورودی:</th>
                                <td>${this.getInDegree(nodeId)}</td>
                            </tr>
                            <tr>
                                <th>درجه خروجی:</th>
                                <td>${this.getOutDegree(nodeId)}</td>
                            </tr>
                            <tr>
                                <th>ضریب خوشگی:</th>
                                <td>${this.calculateClusteringCoefficient(nodeId).toFixed(3)}</td>
                            </tr>
                        </table>
                    </div>
                </div>
            `);

            $('#nodeDetailsModal').modal('show');
        },

        expandNetwork: function (nodeId) {
            const maxDepth = parseInt($('#max-depth').val());
            const currentDepth = parseInt($('#current-depth').val() || '1');

            if (currentDepth >= maxDepth) {
                Utils.showToast('حداکثر عمق تحلیل رسیده است', 'warning');
                return;
            }

            const self = this;
            Utils.setButtonLoading('#expand-node', true);

            Utils.ajax({
                url: App.apiBaseUrl + 'analytics/expandNetwork',
                data: JSON.stringify({
                    nodeId: nodeId,
                    maxDepth: maxDepth,
                    currentDepth: currentDepth
                })
            })
                .done(function (data) {
                    self.nodes.add(data.nodes);
                    self.edges.add(data.edges);
                    self.updateNetworkStats();
                    $('#current-depth').val(currentDepth + 1);
                    $('#nodeDetailsModal').modal('hide');
                })
                .fail(function () {
                    Utils.showToast('خطا در گسترش شبکه', 'danger');
                })
                .always(function () {
                    Utils.setButtonLoading('#expand-node', false);
                });
        },

        updateNetwork: function () {
            const self = this;
            const phoneNumbers = $('#phone-numbers').val()
                .split('\n')
                .map(line => line.trim())
                .filter(line => line.length > 0);

            const maxDepth = parseInt($('#max-depth').val());
            const nodeSize = $('#node-size').val();
            const layout = $('#layout').val();

            if (phoneNumbers.length === 0) {
                Utils.showToast('حداقل یک شماره تلفن وارد کنید', 'warning');
                return;
            }

            Utils.setButtonLoading('#analyze-network', true);

            Utils.ajax({
                url: App.apiBaseUrl + 'analytics/analyzeNetwork',
                data: JSON.stringify({
                    phoneNumbers: phoneNumbers,
                    maxDepth: maxDepth,
                    nodeSize: nodeSize,
                    layout: layout
                })
            })
                .done(function (data) {
                    self.nodes.clear();
                    self.edges.clear();
                    self.nodes.add(data.nodes);
                    self.edges.add(data.edges);
                    self.updateNetworkOptions(layout);
                    self.updateNetworkStats();
                    $('#current-depth').val('1');
                })
                .fail(function () {
                    Utils.showToast('خطا در تحلیل شبکه', 'danger');
                })
                .always(function () {
                    Utils.setButtonLoading('#analyze-network', false);
                });
        },

        updateNetworkOptions: function (layout) {
            let options = {
                nodes: {
                    shape: 'dot',
                    size: 16,
                    font: {
                        size: 12,
                        color: '#333'
                    },
                    borderWidth: 2,
                    shadow: true
                },
                edges: {
                    width: 2,
                    color: { inherit: 'from' },
                    smooth: {
                        type: 'continuous'
                    },
                    shadow: true
                }
            };

            if (layout === 'hierarchical') {
                options.layout = {
                    hierarchical: {
                        direction: 'UD',
                        sortMethod: 'directed'
                    }
                };
                options.physics = false;
            } else if (layout === 'circular') {
                options.layout = {
                    randomSeed: 2
                };
                options.physics = {
                    stabilization: {
                        iterations: 300
                    }
                };
            } else {
                options.physics = {
                    stabilization: false,
                    barnesHut: {
                        gravitationalConstant: -2000,
                        centralGravity: 0.3,
                        springLength: 95,
                        springConstant: 0.04,
                        damping: 0.09
                    }
                };
            }

            this.network.setOptions(options);
        },

        updateNetworkStats: function () {
            const nodeCount = this.nodes.length;
            const edgeCount = this.edges.length;

            $('#node-count').text(nodeCount);
            $('#edge-count').text(edgeCount);

            if (nodeCount > 1) {
                const maxEdges = nodeCount * (nodeCount - 1) / 2;
                const density = (edgeCount / maxEdges * 100).toFixed(1);
                $('#network-density').text(density + '%');

                const avgDegree = (2 * edgeCount / nodeCount).toFixed(2);
                $('#avg-degree').text(avgDegree);
            } else {
                $('#network-density').text('0%');
                $('#avg-degree').text('0');
            }
        },

        getInDegree: function (nodeId) {
            const connectedEdges = this.network.getConnectedEdges(nodeId);
            return connectedEdges.filter(edgeId => {
                const edge = this.edges.get(edgeId);
                return edge.to === nodeId;
            }).length;
        },

        getOutDegree: function (nodeId) {
            const connectedEdges = this.network.getConnectedEdges(nodeId);
            return connectedEdges.filter(edgeId => {
                const edge = this.edges.get(edgeId);
                return edge.from === nodeId;
            }).length;
        },

        calculateClusteringCoefficient: function (nodeId) {
            const neighbors = this.network.getConnectedNodes(nodeId);
            if (neighbors.length < 2) return 0;

            let connections = 0;
            for (let i = 0; i < neighbors.length; i++) {
                for (let j = i + 1; j < neighbors.length; j++) {
                    if (this.network.isConnected(neighbors[i], neighbors[j])) {
                        connections++;
                    }
                }
            }

            return (2 * connections) / (neighbors.length * (neighbors.length - 1));
        },

        exportNetwork: function () {
            html2canvas(this.container).then(canvas => {
                const link = document.createElement('a');
                link.download = 'network-graph.png';
                link.href = canvas.toDataURL();
                link.click();
            });
        },

        showContextMenu: function (nodeId, event) {
            // نمایش منوی زمینه (می‌توانید از کتابخانه‌های موجود استفاده کنید)
            console.log('Show context menu for node:', nodeId, 'at:', event.clientX, event.clientY);
        }
    };

    // ========== Initialization ==========
    $(document).ready(function () {
        // فقط در صفحاتی که نمودار شبکه وجود دارد، مدیریت‌کننده شبکه را مقداردهی اولیه کن
        if ($('#network-graph').length) {
            NetworkGraphManager.init();
        }
    });

    // ========== Expose ==========
    window.NetworkGraphManager = NetworkGraphManager;

})(jQuery);