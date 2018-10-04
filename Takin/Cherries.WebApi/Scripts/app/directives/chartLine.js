app.directive('chartLine', ["$modal", "utilitiesSvc", "$location", "$window", "$rootScope", function ($modal, utilitiesSvc, $location, $window, $rootScope) {
    return {
        restrict: 'EA',
        replace: true,
        templateUrl: 'Scripts/app/partials/chartLine.min.html',
        scope: {
            name: '=',
            labels: '=',
            allData: '=',
            options: '=',
            chartWidth: '=',
            showTooltip: '='
        },
        link: function (scope, element, attrs, ngModelCtrl) {
            var chart
            $rootScope.$on('initChartLine', function (event, chartName) {

                if (scope.name == chartName)
                    init();

            });
            scope.$on('$destroy', function () {
                if (chart != null)
                    chart.destroy();
                for (var i in scope) {
                    if (i.indexOf('$') == 1)
                        scope[i] = null;
                }
                chart = null;
            })
            var init = function () {

                if (!scope.options || scope.flag)
                    return;
                scope.flag = true;
                var ctx = document.getElementById(scope.name).getContext("2d");
                ctx.canvas.width = scope.chartWidth ? scope.chartWidth : 1280;
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
                chart = new Chart(ctx).Line(data, { showTooltips: scope.showTooltip });
                

                init();
            }
        }
    }
}]);
