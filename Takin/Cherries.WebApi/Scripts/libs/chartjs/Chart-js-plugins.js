Chart.pluginService.register({
    afterDraw: function (chart, easing) {
        if (chart.config.options && chart.config.options.scales) {
            if (chart.config.options.scales.xAxes)
                chart.config.options.scales.xAxes.forEach(function (xAxisConfig) {
                    if (!xAxisConfig.color)
                        return;

                    var ctx = chart.chart.ctx;
                    var chartArea = chart.chartArea;
                    var xAxis = chart.scales[xAxisConfig.id];

                    // just draw the scale again with different colors
                    var color = xAxisConfig.gridLines.color;
                    xAxisConfig.gridLines.color = xAxisConfig.color;
                    xAxis.draw(chartArea);
                    xAxisConfig.gridLines.color = color;
                });

            if (chart.config.options.scales.yAxes)
                chart.config.options.scales.yAxes.forEach(function (yAxisConfig) {
                    if (!yAxisConfig.color)
                        return;

                    var ctx = chart.chart.ctx;
                    var chartArea = chart.chartArea;
                    var yAxis = chart.scales[yAxisConfig.id];

                    // here, since we also have the grid lines, set a clip area for the left of the y axis
                    ctx.save();
                    ctx.rect(0, 0, chartArea.left + yAxisConfig.gridLines.lineWidth - 1, chartArea.bottom + yAxisConfig.gridLines.lineWidth - 1);
                    ctx.clip();

                    var color = yAxisConfig.gridLines.color;
                    yAxisConfig.gridLines.color = yAxisConfig.color;
                    yAxis.draw(chartArea);
                    yAxisConfig.gridLines.color = color;

                    ctx.restore();
                });

            // we need to draw the tooltip so that it comes over the (redrawn) elements
            chart.tooltip.transition(easing).draw();
        }
    }
});