app.directive('security', ["$location", "$modal", "utilitiesSvc", "generalSvc", "$filter", "priceSvc", "$rootScope", function ($location, $modal, utilitiesSvc, generalSvc, $filter, priceSvc, $rootScope) {
    return {
        restrict: 'E',
        templateUrl: 'scripts/app/partials/security.html',
        scope: {
            model: '=',
            index: '=',
            columns: '=',
            mobileColumns: '=',
            riskColorField: '=',
            disableMode:'=',
            canSelect: '=',
            chartWidth: '=',
            isFull: '=',
            selectModel: '&',
            currency: '=',
            isLast:'='
        },
        compile: compileDirective
    }

    function compileDirective(tElement)
    {
        tElement.addClass('security');
        return linkDirective;
    }

    function linkDirective(scope, element, attrs, ngModelCtrl)
    {
            if (!scope.model) scope.model = {};
            scope.isNIS = scope.currency != '9001';
            //scope.canSelect = scope.canSelect == 'true' ? true : false;

            scope.model.AvgYield = scope.isNIS ? scope.model.AvgYieldNIS : scope.model.AvgYield
            scope.riskColors = riskColors;

            var rateData;

            scope.changeModelSelect = function () {

                if (scope.selectModel)
                    scope.selectModel({ model: scope.model });
            }

            scope.showInfo = function () {

                if (scope.showPrices) {
                    scope.showPrices = false;
                    return;
                }

                if (scope.lineOptions != null) {
                    scope.showPrices = true;
                    return;
                }
                priceSvc.getPrices({
                    'secId': scope.model.idSecurity, 'currency': scope.currency }).$promise.then(function (data) {
                    rateData = data.Rates;
                    initChartLine(data.Rates);
                    scope.showPrices = true;
                    setTimeout(function () {
                        var secWidth = document.getElementById("sec").offsetWidth;

                        $rootScope.$broadcast('initChartLineScroll', scope.model.idSecurity, secWidth);
                    }, 100);


                    // initChartLine(data.Rates);
                });
            }

            scope.updatechartSize = function () {
                var a = 5;
            }

            scope.getRiskColor = function () {
                if (scope.model[scope.riskColorField]) {
                    var value = $filter('number')(scope.model[scope.riskColorField] * 100, 2) * 1;
                    switch (true) {
                        case (value >= 0 && value <= 9):
                            return scope.riskColors['Solid'];
                            break;
                        case (value > 9 && value <= 14):
                            return scope.riskColors['Low'];
                            break;
                        case (value > 14 && value <= 25):
                            return scope.riskColors['Moderate'];
                            break;
                        case (value > 25 && value <= 40):
                            return scope.riskColors['High'];
                            break;
                        case (value > 40 && value <= 100):
                            return scope.riskColors['Very High'];
                            break;
                    }
                }
                return '#ffffff';

            }

            $rootScope.$on('resizeWindow', function (event, width) {

                if (scope.rowHeight == 50 || scope.rowHeight == 60)
                    scope.rowHeight = width <= 1366 ? 60 : 50;
                if (scope.showPrices) {
                    var secWidth = document.getElementById("sec").offsetWidth;
                    $rootScope.$broadcast('initChartLineScroll', scope.model.idSecurity, secWidth);
                }
            });

            String.format = function (str) {
                var args = arguments;


                return str.replace(/{[0-9]}/g, function (matched) { args[parseInt(matched.replace(/[{}]/g, '')) + 1] });
            };

            var initChartLine = function (data) {

                //scope.chartWidth = 1000;

                scope.lineLabels = [];
                scope.lineLabelTags = [];
                scope.lineData = [];

                for (var i = 0; i < data.length; i++) {

                    var dsplit = data[i].Date.substring(0, data[i].Date.indexOf('T')).split("-");
                    var d = new Date(dsplit[0], dsplit[1] - 1, dsplit[2]);
                    var d2;

                    if (i > 0) {
                        dsplit = data[i - 1].Date.substring(0, data[i - 1].Date.indexOf('T')).split("-");
                        d2 = new Date(dsplit[0], dsplit[1] - 1, dsplit[2]);
                    }
                    scope.lineLabels.push($filter('date')(data[i].Date, "MM/dd/yyyy"));
                    scope.lineLabelTags.push(data[i].Label);
                    //if (i == 0 || d.getFullYear() != d2.getFullYear()) {

                    //    //var s = String.format("{0} years return:{2}{1} %", d.getFullYear().toString(), $filter('number')(data[i].RateVal, 2), '\n');
                    //    var s = d.getFullYear().toString() + ' years return: ' + $filter('number')(data[i].RateVal, 2);
                    //    scope.lineLabels.push(s);

                    //}
                    //else {

                    //    scope.lineLabels.push($filter('date')(data[i].Date, "dd/MM/yy"));
                    //}
                    scope.lineData.push(parseInt($filter('number')(data[i].RateVal, 2)));
                }

                scope.startDate = $filter('date')(data[0].Date, "MM/dd/yyyy");

                scope.endDate = $filter('date')(data[data.length - 1].Date, "MM/dd/yyyy");

                scope.lineOptions = {
                    showTooltips: true,
                    notRotateLabel: true,
                    tooltipTemplate: function (label, value) {

                        if (label.index != null && label.index >= 0) {
                            var obj = rateData[label.index];
                            //var s = String.format("Date : {0} \n Price: {1}", $filter('date')(obj.Date, "dd/MM/yy"), $filter('number')(obj.RateVal, 2));
                            var s = obj.Tooltip; //'Date : ' + $filter('date')(obj.Date, "dd/MM/yy") + ' Price: ' + $filter('number')(obj.RateVal, 2);
                            return s;
                        }
                        //display the clients and  Conversion in toolTip
                        // return label.label + ': ' + label.value;
                    },
                    fillColor: '#ADC0DA',
                    strokeColor: '#ADC0DA',
                    pointColor: '#FFFFFF',
                    barValueSpacing: 1,
                    barDatasetSpacing: 10,
                    scaleShowLabels: true,
                    scaleBeginAtZero: true
                    //datasetFill: false
                }
            }

            var initChartData = function (data) {

                scope.ChartLinePrices = {
                    labels: labels,
                    lineLabelTags: lineLabelTags,
                    series: ["Solid: 0%-9%", "Low: 9%-14%", "Moderate: 14%-25%", "Hight:25%-40%", " very Hight:40%-100%"],
                    data: allData,
                    options: {
                        //Boolean - Whether the scale should start at zero, or an order of magnitude down from the lowest value
                        scaleBeginAtZero: false,
                        //Boolean - Whether grid lines are shown across the chart
                        scaleShowGridLines: true,
                        //String - Colour of the grid lines
                        scaleGridLineColor: "rgba(0,0,0,.05)",
                        //Number - Width of the grid lines
                        scaleGridLineWidth: 1,
                        //Boolean - Whether to show horizontal lines (except X axis)
                        scaleShowHorizontalLines: false,
                        //Boolean - Whether to show vertical lines (except Y axis)
                        scaleShowVerticalLines: false,
                        //define y axis values
                        scaleShowLabels: true,
                        //Boolean - If there is a stroke on each bar
                        barShowStroke: true,
                        //Number - Pixel width of the bar stroke
                        barStrokeWidth: 5,
                        //Number - Spacing between each of the X value sets
                        barValueSpacing: 15,
                        //Number - Spacing between data sets within X values
                        barDatasetSpacing: 3,
                        //String - A legend template
                        legendTemplate: "<ul class=\"<%=name.toLowerCase()%>-legend\"><% for (var i=0; i<datasets.length; i++){%><li><span style=\"background-color:<%=datasets[i].strokeColor%>\"></span><%if(datasets[i].label){%><%=datasets[i].label%><%}%></li><%}%></ul>",
                        //Boolean - whether to make the chart responsive
                        responsive: true,
                        //datasetFill: false,
                        maintainAspectRatio: false,

                        scales: {
                            xAxes: [{
                                ticks: {
                                    display: false
                                }
                            }]
                        }
                    },
                    colours: [
                        { fillColor: '#ADC0DA', strokeColor: '#ADC0DA', pointColor: '#FFFFFF' },
                    ],
                };
            };

            var init = function () {
                scope.showPrices = false;
                scope.rowHeight = 45;
                scope.is_iPad = navigator.userAgent.match(/iPad/i) != null;
            };

            init();

            scope.$on('$destroy', function () {
                for (var i in scope) {
                    if (i.indexOf('$') == 1)
                        scope[i] = null;
                }
            })
    }
}]);