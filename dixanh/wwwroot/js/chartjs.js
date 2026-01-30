// dashboardCharts.js
// Requires Chart.js (global Chart)
(function () {
    const charts = {};

    function ensureChartJs() {
        if (typeof window.Chart === "undefined") {
            console.error("Chart.js chưa được load. Hãy include Chart.js trước dashboardCharts.js");
            return false;
        }
        return true;
    }

    function getCanvas(id) {
        const el = document.getElementById(id);
        return el || null;
    }

    function destroy(id) {
        const c = charts[id];
        if (c) {
            c.destroy();
            delete charts[id];
        }
    }

    function baseOptions() {
        return {
            responsive: true,
            maintainAspectRatio: false,
            animation: { duration: 250 }
        };
    }

    function toPercents(values) {
        const nums = (values || []).map(v => Number(v) || 0);
        const sum = nums.reduce((a, b) => a + b, 0);
        if (!sum) return nums.map(() => 0);
        return nums.map(v => Math.round((v / sum) * 1000) / 10); // 1 decimal
    }

    // PIE
    function renderPie(canvasId, labels, values, opts) {
        if (!ensureChartJs()) return;
        const el = getCanvas(canvasId);
        if (!el) return;

        destroy(canvasId);

        const safeLabels = labels || [];
        const safeValues = (values || []).map(v => Number(v) || 0);
        const percents = toPercents(safeValues);

        opts = opts || {};
        const showLegend = (opts.showLegend ?? false);
        const legendPosition = (opts.legendPosition ?? "bottom");
        const valueSuffix = (opts.valueSuffix ?? "");
        const showPercent = (opts.showPercent ?? true);

        charts[canvasId] = new window.Chart(el, {
            type: "pie",
            data: {
                labels: safeLabels,
                datasets: [{ data: safeValues, borderWidth: 1 }]
            },
            options: Object.assign({}, baseOptions(), {
                plugins: {
                    legend: {
                        display: showLegend,
                        position: legendPosition,
                        labels: { boxWidth: 10, boxHeight: 10 }
                    },
                    tooltip: {
                        callbacks: {
                            label: (ctx) => {
                                const label = ctx.label || "";
                                const v = (ctx.parsed ?? 0);
                                const i = (ctx.dataIndex ?? 0);
                                const p = (percents[i] ?? 0);
                                return showPercent
                                    ? `${label}: ${v}${valueSuffix} (${p}%)`
                                    : `${label}: ${v}${valueSuffix}`;
                            }
                        }
                    }
                }
            })
        });
    }

    // BAR
    function renderBar(canvasId, labels, values, opts) {
        if (!ensureChartJs()) return;
        const el = getCanvas(canvasId);
        if (!el) return;

        destroy(canvasId);

        const safeLabels = labels || [];
        const safeValues = (values || []).map(v => Number(v) || 0);

        opts = opts || {};
        const title = (opts.title ?? "");
        const showLegend = (opts.showLegend ?? !!title);
        const yBeginAtZero = (opts.yBeginAtZero ?? true);
        const valueSuffix = (opts.valueSuffix ?? "");
        const maxXTicks = (opts.maxXTicks ?? 12);
        const maxYTicks = (opts.maxYTicks ?? 6);

        charts[canvasId] = new window.Chart(el, {
            type: "bar",
            data: {
                labels: safeLabels,
                datasets: [{ label: title, data: safeValues, borderWidth: 1 }]
            },
            options: Object.assign({}, baseOptions(), {
                plugins: { legend: { display: showLegend } },
                scales: {
                    x: {
                        ticks: { maxTicksLimit: maxXTicks },
                        grid: { display: false }
                    },
                    y: {
                        beginAtZero: yBeginAtZero,
                        ticks: {
                            maxTicksLimit: maxYTicks,
                            callback: (v) => `${v}${valueSuffix}`
                        }
                    }
                }
            })
        });
    }

    function update(canvasId, labels, values) {
        const c = charts[canvasId];
        if (!c) return;
        c.data.labels = labels || [];
        c.data.datasets[0].data = (values || []).map(v => Number(v) || 0);
        c.update();
    }

    function destroyAll() {
        Object.keys(charts).forEach(destroy);
    }

    // ✅ expose global for Blazor JSInterop
    window.tvtCharts = { renderPie, renderBar, update, destroy, destroyAll };

    // ✅ hàm ready để C# polling
    window.tvtChartsReady = function () {
        return !!window.tvtCharts
            && typeof window.tvtCharts.renderPie === "function"
            && typeof window.tvtCharts.renderBar === "function"
            && typeof window.Chart !== "undefined";
    };

    console.log("dashboardCharts.js loaded. tvtChartsReady =", window.tvtChartsReady());
})();