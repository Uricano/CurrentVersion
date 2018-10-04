app.controller("optimizedPortfolioCtrl", ['$scope', '$rootScope', "$location", "$window", "ngTableParams", "securitiesSvc", '$routeParams', '$filter', 'portfolioSvc', 'utilitiesSvc', 'exportDataSvc', 'isMobile',
    function ($scope, $rootScope, $location, $window, ngTableParams, securitiesSvc, $routeParams, $filter, portfolioSvc, utilitiesSvc, exportDataSvc, isMobile) {

        var allData;
        var portRisks;
        var alllineData = [];
        var portfoliosTemp;
        var currentPage;
        var itemsInPage = 12;
        var isMsgOpen = false;

        $scope.solidData = [];
        $scope.lowData = [];
        $scope.moderateData = [];
        $scope.heighData = [];
        $scope.veryHighData = [];
        $scope.data = [];
        $scope.cashData = {};
        $scope.selectPortfolio;
        $scope.selectProfessional = 'yourPort';
        $scope.chartOrderByField = 'percentage';
        $scope.isProfessionalView = true;
        $scope.searchText = '';
        $scope.weightChart = 'sector';

        $scope.search = function () {

            if ($scope.searchText == "") {
                $scope.data = $scope.selectPortfolio.Securities;
            }
            else {
                $scope.data = $filter('filter')($scope.selectPortfolio.Securities, filterTest, true);
            }
            $scope.data = $scope.data.filter(function (security)
            {
                return security.Quantity >= 1;
            });
            //if ($scope.data[0].Name == 'Cash') {
            //  var cash = $scope.data.splice(0, 1);
            // $scope.cashData = cash[0];
            // }

            $scope.tableParams.page(1);
            $scope.tableParams.reload();
        };

        $scope.setSelectPortfolio = function (index) {
            if (index >= $scope.portfolios.length) index = $scope.portfolios.length - 1;
            $scope.selectPortfolio = $scope.portfolios[index];
            //if ($scope.selectPortfolio.Securities.filter(function (x) { return x.Name == 'Cash'; }).length == 0) {
            //    $scope.selectPortfolio.Securities.push({ Name: 'Cash', 'Symbol': '', 'marketName': '', 'sectorName': '', 'Quantity': '', 'Value': $scope.portfolios[index].Cash });
            //}
            //else if ($scope.selectPortfolio.Securities[0].Name == 'Cash') {
            //    var cash = $scope.selectPortfolio.Securities.splice(0, 1);
            //    $scope.selectPortfolio.Securities.push(cash[0]);
            //}
            allData.PortNumA = index;

            if (($scope.selectPortfolio.Securities.length - 1 < 4 && !isMsgOpen) && !isMobile.phone) {
                isMsgOpen = true;
                utilitiesSvc.showOKMessageWithIndication('Warning', 'Your portfolio is not well diversified. For a more diversified portfolio, please select any of the portfolios to the left of yours along the Efficient Frontier.', 'OK').then(function (result) {
                    isMsgOpen = false;
                });
            }
            allData.PortNumA = index;
            setRiskPortfolio();
            reloadData();
        }

        $scope.setSelectProfessional = function (selectName) {

            $scope.selectProfessional = selectName;

            switch (selectName) {
                case 'MVP':
                    $scope.selectPortfolio = $scope.mvpPort;
                    allData.PortNumA = 0;
                    break;
                case 'yourPort':
                    $scope.selectPortfolio = $scope.yourPort;
                    allData.PortNumA = allData.OrigPortPos;
                    break;
                case 'TP':
                    $scope.selectPortfolio = $scope.tpPort;
                    allData.PortNumA = allData.TangencyPos;
                    break;
            }
            setRiskPortfolio();
            reloadData(true);
        }
        $scope.updatePortdolio = function () {

            if (allData.PortNumA != allData.OrigPortPos) {
                var params = {
                    PortID: allData.PortID,
                    Securities: $scope.selectPortfolio.Securities.filter(function (x) {
                        return x.Name != 'Cash';
                    }),
                    Equity: $scope.newPort.PortDetails.Equity,
                    Risk: $scope.selectPortfolio.Risk,
                    CalcType: 1

                }
                portfolioSvc.updatePortfolio(params).$promise.then(function (data) {

                    utilitiesSvc.showOKMessage('message', 'Portfolio saved successfully.', 'OK');

                    allData.OrigPortPos = allData.PortNumA;
                    $rootScope.$broadcast('updateScetcherTagsAndRedPoint');

                    initChartLine();
                    reloadData();

                }, function (error) {
                    utilitiesSvc.showOKMessage('error', 'failed save data', 'OK');
                });
            }
            else {
                utilitiesSvc.showOKMessage('message', 'Portfolio saved successfully.', 'OK');
            }
        }

        $scope.export = function () {

            var params = {
                portID: allData.PortID,
                OptimalPort: $scope.selectPortfolio
            }

            portfolioSvc.exportOptimize(params).$promise.then(function (data) {
                exportDataSvc.download(data.Data, "", data.Name)
            });
        }

        $scope.changeOrderByBarChart = function () {
            initBarChartSettings();
        }

        var saveNewPort = function () {

            allData = $scope.$parent.getNewPortfolioObj();

            //allData = getData();

            $scope.portfolios = allData.Portfolios;
            $scope.newPort.PortID = allData.PortID;
            $scope.portDetails = allData.PortDetails;
            $scope.changeFromYesterday = $scope.portDetails.CurrEquity - $scope.portDetails.Equity + $scope.portDetails.Profit;
            $scope.equityCalc = $scope.portDetails.Equity + $scope.portDetails.Profit;

            if ($scope.portfolios == null)
                return;
            $scope.selectPortfolio = $scope.portfolios[allData.PortNumA];

            setRiskPortfolio();

            //set basic objects
            $scope.mvpPort = $scope.portfolios[0];
            $scope.yourPort = $scope.portfolios[allData.OrigPortPos];
            $scope.tpPort = $scope.portfolios[allData.TangencyPos];

            initChartLine();
            reloadData();

        }

        var setRiskPortfolio = function () {

            if ($scope.selectPortfolio) {
                $scope.riskColorName = getRiskName($scope.selectPortfolio.Risk);
                $scope.riskColor = $scope.riskColors[$scope.riskColorName];
            }
        }

        var getRiskName = function (val) {

            var value = $filter('number')(val * 100, 2) * 1;
            switch (true) {
                case (value > 0 && value <= 9):
                    return 'Solid'
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
            return 'none';

        }

        var filterTest = function (value, index, array) {
            if (!value)
                return false;

            return value['Name'] && value['Name'].toLowerCase().indexOf($scope.searchText.toLowerCase()) != -1;
        }

        var reloadData = function () {
            $scope.data = $scope.selectPortfolio.Securities.slice();
            $scope.data = $scope.data.filter(function (security)
            {
                return security.Quantity >= 1;
            });
            var cashData = { Name: 'Cash', 'Symbol': '', 'marketName': '', 'sectorName': '', 'Quantity': '', 'DisplayValue': $scope.selectPortfolio.Cash };
            $scope.data.push(cashData);

            initBarChartSettings();
            //initChartLine();
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

        var getLineLabel = function (val, nextval, index, last) {

            if (index == 0 || index == last) {
                return val + "%";
            }

            if ((val > 0 && val <= 9 && nextval > 9 && nextval <= 23) ||
                (val > 9 && val <= 23 && nextval > 23 && nextval <= 40) ||
                (val > 23 && val <= 40 && nextval > 40 && nextval <= 70) ||
                (val > 40 && val <= 70 && nextval > 70 && nextval <= 100)
            ) {
                return val + "%";

            }
            return '';

        }

        var populateRiskDataChart = function (minVal, maxVal) {

            var xDataArrays = ['solidData', 'lowData', 'moderateData', 'heighData', 'veryHighData'];

            for (var i = 0; i < 5; i++) {

                $scope[xDataArrays[i]].push({ x: $filter('number')(portRisks[i + 1].LowerBound * 100, 2), y: 0 });

                if (maxVal >= portRisks[i + 1].UpperBound) {
                    $scope[xDataArrays[i]].push({ x: $filter('number')(portRisks[i + 1].UpperBound * 100, 2), y: 0 });
                }
                else if (maxVal < portRisks[i + 1].UpperBound) {
                    $scope[xDataArrays[i]].push({ x: $filter('number')(maxVal * 100, 2), y: 0 });
                    return;
                }
            }
        }

        String.format = function (str) {
            var args = arguments;
            return str.replace(/{[0-9]}/g, function (matched) { args[parseInt(matched.replace(/[{}]/g, '')) + 1] });
        };

        var initChartLine = function (refreshAfterReload) {

            $scope.lineLabels = [];
            $scope.lineData = [];


            $scope.portfolios = $filter('orderBy')($scope.portfolios, 'Risk', false);
            portfoliosTemp = $scope.portfolios;
            $scope.solidData = [];
            $scope.lowData = [];
            $scope.moderateData = [];
            $scope.heighData = [];
            $scope.veryHighData = [];
            alllineData = [];

            populateRiskDataChart($filter('number')($scope.portfolios[0].Risk, 2), $filter('number')($scope.portfolios[$scope.portfolios.length - 1].Risk, 5));
            var index = 0;
            var itemToRemoveIndex = [];
            for (var i = 0; i < $scope.portfolios.length; i++) {

                var xVal = $filter('number')($scope.portfolios[i].Risk * 100, 1);
                var temp = $filter('filter')(alllineData, { x: xVal }, true);
                if (!temp || temp.length == 0) {

                    alllineData.push({ x: xVal, y: $filter('number')($scope.portfolios[i].Return * 100, 2) });
                    if (i == 0) alllineData[index].tagLabel = "MVP";
                    if (i == allData.OrigPortPos) {

                        if (i == 0) {
                            alllineData[index].tagLabel += " and Opt";
                        }
                        else {
                            alllineData[index].tagLabel = "Opt";
                        }
                    }

                    if (i == allData.TangencyPos) {
                        if (alllineData[index].tagLabel != undefined)
                            alllineData[index].tagLabel += " and TP";
                        else
                            alllineData[index].tagLabel = "TP";
                    }


                    index = index + 1;
                }
                else {
                    itemToRemoveIndex.push(i)

                }
            }
            for (var i = 0; i < itemToRemoveIndex.length; i++) {

                $scope.portfolios.splice(itemToRemoveIndex[i], 1);
            }

            $scope.lineData = [
                {
                    pointDot: true,
                    strokeColor: 'rgb(218,218,218)',
                    pointColor: 'rgb(45,62,80)',
                    scaleSteps: 10,
                    scaleStepWidth: 10,
                    scaleLineWidth: 5,
                    xScaleOverride: true,
                    scaleShowLabels: true,
                    data: alllineData
                }];
            if ($scope.solidData.length > 0) {
                $scope.lineData.push({
                    pointDot: false,
                    strokeColor: 'green',
                    data: $scope.solidData,
                });
            }
            if ($scope.lowData.length > 0 || $scope.solidData.length > 0) {
                $scope.lineData.push({
                    pointDot: false,
                    strokeColor: 'yellow',
                    data: $scope.lowData,
                });
            }
            if ($scope.moderateData.length > 0 || $scope.lowData.length > 0) {
                $scope.lineData.push({
                    pointDot: false,
                    strokeColor: 'rgb(240, 187, 2)',
                    data: $scope.moderateData,
                });
            }
            if ($scope.heighData.length > 0 || $scope.moderateData.length > 0) {
                $scope.lineData.push({
                    pointDot: false,
                    strokeColor: 'darkorange',
                    data: $scope.heighData,
                });
            }
            if ($scope.veryHighData.length > 0 || $scope.heighData.length > 0) {
                $scope.lineData.push({
                    pointDot: false,
                    strokeColor: 'red',
                    data: $scope.veryHighData,

                });
            };

            var max = $filter('number')($scope.portfolios[$scope.portfolios.length - 1].Risk * 100, 2) * 1
            var min = $filter('number')($scope.portfolios[0].Risk * 100, 2) * 1;
            var width = 1;
            var chartWidth = (max - min) * width;

            $scope.lineOptions = {
                selectPointColor: 'red',
                scaleLabel: "<%=value%>%",
                scaleArgLabel: "<%=value%>%",
                precenLabel: false,
                tagWithPath: true,
                //xScaleOverride: true,
                xScaleSteps: 20,
                selectionLine: false,
                xScaleStepWidth: 20,//(window.innerWidth - 250) / max,
                xScaleStartValue: 0,
                scaleShowLabels: true,
                scaleBeginAtZero: true,
                ySelect: $filter('number')($scope.portfolios[0].Return, 2),
                xSelect: $filter('number')($scope.portfolios[0].Risk * 100, 2),
                series: [{
                    text: "Solid: 0%-9%",
                    color: '#2fd074'
                },
                {
                    text: "Low: 9%-14%",
                    color: '#f9ec23'
                },
                {
                    text: "Moderate: 14%-25%",
                    color: '#f9b50a'
                },
                {
                    text: "High:25%-40%",
                    color: '#ff8812'
                },
                {
                    text: "Very High:40%-100%",
                    color: '#fc3d5e'
                }],
                tooltipTemplate: function (label, obj) {
                    if (!label.display) {
                        return '';
                    }
                    //display the clients and  Conversion in toolTip
                    if (label.index != null && label.index >= 0) {
                        var obj = portfoliosTemp[label.index];
                        // var s = String.format("Risk: {0}% \n Average yearly return: {1}% \n Return To Risk Ratio: {2} \n Diversification:{3}% \n ",$filter('number')(obj.Risk * 100, 2), $filter('number')(obj.Return * 100, 2), $filter('number')(obj.RateToRisk, 2), $filter('number')(obj.Diversification * 100, 1));

                        var s = "Risk: " + $filter('number')(obj.Risk * 100, 2) + '% \n Average yearly return: ' + $filter('number')(obj.Return * 100, 2) + '% \n Return To Risk Ratio: ' + $filter('number')(obj.RateToRisk, 2) + ' \n Diversification: ' + $filter('number')(obj.Diversification * 100, 1) + '% \n ';
                        return s;
                    }
                    return label.label + ': ' + label.value;
                },
                showMultiToolTip: false
            }

            var isIE = /(MSIE|Trident\/|Edge\/)/i.test($window.navigator.userAgent);
            var delay = isIE ? 6000 : 2000;

            setTimeout(function () {
                $rootScope.$broadcast('initLineScattertest', refreshAfterReload);
                $scope.setSelectPortfolio(allData.PortNumA);
            }, delay);
        }

        var initBarChartSettings = function () {

            // if (!$scope.barData) {
            $scope.barLabels = [];
            $scope.barData = [];
            $scope.barData1 = [];

            var orderByField = $scope.chartOrderByField == 'percentage' ? 'Weight' : 'Symbol';
            var orderByType = $scope.chartOrderByField == 'percentage' ? true : false;
            $scope.selectPortfolio.Securities = $filter('orderBy')($scope.selectPortfolio.Securities, orderByField, orderByType);

            var max = $scope.selectPortfolio.Securities.length < 325 ? $scope.selectPortfolio.Securities.length : 325;
            for (var i = 0; i < max; i++) {
                if ($scope.selectPortfolio.Securities[i].Symbol != '') {
                    var a = $filter('number')($scope.selectPortfolio.Securities[i].Weight * 100, 2);

                    $scope.barLabels.push($scope.selectPortfolio.Securities[i].Symbol);
                    $scope.barData.push(a * 1);
                    $scope.barData1.push({ id: a * 1 });
                }
            }
            //  }

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
                $rootScope.$broadcast('initChartBar', 'optSecurities', screen.width - 200);
            }, 1500);

        }

        var initChartPie = function () {

            var chartColors = ["#364a60", '#496481', "#7eaddf", "#2d3e58", "#5a7b9f", "#6c94bf", "#89BDF3"];
            var marketData = [];
            var sectors = []
            $scope.stockData = [];
            $scope.sectorData = [];
            var stokeSeries = [];
            var sectorSeries = [];
            var stokeIndex = 0;
            var sectorIndex = 0;
            var securities = $scope.selectPortfolio.Securities.filter(function (x) { return x.sectorName != '' });
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

                if (chartColors.length < i)
                    stokeIndex = 0;
                stokeSeries.push({
                    text: marketData[i].name,
                    color: chartColors[stokeIndex]
                });
                $scope.stockData.push({
                    label: marketData[i].name,
                    value: (marketData[i].value * 100) / securities.length,
                    color: chartColors[stokeIndex],
                    labelColor: 'white',
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
            for (var i = 0; i < sectors.length; i++) {

                if (chartColors.length < i)
                    sectorIndex = 0;
                sectorSeries.push({
                    text: sectors[i].name,
                    color: chartColors[sectorIndex]
                });

                $scope.sectorData.push({
                    label: sectors[i].name,
                    value: (sectors[i].value * 100) / securities.length,
                    color: chartColors[i],
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

        var getRiskLookup = function () {

            var data = $scope.$parent.getLookupValues();

            if (data != null) {
                portRisks = data.PortRisks;
            }
        }

        var init = function () {

            $scope.$parent.selectMenu = 0;

            $scope.riskColors = riskColors;

            $scope.chartName = 'lineChart';

            initTableParams();

            $scope.newPort = $scope.$parent.getNewPortfolio($routeParams.name);

            getRiskLookup();

            saveNewPort();

            $scope.columns = [
                { name: 'Name', colwidth: 3 },
                { name: 'Symbol', colwidth: 1 },
                { name: 'marketName', colwidth: 2 },
                { name: 'Weight', colwidth: 1, isPercentage: true },
                { name: 'sectorName', colwidth: 2, paddingLeft: "3px" },
                { name: 'Quantity', isNumber: true, truncate: true, colwidth: 1 },
                { name: 'DisplayValue', isNumber: true, decimalPoins: 2, colwidth: 2, moreInfo: true }
            ];

            $scope.mobileColumns = {
                title: {
                    primary: 'Name',
                    secondary: 'Symbol'
                },
                properties: [
                    { name: 'Quantity', title: 'Quantity', truncate: true },
                    { name: 'Weight', title: 'Weight', isPercentage: true },
                    { name: 'DisplayValue', title: 'Value', isNumber: true, decimalPoins: 2 }
                ]
            };

            $scope.mobileCashColumns = {
                isCash: true,
                title: {
                    primary: 'Name',
                },
                cacheField: 'DisplayValue'
            };

            user = $scope.$parent.getUser();

            $scope.currency = $scope.$parent.getLookupValues().Currencies.filter(
                function (x) { return x.CurrencyId == $scope.portDetails.CalcCurrency })[0].CurrencySign
            $scope.currencyId = $scope.$parent.getLookupValues().Currencies.filter(
                function (x) { return x.CurrencyId == $scope.portDetails.CalcCurrency })[0].CurrencyId;

            $scope.currDay = new Date();
            if (isMobile.phone) utilitiesSvc.showOKMessage('message', 'Please enable popups for portfolio export', 'OK');
        }

        init();

    }]);