app.controller("backtestingCtrl", ['$scope', '$rootScope', "$location", "$window", "ngTableParams", "backtestingSvc", '$routeParams', '$filter', 'utilitiesSvc',
    function ($scope, $rootScope, $location, $window, ngTableParams, backtestingSvc, $routeParams, $filter, utilitiesSvc) {

        $scope.backtesting;

        $scope.securitiesData = [];

        $scope.data = [];

        $scope.benchmarkName;

        $scope.benchMarkResultSelect = null;

        $scope.seIndex = 0;
        $scope.searchText = '';

        var benchmarkHexacolors = ['#02A8F3', '#FF9500', '#CCDB38'];
        var itemsInPage = 12;
        var currentPage;
        var currentSelectedIndex;
        var currentBenchmarkIndex;

        $scope.changeBenchmark = function () {
            setBenchmark($scope.benchmark);
        }

        $scope.setSelectBenchmarkParams = function (index, indexline) {

            setTimeout(function () {

                $scope.$apply(function () {

                    currentSelectedIndex = index;
                    currentBenchmarkIndex = indexline;

                    setSelectPortfolio(index);

                    if (indexline < $scope.backtesting.benchMarkResult.length &&
                        ($scope.backtesting.benchMarkResult[indexline].Item1.Name == 'Empty' ||
                        !$scope.backtesting.benchMarkResult[indexline].Item2[index] ||
                        $scope.backtesting.benchMarkResult[indexline].Item1.ID == '0000'))
                        return;

                    var benchmarkIndex;
                    if (indexline >= $scope.backtesting.benchMarkResult.length &&
                        $scope.backtesting.benchMarkResult.length > 0)
                    {
                        benchmarkIndex = 0
                        var benchmarkReturns = $scope.backtesting.benchMarkResult.map(function (benchmark)
                        {
                            return benchmark.Item2[index].IndexReturn;
                        });
                        var benchmarkMaxReturn = benchmarkReturns[0];
                        for (var i = 1; i < benchmarkReturns.length; i++)
                        {
                            if (benchmarkMaxReturn < benchmarkReturns[i])
                            {
                                benchmarkMaxReturn = benchmarkReturns[i];
                                benchmarkIndex = i;
                            }
                        }
                    }
                    else
                    {
                        benchmarkIndex = indexline;
                    }

                    $scope.startSelectBenchmark = $scope.backtesting.benchMarkResult[benchmarkIndex].Item2[0];
                    $scope.benchMarkResultSelect = $scope.backtesting.benchMarkResult[benchmarkIndex].Item2[index];
                    $scope.benchMarkResultSelect.Name = $scope.backtesting.benchMarkResult[benchmarkIndex].Item1.Name;
                    $scope.benchMarkResultSelect.ID = $scope.backtesting.benchMarkResult[benchmarkIndex].Item1.ID;
                    $scope.benchMarkResultSelect.colorBack = benchmarkHexacolors[benchmarkIndex];
                    var qty = $scope.backtesting.Equity / $scope.backtesting.benchMarkResult[0].Item2[0].IndexAmount * $scope.backtesting.AdjCoeff;
                    bmEquityCursor = qty * ($scope.benchMarkResultSelect.IndexAmount / $scope.backtesting.AdjCoeff);

                    if ($scope.benchMarkResultSelect)
                    {
                        $scope.benchMarkValue = bmEquityCursor;
                        $scope.benchMarkProfit = bmEquityCursor - $scope.backtesting.Equity;
                        $scope.endDate = $scope.benchMarkResultSelect.StartDate ? $scope.benchMarkResultSelect.StartDate : $scope.params.EndDate;
                    }
                    $scope.lastSelectBenchmark = $scope.backtesting.benchMarkResult[benchmarkIndex].Item2[$scope.backtesting.benchMarkResult[benchmarkIndex].Item2.length - 1];
                });
            }, 1000);
        }

        $scope.updatechartSize = function () {
            var a = 5;
        }

        $scope.search = function () {

            if ($scope.searchText == "") {
                $scope.data = $scope.securitiesData;
                $scope.tableParams.reload();
                return;
            }

            $scope.data = $filter('filter')($scope.securitiesData, filterTest, true);
            $scope.tableParams.page(1);
            $scope.tableParams.reload();
        }

        $scope.prevBenchmark = function ()
        {
            changeBenchmark(-1);
        }

        $scope.nextBenchmark = function ()
        {
            changeBenchmark(1);
        }

        $scope.$on('$destroy', function () {
            for (var i in $scope) {
                if (i.indexOf('$') == 1)
                    $scope[i] = null;
            }
        })

        function changeBenchmark(shift)
        {
            currentBenchmarkIndex += shift;
            if (currentBenchmarkIndex >= $scope.backtesting.benchMarkResult.length)
            {
                currentBenchmarkIndex = 0;
            }
            else if (currentBenchmarkIndex < 0)
            {
                currentBenchmarkIndex = $scope.backtesting.benchMarkResult.length - 1;
            }
            $scope.setSelectBenchmarkParams(currentSelectedIndex, currentBenchmarkIndex);
        }

        var filterTest = function (value, index, array) {
            if (!value)
                return false;

            return (value['strSecName'] && value['strSecName'].toLowerCase().indexOf($scope.searchText.toLowerCase()) != -1);
        };

        var setSelectPortfolio = function (index) {
            if ($scope.backtesting.benchMarkResult.length == 0) {
                $scope.selectPortfolio = {};
                return;
            }
            $scope.selectPortfolio = $scope.backtesting.benchMarkResult[0].Item2[index];
            $scope.startSelectBenchmark = $scope.backtesting.benchMarkResult[0].Item2[0];
            $scope.portendDate = $scope.selectPortfolio && $scope.selectPortfolio.StartDate ? $scope.selectPortfolio.StartDate : $scope.params.EndDate;
            if (!$scope.lastSelectBenchmark)
                $scope.lastSelectBenchmark = $scope.backtesting.benchMarkResult[0].Item2[$scope.backtesting.benchMarkResult[0].Item2.length - 1];
        }

        var getBacktesting = function () {

            $scope.benchmarkSecurities = $scope.$parent.getBenchmarkSecurities();
            $scope.backtesting = $scope.$parent.getNewBacktesting();
            currentSelectedIndex = $scope.backtesting.benchMarkResult[0].Item2.length - 1;
            if (!$scope.backtesting)
                return;
            if ($scope.backtesting.SecuritiesTable) {
                $scope.securitiesData = $scope.backtesting.SecuritiesTable;
                $scope.secsNum = $scope.securitiesData.length;
                $scope.data = $filter('orderBy')($scope.securitiesData, 'ProfitLoss', true);
                var _cashData = $scope.data.filter(function (x) { return x.strSecName == 'CASH'; });
                if (_cashData.length > 0) {
                    $scope.data.splice($scope.data.indexOf(_cashData[0]), 1);
                    $scope.secsNum = $scope.data.length;
                    $scope.cashData = _cashData[0];
                }
            }
            setBenchmark($scope.backtesting.BenchmarkID);
            reloadData();
            setRiskColor($scope.backtesting.CurrPortRiskVal * 100);
            if ($scope.backtesting.benchMarkResult.length == 0)
                return;
            var lastIndex = $scope.backtesting.benchMarkResult[0].Item2.length - 1;
            $scope.lastPortfolioEquity = $scope.backtesting.benchMarkResult[0].Item2[lastIndex];
            $scope.setSelectBenchmarkParams(lastIndex, 0);
            setSelectPortfolio(lastIndex);
        };

        var setBenchmark = function (id) {

            var val = $filter('filter')($scope.benchmarkSecurities, { ID: id }, true);
            if (val) {
                val = val[0];
                $scope.benchmark = val.ID;
                $scope.benchmarkName = val.Name;
            }

        }

        var initChartLine = function () {

            $scope.lineLabels = [];
            $scope.lineData = [];

            var portData = [];
            var benchmarkData = [];
            var benchmarkcolors = ['rgb(2,168,244)', 'rgb(255,148,0)', 'rgb(204,219,56)'];

            $scope.chartLegend = [{ name: 'Cherries Portfolio', color: '#2D3E50' }];

            if ($scope.backtesting.benchMarkResult == null) $scope.backtesting.benchMarkResult = [];
            var index = 0;

            benchMarkIndex = 0;
            for (var i = 0; i < $scope.backtesting.benchMarkResult.length; i++) {
                index = 0;
                var benchMark = $scope.backtesting.benchMarkResult[i].Item1;
                var currentBenchmark = [];
                if (benchMark.Name != 'Empty')
                    $scope.chartLegend.unshift({ name: benchMark.Name, color: benchmarkHexacolors[benchMarkIndex] });

                $scope.backtesting.benchMarkResult[i].Item2 = $filter('orderBy')($scope.backtesting.benchMarkResult[i].Item2, 'StartDate', false);
                for (var j = 0; j < $scope.backtesting.benchMarkResult[i].Item2.length; j++) {

                    if (benchMark.Name != 'Empty')
                        currentBenchmark.push({ x: index, y: $filter('number')($scope.backtesting.benchMarkResult[i].Item2[j].IndexReturn * 100, 2) });
                    if (benchMarkIndex == 0) {
                        portData.push({ x: index, y: $filter('number')($scope.backtesting.benchMarkResult[i].Item2[j].PortReturn * 100, 2) });
                        $scope.lineLabels.push($filter('date')($scope.backtesting.benchMarkResult[i].Item2[j].StartDate, 'MMM dd/yy'));
                    }
                    index++;
                }
                if (currentBenchmark.length > 0)
                    benchmarkData.push(currentBenchmark);
                benchMarkIndex++;
            }
            if (benchmarkData.length > 0)
                benchmarkData[benchmarkData.length - 1].tagLabel = 'Benchmark';
            if (portData.length > 0)
                portData[portData.length - 1].tagLabel = 'Portfolio';


            $scope.lineData = [];


            for (var i = 0; i < benchmarkData.length; i++) {
                $scope.lineData.push(
                    {
                        pointDot: true,
                        strokeColor: benchmarkcolors[i],
                        pointColor: benchmarkcolors[i],
                        data: benchmarkData[i]

                    });
            }

            $scope.lineData.push({
                pointDot: true,
                strokeColor: 'rgb(45,62,80)',
                pointColor: 'rgb(45,62,80)',
                data: portData,
                label: 'test1'
            });

            $scope.lineOptions = {
                fillColor: 'transparent',
                strokeColor: '#DADADA',
                pointColor: '#2D3E50',
                barStrokeWidth: 5,
                barValueSpacing: 1,
                barDatasetSpacing: 10,
                labels: $scope.lineLabels,
                scaleLabel: "<%=value%>%",
                scaleArgLabel: "<%=value%>%",
                selectionLine: true,
                leftTag: true,
                tooltipTemplate: function (label, obj) {
                    //display the clients and  Conversion in toolTip
                    return 'Date: ' + $scope.lineLabels[label.index] + ' Return: ' + label.value;
                }
            }

            setTimeout(function () {
                $rootScope.$broadcast('initLineScattertest');
            }, 5000);
            if ($scope.backtesting.benchMarkResult.length == 0)
                return;
            $scope.setSelectBenchmarkParams($scope.backtesting.benchMarkResult[0].Item2.length - 1, 0);
            setSelectPortfolio($scope.backtesting.benchMarkResult[0].Item2.length - 1);
        }

        var reloadData = function () {

            initChartLine();
            $scope.tableParams.page(1);
            $scope.tableParams.reload();
        }

        var initTableParams = function () {

            $scope.tableParams = new ngTableParams({
                page: 1,            // show first page
                count: itemsInPage         // count per page
            }, {
                    total: $scope.data ? $scope.data.length : 0, // length of data
                    counts: [],
                    getData: function ($defer, params)
                    {
                        currentPage = params.page();
                        params.total($scope.data.length);
                        if ($scope.data && $scope.data.length > 0)
                            $defer.resolve($scope.data.slice((params.page() - 1) * params.count(), params.page() * params.count()));
                        else {
                            $defer.resolve($scope.data);
                        }

                    }
                });

            $scope.columns = [
                { name: 'strSecName', colwidth: 2 },
                { name: 'strSymbol', colwidth: 1 },
                { name: 'marketName', colwidth: 2 },
                { name: 'sectorName', colwidth: 1 },
                { name: 'dValOrig', isNumber: true, colwidth: 1, leftBorder: true },
                { name: 'dPriceOrig', isNumber: true, decimalPoins: 2, colwidth: 1, rightBorder: true, paddingLeft: '4px' },
                { name: 'dValNew', isNumber: true, colwidth: 1, leftBorder: true },
                { name: 'dPriceNew', isNumber: true, decimalPoins: 2, colwidth: 1, rightBorder: true, paddingLeft: '4px' },
                { name: 'ProfitLoss', isNumber: true, decimalPoins: 2, colorNumber: true, colwidth: 2, moreInfo: true, paddingLeft: '16px' }
            ];

            $scope.mobileColumns = {
                title: {
                    primary: 'strSecName',
                    secondary: 'strSymbol'
                },
                properties: [
                    {
                        name: 'dValOrig',
                        title: 'Value as of ' + $filter('date')($scope.params.StartDate, 'MM/dd/yyyy'),
                        isNumber: true
                    },
                    {
                        name: 'dValNew',
                        title: 'Value as of ' + $filter('date')($scope.params.EndDate, 'MM/dd/yyyy'),
                        isNumber: true
                    },
                    { name: 'ProfitLoss', title: 'Profit/Loss', isNumber: true, decimalPoins: 2, colorNumber: true }
                ]
            };

            $scope.cashColumns = [
                { name: 'strSecName', colwidth: 2 },
                { name: 'strSymbol', colwidth: 1 },
                { name: 'marketName', colwidth: 2 },
                { name: 'sectorName', colwidth: 1 },
                { name: 'dValOrig', isNumber: true, colwidth: 6, paddingLeft: '12px' }
            ];

            $scope.mobileCashColumns = {
                isCash: true,
                title: {
                    primary: 'strSecName'
                },
                cacheField: 'dValOrig'
            };
        }

        var setRiskColor = function (riskVal) {

            switch (true) {
                case (riskVal > 0 && riskVal <= 9):
                    $scope.riskColor = $scope.riskColors['Solid'];
                    $scope.riskName = 'Solid';
                    break;
                case (riskVal > 9 && riskVal <= 14):
                    $scope.riskColor = $scope.riskColors['Low'];
                    $scope.riskName = 'Low';
                    break;
                case (riskVal > 14 && riskVal <= 25):
                    $scope.riskColor = $scope.riskColors['Moderate'];
                    $scope.riskName = 'Moderate';
                    break;
                case (riskVal > 25 && riskVal <= 40):
                    $scope.riskColor = $scope.riskColors['High'];
                    $scope.riskName = 'High';
                    break;
                case (riskVal > 40 && riskVal <= 100):
                    $scope.riskColor = $scope.riskColors['Very High'];
                    $scope.riskName = 'Very High';
                    break;
            }
            if (!$scope.riskColor) {
                $scope.riskColor = '#ffffff';
                $scope.riskName = 'None';
            }
        }

        var init = function () {

            $scope.$parent.selectMenu = 0;

            $scope.riskColors = riskColors;

            if ($routeParams.id)
                $scope.params = $scope.$parent.getNewBacktestingParams();
            else
                $scope.params = $scope.$parent.getBacktestingParams();

            initTableParams();

            getBacktesting($scope.params);

            if ($scope.params) {

                if (angular.isDate($scope.params.EndDate) == false)
                    $scope.params.EndDate = new Date($scope.params.EndDate);
                if (angular.isDate($scope.params.StartDate) == false)
                    $scope.params.StartDate = new Date($scope.params.StartDate);

                $scope.periodDays = Math.floor($scope.params.EndDate.getTime() / (3600 * 24 * 1000)) - Math.floor($scope.params.StartDate.getTime() / (3600 * 24 * 1000));

                $scope.params.StartDate = $filter('date')($scope.params.StartDate, 'MM/dd/yyyy');
                $scope.params.EndDate = $filter('date')($scope.params.EndDate, 'MM/dd/yyyy');
                $scope.endDate = $scope.params.EndDate;
            }

            $scope.validDate = new Date();
            $scope.validDate.setDate($scope.validDate.getDate() - 1);
            $scope.validDate = $filter('date')($scope.validDate, 'MM/dd/yyyy');

            $scope.showCashRow = function ()
            {
                //check last page in grid and search condition
                var str = 'cash';
                var numPages = Math.floor($scope.data.length / itemsInPage);
                return ((currentPage - 1) == numPages || ($scope.data.length % itemsInPage == 0 && currentPage == numPages)) && ($scope.searchText == "" || (str.toLowerCase().indexOf($scope.searchText.toLowerCase()) != -1));
            }
        }

        init();

    }]);