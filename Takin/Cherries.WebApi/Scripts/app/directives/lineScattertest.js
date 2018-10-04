app.directive('lineScattertest', ["$modal", "utilitiesSvc", "$location", "$window", "$rootScope", "underscore",
    function ($modal, utilitiesSvc, $location, $window, $rootScope, underscore)
    {
    return {
        restrict: 'EA',
        replace: true,
        templateUrl: 'Scripts/app/partials/lineScattertest.min.html',
        scope: {
            labels: '=',
            allData: '=',
            options: '=',
            chartWidth: '=',
            multiLines:'=',
            selectPointEvent: '&'
        },
        link: function (scope, element, attrs, ngModelCtrl) {

            var chart;
            var ctx;
            var canvas;
            var bgCanvas = element.find('.bg-canvas')[0];
            scope.flag;
            scope.isInit = true;
            scope.$on('$destroy', function () {
                if (chart != null)
                    chart.destroy();
                for (var i in scope) {
                    if (i.indexOf('$') == 1)
                        scope[i] = null;
                }
                chart = null
            });
            $rootScope.$on('initLineScattertest', function (event, refreshAfterReload) {

                scope.warpperWidth = window.innerWidth - 250;
                init(refreshAfterReload);

            });

            $rootScope.$on('resizeWindow', function (event, width) {
                if (scope.warpperWidth != (window.innerWidth - 250)) {
                    scope.warpperWidth = window.innerWidth - 250;
                    if (!chart)
                        return;
                    chart.destroy();
                    buildChart();
                    var prevWidth = bgCanvas.width - 68;
                    var currentWidth = (scope.chartWidth ? scope.chartWidth : scope.warpperWidth) - 68;
                    if (currentSelectionX != null)
                    {
                        currentSelectionX = ((currentSelectionX - 40) * (currentWidth / prevWidth)) + 40;
                        drawSelectionLine(currentSelectionX);
                    }
                }
            });

            var init = function (refreshAfterReload) {

                if (scope.flag)
                    return;
                scope.flag = true;
                buildChart(refreshAfterReload);
            }

            var currentPoint = null;
            var currentSelectionX = null;

            var selectPoint = function (evt) {
                if (!chart)
                    return;

                var points = chart.getPointsAtEvent(evt);
                var index = chart.datasets[0].points.indexOf(points[0]);

                if (scope.options.selectionLine && points.length > 0)
                {

                    currentPoint = points[0];
                    currentSelectionX = currentPoint.x;
                    drawSelectionLine(currentSelectionX);
                }

                for (var i = 0; i < chart.datasets.length; i++) {
                    if (chart.datasets[i].points.indexOf(points[0]) > -1) {
                        scope.options.ySelect = points[0].value;
                        scope.options.xSelect = Number(points[0].label);
                        scope.selectPointEvent({ index: chart.datasets[i].points.indexOf(points[0]), indexline: i });
                        chart.destroy();
                        buildChart();
                        return;
                    }
                }
            };

            function drawSelectionLine(x)
            {
                bgCanvas.height = '450';
                bgCanvas.width = scope.chartWidth ? scope.chartWidth : scope.warpperWidth;

                var bgCtx = bgCanvas.getContext('2d');
                bgCtx.strokeStyle = '#ff0000';
                bgCtx.lineWidth = 2;
                bgCtx.beginPath();
                bgCtx.moveTo(x, 0);
                bgCtx.lineTo(x, canvas.height);
                bgCtx.stroke();
            }

            var buildChart = function (refreshAfterReload) {
                if (chart != null)
                    chart.destroy();
                var divContainer = document.getElementById('container-lsChart');
                canvas = document.getElementById("lsChart");
                if (canvas) $(canvas).remove();
                canvas = document.createElement('canvas');
                canvas.id = "lsChart";
                canvas.height = "450";
                angular.element(canvas).css('position', 'relative');
                divContainer.appendChild(canvas);
                ctx = canvas.getContext("2d");
                ctx.canvas.width = scope.chartWidth ? scope.chartWidth : scope.warpperWidth;
                scope.options.onAnimationProgress = function () { setTimeout(function () { drawDatasetPointsLabels(); }, 0); }
                scope.options.onAnimationComplete = function () { setTimeout(function () { drawDatasetPointsLabels(); }, 0); }

                
                canvas.onclick = selectPoint;
                canvas.onmousemove = underscore.throttle(mouseMoveEventHandler, 100, { leading: false });

                function mouseMoveEventHandler(evt)
                {
                    var point = chart.getPointsAtEvent(evt);
                    if (point && point.length > 0)
                    {
                        drawDatasetPointsLabels(true);
                    }
                    else
                        drawDatasetPointsLabels();
                }

                canvas.onmouseover = function (evt) {
                    setTimeout(function () { drawDatasetPointsLabels(true); }, 0);
                }

                canvas.onmouseenter = function (evt) {
                    setTimeout(function () { drawDatasetPointsLabels(true); }, 0);
                }

                canvas.onmouseout = function (evt) {
                    setTimeout(function () { drawDatasetPointsLabels(); }, 0);
                }
                chart = new Chart(ctx).Scatter(scope.allData, scope.options);

                setTimeout(function () {
                    if (refreshAfterReload) {
                        chart.draw();
                        angular.element('#chartScatchercanvas').trigger('click');
                    }
                }, 100)
            }



            function drawDatasetPointsLabels(showPointLine) {
                ctx.font = '16px "Gotham Book",sans-serif';
                ctx.fontWeight = 'bold';
                ctx.fillStyle = '#000';
                ctx.textAlign = "center";

                $(chart.datasets).each(function (idx, dataset) {
                    $(dataset.points).each(function (pdx, pointinfo) {
                        //pointinfo.valueLabel = pointinfo.valueLabel + '%';
                        //ctx.fillText(pointinfo.value + '%', pointinfo.x, pointinfo.y - 10);
                        if (pointinfo.tagLabel !== undefined) {
                            if (pointinfo.tagLabel != 'overPoint') {

                                if (pointinfo.tagLabel && pointinfo.tagLabel.indexOf('Opt') > -1 && scope.isInit) {
                                    scope.options.ySelect = pointinfo.value;
                                    scope.options.xSelect = Number(pointinfo.label);
                                    scope.selectPointEvent({ index: pdx, indexline: idx });
                                    scope.isInit = false;
                                    chart.destroy();
                                    buildChart();
                                }
                                var pointXTag = pointinfo.x;
                                //if (chart.datasets[0].points.length - 1 == pdx)
                                //    pointXTag = scope.options.leftTag ? pointXTag - 15 : pointXTag - 25;
                                
                                if (scope.options.tagWithPath) {
                                    ctx.beginPath();
                                    var offset = 0;
                                    //ctx.setLineDash([1, 3]);
                                    if (pointinfo.y > 60)
                                    {
                                        ctx.fillText(pointinfo.tagLabel, pointXTag, scope.options.leftTag ? pointinfo.y + 20 : pointinfo.y - 50);
                                        ctx.moveTo(pointXTag, pointinfo.y - 40);
                                        ctx.lineTo(pointXTag, pointinfo.y - 5);
                                    }
                                    else {
                                        ctx.fillText(pointinfo.tagLabel, pointXTag, scope.options.leftTag ? pointinfo.y - 20 : pointinfo.y + 50);
                                        ctx.moveTo(pointXTag, pointinfo.y + 40);
                                        ctx.lineTo(pointXTag, pointinfo.y + 5);
                                    }
                                    ctx.strokeStyle = '#000000';
                                    ctx.stroke();
                                }
                            }
                            else if (showPointLine) {
                                ctx.beginPath();
                                var pointXTag = pointinfo.x;
                                var pointYTag = pointinfo.y;
                                //ctx.setLineDash([1, 3]);
                                ctx.moveTo(pointXTag, pointinfo.y);
                                ctx.lineTo(pointXTag, pointinfo.y + (ctx.canvas.height - pointinfo.y -30));
                                ctx.strokeStyle = '#000000';
                                ctx.stroke();

                                ctx.beginPath();
                                //ctx.setLineDash([1, 3]);
                                ctx.moveTo(pointXTag, pointinfo.y);
                                ctx.lineTo(45, pointYTag);
                                ctx.strokeStyle = '#000000';
                                ctx.stroke();

                            }
                        }

                    });
                });
            }

            $rootScope.$on('updateScetcherTagsAndRedPoint', function (event, index) {
                scope.isInit = true
                scope.flag = false;

            });
        }
    };
}]);