app.directive('chartBar', ["$rootScope", function ($rootScope) {
    return {
        restrict: 'EA',
        replace: true,
        templateUrl: 'Scripts/app/partials/chartBar.html',
        scope: {
            name: "=",
            labels: '=',
            allData: '=',
            options: '=',
            showValue: '=',
            chartHeight: '='
        },
        link: function (scope, element, attrs, ngModelCtrl) {

            scope.warpperWidth = window.innerWidth - 200;
            var ctx;
            var chart;

            scope.$on('$destroy', function () {
                if (chart != null)
                    chart.destroy();
                for (var i in scope) {
                    if (i.indexOf('$') == 1)
                        scope[i] = null;
                }
                chart = null
            })

            $rootScope.$on('initChartBar', function (event, chartName, maxWidth) {

                if (scope.name == chartName) {
                    scope.warpperWidth = window.innerWidth - 200;
                    init();
                }

            });

            $rootScope.$on('resizeWindow', function (event, width) {
                if (scope.warpperWidth != (window.innerWidth - 200)) {
                        scope.warpperWidth = window.innerWidth - 200;
                        var chartWidth = scope.allData && (scope.allData.length * 100 > scope.warpperWidth) ? scope.allData.length * 100 : scope.warpperWidth;
                        var is_iPad = navigator.userAgent.match(/iPad/i) != null;
                        chart.chart.width = chartWidth > 11000 && is_iPad ? 11000 : chartWidth;
                        chart.draw();
                        angular.element('#chartcanvas').trigger('click');
                }
            });


            var init = function () {

                if (!scope.options)
                    return;
               
                if (chart != null)
                    chart.destroy();
                var divContainer = document.getElementById('chartAreaWrapper-' + scope.name);
                canvas = document.getElementById(scope.name);
                if (canvas) $(canvas).remove();
                canvas = document.createElement('canvas');
                canvas.id = scope.name;
                
                divContainer.appendChild(canvas);
                ctx = canvas.getContext("2d");
                var chartWidth = scope.allData && (scope.allData.length * 100 > scope.warpperWidth) ? scope.allData.length * 100 : scope.warpperWidth;
                var is_iPad = navigator.userAgent.match(/iPad/i) != null;
                ctx.canvas.width = chartWidth > 11000 ? 11000 : chartWidth;
                ctx.canvas.height = scope.chartHeight ? scope.chartHeight : 300;
                var data = {
                    labels: scope.labels,
                    datasets: [
                        {

                            label: scope.labels,
                            fillColor: scope.options.fillColor,
                            strokeColor: scope.options.strokeColor,
                            pointColor: scope.options.pointColor,
                            barStrokeWidth: scope.options.barStrokeWidth,
                            barValueSpacing: scope.options.barValueSpacing,
                            barDatasetSpacing: scope.options.barValueSpacing,
                            data: scope.allData.filter(function (x) { return x.sectorName != '' })
                        }
                    ]
                }; 

                chart = new Chart(ctx).Bar(data, {
                    showTooltips: true,
                    scaleLabel: scope.options.scaleLabel ? scope.options.scaleLabel : "<%=value%>",
                    onAnimationComplete: function () {

                        var sourceCanvas = this.chart.ctx.canvas;
                        var copyWidth = this.scale.xScalePaddingLeft - 5;
                        // the +5 is so that the bottommost y axis label is not clipped off
                        // we could factor this in using measureText if we wanted to be generic
                        var copyHeight = this.scale.endPoint + 5;
                        //if (document.getElementById("myChartAxis")) {
                        //    var targetCtx = document.getElementById("myChartAxis").getContext("2d");
                        //    targetCtx.canvas.width = copyWidth;
                        //    targetCtx.drawImage(sourceCanvas, 0, 0, copyWidth, copyHeight, 0, 0, copyWidth, copyHeight);
                        //}

                        var chartObj = this;

                        drawLabels(chartObj);

                        var drawOrig = angular.bind(this, this.draw);
                        this.draw = function ()
                        {
                            drawOrig();
                            drawLabels(chartObj);
                        }

                        function drawLabels(chartObj)
                        {
                            var ctx1 = chartObj.chart.ctx;
                            ctx1.font = chartObj.scale.font;
                            ctx1.fillStyle = chartObj.scale.textColor
                            ctx1.textAlign = "center";
                            ctx1.textBaseline = "bottom";

                            if (!scope.showValue)
                                return;
                            chartObj.datasets.forEach(function (dataset) {
                                dataset.bars.forEach(function (points) {
                                    ctx1.fillText(points.value + '%', points.x, points.y - 10);
                                });
                            })
                        }
                    }
                });


            }

        }
    }
}]);