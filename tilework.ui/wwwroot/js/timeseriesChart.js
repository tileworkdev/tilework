const chartInstances = new WeakMap();

function ensureChartJs() {
    if (typeof Chart === "undefined") {
        console.warn("Chart.js is not available on the page.");
        return false;
    }
    return true;
}

function buildOptions(labels, color) {
    return {
        type: "line",
        data: {
            labels,
            datasets: [
                {
                    label: "",
                    data: [],
                    borderColor: color,
                    backgroundColor: color,
                    borderWidth: 1.5,
                    tension: 0.25,
                    pointRadius: 0,
                    pointHoverRadius: 4,
                    pointHitRadius: 8,
                    fill: false
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                tooltip: { mode: "nearest", intersect: false }
            },
            scales: {
                x: {
                    ticks: {
                        autoSkip: false,
                        maxRotation: 0,
                        callback: (_, index) => labels[index] ?? "",
                        font: { size: 12 }
                    },
                    grid: { display: false }
                },
                y: {
                    ticks: {
                        font: { size: 12 },
                        maxTicksLimit: 6
                    },
                    grid: { color: "rgba(0, 0, 0, 0.05)" },
                    beginAtZero: true
                }
            },
            elements: {
                line: { borderWidth: 1.5, tension: 0.25 },
                point: {
                    radius: 0,
                    hoverRadius: 4,
                    hitRadius: 8
                }
            }
        }
    };
}

export function renderTimeseriesChart(canvas, config) {
    if (!canvas || !ensureChartJs()) {
        return;
    }

    const { labels = [], data = [], color = "#1B5E20", name = "" } = config ?? {};
    const existing = chartInstances.get(canvas);

    if (existing) {
        existing.data.labels = labels;
        existing.data.datasets[0].label = name;
        existing.data.datasets[0].data = data;
        existing.update();
        return;
    }

    const ctx = canvas.getContext("2d");
    if (!ctx) {
        return;
    }

    const chartConfig = buildOptions(labels, color);
    chartConfig.data.datasets[0].data = data;
    chartConfig.data.datasets[0].label = name;

    const chart = new Chart(ctx, chartConfig);
    chartInstances.set(canvas, chart);
}

export function disposeTimeseriesChart(canvas) {
    const chart = chartInstances.get(canvas);
    if (chart) {
        chart.destroy();
        chartInstances.delete(canvas);
    }
}
