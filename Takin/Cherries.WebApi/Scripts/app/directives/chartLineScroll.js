app.directive('chartLineScroll', ["$modal", "utilitiesSvc", "$location", "$window", "$rootScope", function ($modal, utilitiesSvc, $location, $window, $rootScope) {
    return {
        restrict: 'EA',
        replace: true,
        templateUrl: 'Scripts/app/partials/chartLineScroll.min.html',
        scope: {
            name: '=',
            labels: '=',
            allData: '=',
            options: '=',
            chartWidth: '=',
            showTooltip: '=',
            labelTags: '=',
            selectPointEvent: '&'
        },
        link: function (scope, element, attrs, ngModelCtrl) {

            var chart;
            var ctx;
            var dataSource;
            scope.maxChartWidth;
            scope.$on('$destroy', function () {
                if (chart != null)
                    chart.destroy();
                for (var i in scope) {
                    if (i.indexOf('$') == 1)
                        scope[i] = null;
                }
                chart = null
            })
            $rootScope.$on('initChartLineScroll', function (event, chartName, maxWidth) {

                if (scope.name == chartName) {

                    if (!scope.chartWidth && scope.warpperWidth != (maxWidth - 50)) {
                        scope.flag = false;
                        scope.warpperWidth = maxWidth - 50;
                        scope.maxChartWidth = (window.innerWidth - 200);
                        init();
                        angular.element('#chartlinecanvas').trigger('click');
                    }
                }

            });

            var init = function () {
               if (!scope.options || scope.flag || !document.getElementById(scope.name))
                    return;
                scope.flag = true;
                dataSource = scope.allData;
                ctx = document.getElementById(scope.name).getContext("2d");
                var chartWidth = scope.allData && (scope.allData.length * 100 > scope.maxChartWidth) ? scope.allData.length * 100 : scope.maxChartWidth;
                var is_iPad = navigator.userAgent.match(/iPad/i) != null;
                ctx.canvas.width = chartWidth > 11000 && is_iPad ? 11000 : chartWidth;
                ctx.canvas.height = scope.chartHeight ? scope.chartHeight : 300;
                var data = {
                    labels: scope.labels,
                    labelTags: scope.labelTags,
                    datasets: [
                        {
                            label: scope.labels,
                            labelTags: scope.labelTags,
                            fillColor: scope.options.fillColor,
                            strokeColor: scope.options.strokeColor,
                            pointColor: scope.options.pointColor,
                            data: scope.allData.filter(function (x) { return x.sectorName != '' })
                        }
                    ]
                };

                scope.options.onAnimationProgress = function () { var self = this; setTimeout(function () { drawDatasetPointsLabels(self); }, 1); }
                scope.options.onAnimationComplete = function () { var self = this; drawYAxis(self); setTimeout(function () { drawDatasetPointsLabels(self); }, 1); }
                var canvas = document.getElementById(scope.name);

                canvas.onmousemove = function (evt) {
                    var self = this;
                    setTimeout(function () { drawDatasetPointsLabels(self); }, 1);
                }

                canvas.onmouseover = function (evt) {
                    var self = this;
                    setTimeout(function () { drawDatasetPointsLabels(self); }, 1);
                }

                canvas.onmouseenter = function (evt) {
                    var self = this;
                    setTimeout(function () { drawDatasetPointsLabels(self); }, 1);
                }

                canvas.onmouseout = function (evt) {
                    var self = this;
                    setTimeout(function () { drawDatasetPointsLabels(self); }, 1);
                }

                chart = new Chart(ctx).Line(data, scope.options);
            }

            
            init();

            function drawYAxis(obj) {
                if (obj.scale != undefined) {
                    var sourceCanvas = obj.chart.ctx.canvas;
//                    var copyWidth = obj.scale.xScalePaddingLeft + 5;
                    var copyWidth = obj.scale.xScalePaddingLeft - 4;
                    // the +5 is so that the bottommost y axis label is not clipped off
                    // we could factor this in using measureText if we wanted to be generic
                    var copyHeight = obj.scale.height - 16;
                    var targetCtx = document.getElementById(scope.name + "Axis").getContext("2d");
                    var targetContainer = element.find(".chartWrapper__axis");
                    targetContainer.width(copyWidth);
                    targetContainer.height(copyHeight);
                    targetCtx.canvas.width = copyWidth;
//                    targetCtx.canvas.height = sourceCanvas.height;
                    targetCtx.canvas.height = copyHeight;

                    var imageData = obj.chart.ctx.getImageData(0, 0, copyWidth, copyHeight);
                    targetCtx.putImageData(imageData, 0, 0);
//                    targetCtx.drawImage(sourceCanvas.toDataURL(), 0, 0, copyWidth, copyHeight);
//                    targetCtx.beginPath();
//                    targetCtx.moveTo(0, 0);
//                    targetCtx.lineTo(0, copyHeight);
//                    targetCtx.strokeStyle = '#000000';
//                    targetCtx.stroke();
                }
            }

            function drawDatasetPointsLabels(obj) {
                
                ctx.font = '16px "Gotham Book",sans-serif';
                ctx.fontWeight = 'bold';
                ctx.fillStyle = '#000';
                ctx.textAlign = "center";

                $(chart.datasets).each(function (idx, dataset) {
                    $(dataset.points).each(function (pdx, pointinfo) {
                        var label = dataset.labelTags[pdx];
                        if (label) {
                                if (pointinfo.y > 230 ){
                                ctx.fillText(label, pointinfo.x, pointinfo.y - 45);
                                ctx.beginPath();
                                ctx.moveTo(pointinfo.x, pointinfo.y - 30);
                                ctx.lineTo(pointinfo.x, pointinfo.y - 2);
                            }
                            else {
                                ctx.fillText(label, pointinfo.x, pointinfo.y + 45);
                                ctx.beginPath();
                                //ctx.setLineDash([1, 3]);
                                ctx.moveTo(pointinfo.x, pointinfo.y + 30);
                                ctx.lineTo(pointinfo.x, pointinfo.y + 2);
                            }
                            ctx.strokeStyle = '#000000';
                            ctx.stroke();
                        }
                    });
                });

                
            }
        }
    };
}]);