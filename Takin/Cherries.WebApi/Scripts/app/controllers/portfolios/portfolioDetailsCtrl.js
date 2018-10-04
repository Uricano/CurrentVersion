app.controller("portfolioDetailsCtrl", ['$scope', '$rootScope', "$location", "$window", "ngTableParams", "securitiesSvc", '$routeParams', '$filter', 'portfolioSvc', 'utilitiesSvc', function ($scope, $rootScope, $location, $window, ngTableParams, securitiesSvc, $routeParams, $filter, portfolioSvc, utilitiesSvc) {

    $scope.portfolio;

    $scope.data = [];

    $scope.chartOrderByField = 'percentage';

    $scope.cashData = {};

    $scope.searchText = '';

    var currentPage = -1;

    var itemsInPage = 12;

    $scope.search = function () {

        if ($scope.searchText == "") {
            $scope.data = $scope.portfolio.SecurityData;
        }
        else {
            $scope.data = $filter('filter')($scope.portfolio.SecurityData, filterTest, true);
        }

        $scope.data = $scope.data.filter(function (security)
        {
            return security.flQuantity >= 1;
        });

        //if ($scope.data[0].strName == 'Cash') {
        //  var cash = $scope.data.splice(0, 1);
        //  $scope.data.push(cash[0]);
        //}
        $scope.tableParams.page(1);
        $scope.tableParams.reload();
    };

    $scope.updatechartSize = function () {
        var a = 5;
    }

    $scope.calculate = function () {

        if (!$scope.lastPeriod || !$scope.Lenchmark) {
            utilitiesSvc.showOKMessage('message', 'Please fill benchMark index and last period.', 'OK');
            return;
        }
        var x = 1;
        if ($scope.lastPeriod == 2)
            x = 6;
        if ($scope.lastPeriod == 3)
            x = 12;
        var startDate = new Date();
        startDate.setMonth(startDate.getMonth() - x);

        var params = {
            portName: $scope.portfolio.Name,
            PortID: $scope.portfolio.ID,
            Equity: $scope.portfolio.Equity,
            StartDate: startDate,
            EndDate: new Date(),
            BenchMarkID: $scope.Lenchmark,
        }

        $scope.$parent.saveBacktestingParams(params);
        $location.path('backtesting');
    };

    $scope.changePortfolio = function () {
        getPortfolio();
    }

    $scope.exportToPdf = function () {

        var dataCount = $scope.data.length < 39 ? $scope.data.length : 39;

        reloadSecTable(dataCount, 1);
        utilitiesSvc.exportToPdf('portDetails', 'portfolio ' + $scope.portfolio.Name).then(function () {
            if ($scope.data.length > 39) {
                exportPdf(dataCount, 2, $scope.data.length / 39);
            }
            else {
                reloadSecTable(12, 1);
            }
        });
    }

    var reloadSecTable = function (count, page) {

        $scope.tableParams.count(count);
        $scope.tableParams.page(page);
        $scope.tableParams.reload();

    }

    var exportPdf = function (dataCount, index, max) {

        reloadSecTable(dataCount, index);

        utilitiesSvc.exportToPdf('portSecurities', 'portfolio ' + $scope.portfolio.Name).then(function () {
            if (index < max) {
                exportPdf(dataCount, index + 1, max);
                index = index + 1;
            }
            else {
                reloadSecTable(12, 1);
            }
        });
    }

    var filterTest = function (value, index, array) {
        if (!value)
            return false;

        return (value['strName'] && value['strName'].toLowerCase().indexOf($scope.searchText.toLowerCase()) != -1);
        //|| (value['strSymbol'] && value['strSymbol'].toLowerCase().indexOf($scope.searchText.toLowerCase()) != -1)
        //|| (value['marketName'] && value['marketName'].toLowerCase().indexOf($scope.searchText.toLowerCase()) != -1)
        //|| (value['sectorName'] && value['sectorName'].toLowerCase().indexOf($scope.searchText.toLowerCase()) != -1);
    };

    var getPortfolio = function () {

        portfolioSvc.getSinglePortfolio($scope.portfolioId).$promise.then(function (data) {
            $scope.portfolio = data.Details[0];
            $scope.data = $filter('orderBy')($scope.portfolio.SecurityData, 'SecValue', true);
            $scope.data = $scope.data.filter(function (security)
            {
                return security.flQuantity >= 1;
            });
            $scope.riskName = getRiskColorName($scope.portfolio.CurrentStDev);
            $scope.riskColor = $scope.riskColors[$scope.riskName];
            $scope.changeFromYesterday = $scope.portfolio.CurrEquity - $scope.portfolio.Equity + $scope.portfolio.Profit;

            $scope.changeIcon = $scope.changeFromYesterday > 0 ? 'arrow_up' : 'arrow_down';
            $scope.cashData = { 'strName': 'Cash', 'strSymbol': '', 'marketName': '', 'sectorName': '', 'flQuantity': '', 'SecValue': $scope.portfolio.Cash };
            //$scope.data.push({ 'strName': 'Cash', 'strSymbol': '', 'marketName': '', 'sectorName': '', 'flQuantity': '', 'SecValue': $scope.portfolio.Cash });
            $scope.currency = $scope.$parent.getLookupValues().Currencies.filter(
                function (x) { return x.CurrencyId == $scope.portfolio.CalcCurrency })[0].CurrencySign
            $scope.currencyId = $scope.$parent.getLookupValues().Currencies.filter(
                function (x) { return x.CurrencyId == $scope.portfolio.CalcCurrency })[0].CurrencyId;
            var dValueUSAFields = $scope.portfolio.CalcCurrency != '9001' ? 'dValueUSA' : 'dValueNIS';

            $scope.columns = [
                { name: 'strName', colwidth: 3, paddingLeft: '2px' },
                { name: 'strSymbol', colwidth: 1 },
                { name: 'marketName', colwidth: 2, margin: '-12px' },
                { name: 'portSecWeight', colwidth: 1, isPercentage: true, margin: '-8px' },
                { name: 'sectorName', colwidth: 2, paddingLeft: '7px' },
                { name: 'flQuantity', isNumber: true, colwidth: 1 },
                { name: 'SecValue', isNumber: true, decimalPoins: 2, colwidth: 1, paddingLeft: '10px' },
                { name: 'Profit', colwidth: 1, moreInfo: true, isProfit: true }
            ];

            $scope.cacheColumns = [
                { name: 'strName', colwidth: 3, paddingLeft: '2px' },
                { name: 'Mock', colwidth: 7 },
                { name: 'SecValue', isNumber: true, decimalPoins: 2, colwidth: 2, paddingLeft: '5px', moreInfo: true }
            ];

            $scope.mobileColumns = {
                isCash: false,
                title: {
                    primary: 'strName',
                    secondary: 'strSymbol'
                },
                profit: 'Profit',
                properties: [
                    { name: 'flQuantity', title: 'Quantity', isNumber: true },
                    { name: 'portSecWeight', title: 'Weight', isPercentage: true },
                    { name: 'SecValue', title: 'Current Value', isNumber: true, decimalPoins: 2 }
                ]
            };

            $scope.mobileCachColumns = {
                isCash: true,
                title: {
                    primary: 'strName'
                },
                cacheField: 'SecValue',
                profitSpacer: true
            };

            reloadData();
        }, function (error) {
            //utilitiesSvc.showOKMessage('error', 'Failed to get portfolio', 'OK');
        });
    }

    var reloadData = function () {

        initBarChartSettings();
        initChartPie();
        $scope.tableParams.page(1);
        $scope.tableParams.reload();
    }

    var initTableParams = function () {

        $scope.tableParams = new ngTableParams({
            page: 1,            // show first page
            count: itemsInPage         // count per page

        }, {
                total: $scope.data.length, // length of data
                counts: [],
                getData: function ($defer, params) {
                    currentPage = params.page();
                    params.total($scope.data.length);
                    if ($scope.data && $scope.data.length > 0)
                        $defer.resolve($scope.data.slice((params.page() - 1) * params.count(), params.page() * params.count()));
                    else {
                        $defer.resolve($scope.data);
                    }

                }
            });
    }

    //$rootScope.$on('resizeWindow', function (event, width) {

    //    $rootScope.$broadcast('initChartBar', 'portSecurities', width-200);

    //});

    $scope.showCashRow = function () {

        //check last page in grid and search condition
        if (!$scope.data)
        {
            return false;
        }
        var str = 'cash';
        var numPages = Math.floor($scope.data.length / itemsInPage);
        return ((currentPage - 1) == numPages || ($scope.data.length % itemsInPage == 0 && currentPage == numPages)) && ($scope.searchText == "" || (str.toLowerCase().indexOf($scope.searchText.toLowerCase()) != -1));
    }

    $scope.changeOrderByBarChart = function () {
        initBarChartSettings();
    }

    var initBarChartSettings = function () {

        $scope.barLabels = [];
        $scope.barData = [];
        $scope.barData1 = [];
        $scope.deleteSecuritiesMone = 0;

        var maxBarLength = $scope.portfolio.SecurityData.length < 327 ? $scope.portfolio.SecurityData.length : 327;
        var orderByField = $scope.chartOrderByField == 'percentage' ? 'portSecWeight' : 'strSymbol';
        var orderByType = $scope.chartOrderByField == 'percentage' ? true : false;
        $scope.portfolio.SecurityData = $filter('orderBy')($scope.portfolio.SecurityData, orderByField, orderByType);
        for (var i = 0; i < maxBarLength; i++) {
            if ($scope.portfolio.SecurityData[i].strSymbol != '') {
                $scope.barLabels.push($scope.portfolio.SecurityData[i].strSymbol);
                $scope.barData.push($filter('number')($scope.portfolio.SecurityData[i].portSecWeight * 100, 2));
                $scope.barData1.push({ id: $filter('number')($scope.portfolio.SecurityData[i].portSecWeight * 100, 2) });
            }
        }

        $scope.barOptions = {
            scaleLabel: "<%=value%>%",
            fillColor: ['#6C94BF', '#2D3E50', '#496481'],
            strokeColor: ['#6C94BF', '#2D3E50', '#496481'],
            pointColor: "#6C94BF",
            barStrokeWidth: 5,
            barValueSpacing: 1,
            barDatasetSpacing: 10,
        }

        setTimeout(function () {
            $rootScope.$broadcast('initChartBar', 'portSecurities', screen.width - 200);
        }, 1000);

    }

    var initChartPie = function () {

        $scope.stockData = [];
        $scope.sectorData = [];

        var chartColors = ["#364a60", '#496481', "#7eaddf", "#2d3e58", "#5a7b9f", "#89BDF3", "#6c94bf"];
        var marketData = [];
        var sectors = []
        var stokeSeries = [];
        var sectorSeries = [];
        var stokeIndex = 0;
        var securities = $scope.portfolio.SecurityData.filter(function (x) { return x.sectorName != '' });
        for (var i = 0; i < securities.length; i++) {

            var a = $filter('filter')(marketData, { id: securities[i].idMarket }, true);
            if (a.length > 0) {
                a[0].value = a[0].value + 1;
            }
            else {
                marketData.push({ name: securities[i].marketName, value: 1, id: securities[i].idMarket });
            }

        }
        for (var i = 0; i < marketData.length; i++) {

            if (chartColors.length - 1 < stokeIndex) {
                if (chartColors.length + 1 == marketData.length)
                    stokeIndex = 1;
                else stokeIndex = 0;

            }
            stokeSeries.push({
                text: marketData[i].name,
                color: chartColors[stokeIndex]
            });

            $scope.stockData.push({
                label: marketData[i].name,
                value: (marketData[i].value * 100) / securities.length,
                color: chartColors[stokeIndex],
                labelColor: '#ffffff',
                labelFontSize: '16'
            });
            stokeIndex = stokeIndex + 1;
        }

        for (var i = 0; i < securities.length; i++) {

            var a = $filter('filter')(sectors, { id: securities[i].idSector }, true);
            if (a.length > 0) {
                a[0].value = a[0].value + 1;
            }
            else {
                sectors.push({ name: securities[i].sectorName, value: 1, id: securities[i].idSector });
            }

        }
        var sectorIndex = 0;
        for (var i = 0; i < sectors.length; i++) {

            if (chartColors.length - 1 < sectorIndex) {
                if (chartColors.length + 1 == sectors.length)
                    sectorIndex = 1;
                else sectorIndex = 0;
            }
            sectorSeries.push({
                text: sectors[i].name,
                color: chartColors[sectorIndex]
            });
            $scope.sectorData.push({

                value: (sectors[i].value * 100) / securities.length,
                color: chartColors[sectorIndex],
                label: sectors[i].name,
                labelColor: 'white',
                labelFontSize: '16'
            });

            sectorIndex = sectorIndex + 1;
        }


        $scope.stockOptions = {
            segmentShowStroke: false,
            showTooltips: false,
            series: stokeSeries,
            tooltipTemplate: function (label) {
                //display the clients and  Conversion in toolTip
                return label.label + ': ' + label.value;
            }
        };

        $scope.sectorOptions = {
            showTooltips: false,
            segmentShowStroke: false,
            series: sectorSeries,
            tooltipTemplate: function (label) {
                //display the clients and  Conversion in toolTip
                return label.label + ': ' + label.value;
            }
        };


        setTimeout(function () {
            $rootScope.$broadcast('initChartPie', 'stockPieChart');
            $rootScope.$broadcast('initChartPie', 'sectorPieChart');
            $rootScope.$broadcast('initChartPie', 'stockPieChartMobile');
            $rootScope.$broadcast('initChartPie', 'sectorPieChartMobile');
        }, 1000);

    }

    var initBenchmarkSecurities = function () {

        $scope.benchmarkSecurities = $scope.$parent.getBenchmarkSecurities();
        if ($scope.benchmarkSecurities != null)
            return;
        securitiesSvc.getBenchmarkSecurities().$promise.then(function (data) {
            $scope.benchmarkSecurities = data.Securities;
            $scope.$parent.saveBenchmarkSecurities(data.Securities);
        }, function (error) {
            //utilitiesSvc.showOKMessage('error', 'Failed to get benchmark Securities', 'OK');
            var d = [
                {
                    "$id": "2",
                    "ID": "0.1297",
                    "Name": "NYSE Composite"
                },
                {
                    "$id": "3",
                    "ID": "0.2020",
                    "Name": "S&P 500 Index"
                },
                {
                    "$id": "4",
                    "ID": "0.5177",
                    "Name": "Nasdaq 100"
                },
                {
                    "$id": "5",
                    "ID": "137",
                    "Name": "TA – 100"
                },
                {
                    "$id": "6",
                    "ID": "142",
                    "Name": "TA – 25"
                },
                {
                    "$id": "7",
                    "ID": "143",
                    "Name": "TA – 75"
                },
                {
                    "$id": "8",
                    "ID": "147",
                    "Name": "TA MidCap – 50"
                },
                {
                    "$id": "9",
                    "ID": "164",
                    "Name": "TA – Banking"
                },
                {
                    "$id": "10",
                    "ID": "168",
                    "Name": "TA – Composite"
                },
                {
                    "$id": "11",
                    "ID": "2",
                    "Name": "General Share"
                },
                {
                    "$id": "12",
                    "ID": "709",
                    "Name": "Tel – Bond 60"
                }
            ];
            $scope.$parent.saveBenchmarkSecurities(d);
        });

    }

    var getRiskColorName = function (val) {
        var value = $filter('number')(val * 100, 2) * 1;

        switch (true) {
            case (value > 0 && value <= 9):
                return 'Solid';
                break;
            case (value > 9 && value <= 14):
                return 'Low';
                break;
            case (value > 14 && value <= 25):
                return 'Moderate';
                break;
            case (value > 25 && value <= 40):
                return 'High';
                break;
            case (value > 40 && value <= 100):
                return 'Very High';
                break;
        }
        return '#ffffff';
    }

    var init = function () {

        $scope.$parent.selectMenu = 0;

        $scope.portfolioId = $routeParams.id;

        $scope.portfolios = $scope.$parent.getPortfolioList();

        getPortfolio();

        initTableParams();

        $scope.periods = [{ ID: 1, Name: 'last month' }, { ID: 2, Name: 'last 6 month' }, { ID: 3, Name: 'last year' }];

        //initBenchmarkSecurities();

        $scope.riskColors = riskColors;

        var user = $scope.$parent.getUser();



        $scope.currDay = new Date();

    }

    init();
}]);