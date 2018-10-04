app.directive('chartPie', ["$modal", "utilitiesSvc", "$location", "$window", "$rootScope", function ($modal, utilitiesSvc, $location, $window, $rootScope) {
    return {
        restrict: 'EA',
        replace: true,
        templateUrl: 'Scripts/app/partials/chartPie.min.html',
        scope: {
            name: '=',
            labels: '=',
            data: '=',
            options: '=',
            chartWidth: '=',
            chartHeight: '='
        },
        link: function (scope, element, attrs, ngModelCtrl) {

            var totalValue;
            var myPieChart;
            var midX;
            var midY;
            var ctx;
            var canvas;
            scope.$on('$destroy', function () {
                if (myPieChart != null)
                    myPieChart.destroy();
                for (var i in scope) {
                    if (i.indexOf('$') == 1)
                        scope[i] = null;
                }
                myPieChart = null
            })
            $rootScope.$on('initChartPie', function (event, chartName) {

                if (scope.name == chartName)
                    init();

            });

            scope.seriesClick = function (index) {

                for (var i = 0; i < scope.options.series.length; i++) {
                    scope.options.series[i].select = false;
                }

                scope.options.series[index].select = true;
                drawSegmentValues();
            }

            var selectPoint = function (evt) {
                if (!myPieChart)
                    return;

                var points = myPieChart.getSegmentsAtEvent(evt);
                var index = myPieChart.segments.indexOf(points[0]);

                // scope.options.ySelect = points[0].value;
                // scope.options.xSelect = Number(points[0].label);
                for (var i = 0; i < scope.options.series.length; i++) {
                    scope.options.series[i].select = false;
                }

                scope.options.series[index].select = true;

                return;



            };

            var init = function () {
                if (myPieChart != null)
                    myPieChart.destroy();
                var divContainer = document.getElementById('container-' + scope.name);
                canvas = document.getElementById(scope.name);
                if (canvas) $(canvas).remove();
                canvas = document.createElement('canvas');
                canvas.id = scope.name;
                canvas.style.marginLeft = "-20px";
                divContainer.appendChild(canvas);
                ctx = canvas.getContext("2d");
                ctx.clearRect(0, 0, canvas.width, canvas.height);
                ctx.canvas.width = scope.chartWidth ? scope.chartWidth : 350;
                ctx.canvas.height = scope.chartHeight ? scope.chartHeight : 250;
                midX = canvas.width / 2;
                midY = canvas.height / 2
                totalValue = getTotalValue(scope.data);
                canvas.onclick = selectPoint;
                scope.options.showTooltips = true;
                scope.options.tooltipTemplate = "<%if (label){%><%=label %>: <%}%><%= Math.floor(value) + ' %' %>";
                scope.options.onAnimationProgress = drawSegmentValues;

                // Create a pie chart
                myPieChart = new Chart(ctx).Pie(scope.data, scope.options);

            }

            var getTextSize = function (value, isBold) {

               
                return 8;

            }

            var drawSegmentValues = function () {

                for (var i = 0; i < myPieChart.segments.length; i++) {
                    drawSegment(myPieChart.segments[i], scope.options.series[i].select)
                }
            }

            var drawSegment = function (segment, isBold) {
                var radius = myPieChart.outerRadius;
                ctx.fillStyle = "black";
                var textSize = getTextSize(segment.value, isBold);
                ctx.font = textSize + "px Verdana";
                // Get needed variables
                var value = Math.floor(segment.value);
                var startAngle = segment.startAngle;
                var endAngle = segment.endAngle;
                var middleAngle = startAngle + ((endAngle - startAngle) / 2);

                // Compute text location
                var posX = (radius / 0.98) * Math.cos(middleAngle) + midX;
                var posY = (radius / 0.98) * Math.sin(middleAngle) + midY;

                // Text offside by middle
                var w_offset = ctx.measureText(value).width / 2;
                var h_offset = textSize / 2;

                //ctx.fillText(value + '%', posX - w_offset, posY + h_offset);
            }

            var getTotalValue = function (arr) {
                if (!arr)
                    return 0;
                var total = 0;
                for (var i = 0; i < arr.length; i++)
                    total += arr[i].value;
                return total;
            }

        }

    }

}]);