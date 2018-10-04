app.controller("createBacktestingCtrl", ['$scope', "$location", "$window", "ngTableParams", "securitiesSvc", '$routeParams', 'utilitiesSvc', '$filter', "lookupSvc", "$timeout", "isMobile",
    function ($scope, $location, $window, ngTableParams, securitiesSvc, $routeParams, utilitiesSvc, $filter, lookupSvc, $timeout, isMobile) {

        var currentPage;
        var user;
        var timeoutId = 0;
        var minInvestmentAmount = 0;
        var lastKnownExchanges = [];
        var lastKnownSectors = [];
        var selectedExchanges = [];
        var selectedSectors = [];

        $scope.searchParams = {
            exchangesPackagees: [],
            sectors: [],
            maxRiskLevel: 0,
            pageNumber: 1,
            pageSize: isMobile.phone ? 20 : 50,
            searchText: "",
            field: "strName",
            direction: "asc",
            hideDisqualified: true
        };

        $scope.showDisqualified = false;

        $scope.riskFilter;

        $scope.data = [];

        $scope.selectedSecurities = [];

        $scope.selctesItemsNum = null;

        $scope.searchText;

        $scope.selectSecurityNum = 0;

        $scope.isRiskFilter = null;

        $scope.dataLength;

        $scope.exchanges = [];
        $scope.exchangesFilter = [];
        $scope.sectorsOptions = [];
        $scope.sectors = [];
        $scope.newPort = { Exchanges: [], benchMarks: [] };
        $scope.filterDataSource = {
            exchanges: [],
            sectors: []
        };

        $scope.selectOrUnSelectAll = function ()
        {

            if (!$scope.customType || $scope.customType == 1)
                return;

            var maxSecs = $scope.stocksCount < $scope.tableParams.total() ? $scope.stocksCount : $scope.tableParams.total();

            if ($scope.selectedSecurities.length == 0)
            {
                var page = $scope.searchParams.pageNumber;
                var size = $scope.searchParams.pageSize;
                $scope.searchParams.pageNumber = 1;
                $scope.searchParams.pageSize = $scope.stocksCount;
                var hide = $scope.searchParams.hideDisqualified;
                $scope.searchParams.hideDisqualified = true;
                $scope.selectedSecurities = [];
                $scope.selectSecurityNum = maxSecs;
                securitiesSvc.getAllSecurities($scope.searchParams).$promise.then(function (data)
                {
                    $scope.searchParams.pageNumber = page;
                    $scope.searchParams.pageSize = size;

                    for (var i = 0; i < data.Securities.length; i++)
                    {
                        $scope.selectedSecurities.push({ select: true, idSecurity: data.Securities[i].idSecurity });
                    }
                    for (var i = 0; i < $scope.data.length; i++)
                    {
                        var items = $scope.selectedSecurities.filter(function (x) { return x.idSecurity == $scope.data[i].idSecurity });
                        if (items.length > 0)
                            $scope.data[i].select = true;
                    }
                });
                $scope.searchParams.hideDisqualified = hide;
            }
            else
            {
                $scope.selectSecurityNum = 0;
                for (var i = 0; i < $scope.data.length; i++)
                {
                    $scope.data[i].select = false;
                }
            }

            $scope.selectedSecurities = [];
        }

        $scope.createBacktesting = function () {

            $scope.submit = true;
            // $scope.open();
            // return;

            $scope.newPort.Exchanges = $scope.exchanges.map(function (exchange)
            {
                return exchange.id;
            });

            if (!isValidBacktestingParams())
                return;

            if ($scope.customType == 1)
                $scope.createAutoBacktesting();
            else {
                $scope.createCustomBacktesting();
            }
        }

        $scope.createCustomBacktesting = function () {

            if (!$scope.newPort.Name || !$scope.newPort.Equity) {
                utilitiesSvc.showOKMessage('error', 'Fill all portfolio details', 'OK');
                return;
            }

            $scope.createNewPortfolio = true;

            if ($scope.selectedSecurities.length == 0) {
                utilitiesSvc.showOKMessage('message', 'Please select at least 1 security from the list below.', 'OK');
                return;
            }
            $scope.newPort.Securities = [];
            for (var i = 0; i < $scope.selectedSecurities.length; i++) {
                $scope.newPort.Securities.push($scope.selectedSecurities[i].idSecurity);
            }

            $scope.newPort.Exchanges = [];
            for (var i = 0; i < $scope.exchangesFilter.length; i++) {
                $scope.newPort.Exchanges.push($scope.exchangesFilter[i].id);
            }

            buildAndCreateBacktesting(4, $scope.newPort.Securities);
        };

        $scope.createAutoBacktesting = function () {

            if (!$scope.newPort.Name || !$scope.newPort.Equity || $scope.exchanges.length == 0) {
                utilitiesSvc.showOKMessage('error', 'Fill all portfolio details', 'OK');
                return;
            }

            else if ($scope.newPort.Equity < minInvestmentAmount) {
                utilitiesSvc.showOKMessage('error', 'Investment amount must be ' + $filter('number')(minInvestmentAmount, 0) + $scope.currency + ' or higher', 'OK');
                return;
            }

            var securities = [];
            buildAndCreateBacktesting(enumEfCalculationType.BestTP, securities);
        };

        $scope.$watch('customType', function (newValue, oldValue) {
            if (newValue == oldValue) return;
            if (newValue == "2") getSecurities(1);
        }, true);

        var buildAndCreateBacktesting = function (calcType, Securities) {

            $scope.newPort.benchMarks = $scope.newPort.benchMarks.map(function (x) { return x && x.id });
            $scope.newPort.FullbenchMarks = $filter('filter')($scope.benchMarks, filterTest, true);
            $scope.$parent.saveNewBacktestingParams($scope.newPort);

            var parameters = {
                StartDate: $scope.newPort.StartDate,
                EndDate: $scope.newPort.EndDate,
                PortfolioCommand: {
                    Name: $scope.newPort.Name,
                    Securities: Securities,
                    Equity: $scope.newPort.Equity,
                    Risk: 0,
                    Exchanges: $scope.newPort.Exchanges,
                    CalcType: calcType
                },
                BenchMarkID: $scope.newPort.benchMarks
            };

            $scope.createNewPortfolio = true;

            $scope.$parent.createPortfolio(parameters, true);
        }

        var filterTest = function (value, index, array) {
            if (!value)
                return false;
            return $scope.newPort.benchMarks.indexOf(value['id']) >= 0;
        }

        $scope.isValidPortfolioName = function () {
            if ($scope.newPort && $scope.newPort.Name && $scope.newPort.Name.length > 15 ) {
                utilitiesSvc.showOKMessage('error', 'Invalid portfolio name – Please enter a name up to 15 characters', 'OK');
                return false;
            }
            else if ($scope.newPort.Name.indexOf('/') > -1) {
                utilitiesSvc.showOKMessage('error', 'Invalid portfolio name – "/" is not allowed in portfolio name', 'OK');
                return false;
            }
            else {
                return true;
            }
        }

        $scope.search = function () {
            clearTimeout(timeoutId);
            timeoutId = setTimeout(function () {
                $scope.tableParams.page(1);
                $scope.tableParams.reload();
            }, 500);
        }

        $scope.addOrRemoveToSectorList = function (model) {
            if (!model)
                return;

            if (model.select) {
                if ($scope.selectSecurityNum == $scope.stocksCount) {
                    utilitiesSvc.showOKMessage('message', 'You have reach the max securities you can select', 'OK');
                    model.select = false;
                    return;
                }
                $scope.selectSecurityNum = $scope.selectSecurityNum + 1;
                $scope.selectedSecurities.push({ select: true, idSecurity: model.idSecurity });

            }
            else {
                $scope.selectSecurityNum = $scope.selectSecurityNum - 1;
                for (var i = 0; i < $scope.selectedSecurities.length; i++) {
                    if ($scope.selectedSecurities[i].idSecurity == model.idSecurity) {
                        $scope.selectedSecurities.splice(i, 1);
                        break;
                    }
                }
            }
        }

        $scope.setRiskFilter = function (risk) {
            if (risk == 'None') {
                $scope.isRiskFilter = false;
                $scope.riskFilter = null
            } else {
                $scope.isRiskFilter = true;
                $scope.riskFilter = risk;
            }
            $scope.searchParams.maxRiskLevel = getMaxRisk(risk);
            updatePopupFilter();
            filterChanged();
        }

        $scope.exchangesClosed = function ()
        {
            updatePopupFilter();
            var currentExchanges = $scope.exchangesFilter.slice();
            if (!arrayEquals(lastKnownExchanges, currentExchanges, 'id'))
            {
                filterChanged();
            }
            lastKnownExchanges = currentExchanges;
        }

        $scope.sectorsClosed = function ()
        {
            updatePopupFilter();
            var currentSectors = $scope.sectors.slice();
            if (!arrayEquals(lastKnownSectors, currentSectors, 'id'))
            {
                filterChanged();
            }
            lastKnownSectors = currentSectors;
        }

        function arrayEquals(array_1, array_2, fieldName)
        {
            if (fieldName)
            {
                array_1.sort(function (x, y)
                {
                    return x[fieldName] - y[fieldName];
                });
                array_2.sort(function (x, y)
                {
                    return x[fieldName] - y[fieldName];
                });
            }
            else
            {
                array_1.sort();
                array_2.sort();
            }

            return angular.equals(array_1, array_2);
        }

        $scope.setNewPortRiskType = function (type) {
            if (!$scope.newPort.PreferedRisk) { $scope.newPort.PreferedRisk = {}; }

            if ($scope.customType != 2) {
                $scope.RiskType = type;
                $scope.newPort.PreferedRisk.RiskType = getRiskValue(type);
            }
            else {
                $scope.RiskType = null;
                $scope.newPort.PreferedRisk.RiskType = null;
            }
        }

        $scope.applyFilter = function (filterData)
        {
            $scope.sectors = filterData.sectors.slice();
            $scope.exchangesFilter = filterData.exchanges.slice();
            $scope.searchParams.maxRiskLevel = filterData.riskLevel;
            filterChanged();
        }

        $scope.$on('$destroy', function () {
            for (var i in $scope) {
                if (i.indexOf('$') == 1)
                    $scope[i] = null;
            }
        })

        var isValidBacktestingParams = function () {

            if ($scope.$parent.isInProgress) {
                utilitiesSvc.showOKMessage('message', 'The system is already building a portfolio, please wait', 'OK');
                return false;
            }

            if ($scope.portForm.$invalid) {
                utilitiesSvc.showOKMessage('message', "Please fill in all required fields.", 'OK'); return false;
            }

            if ($scope.newPort.Name) {
                if (!$scope.isValidPortfolioName()) return false;
            }

            var oneday = 24 * 60 * 60 * 1000;
            if (($scope.newPort.EndDate.getTime() - $scope.newPort.StartDate.getTime()) / oneday < 30) {
                utilitiesSvc.showOKMessage('message', 'Must select a minimum of 90 days', 'OK');
                return false;
            }

            if (new Date($scope.newPort.EndDate) > new Date()) {
                utilitiesSvc.showOKMessage('message', "Can't select future date", 'OK');
                return false;
            }

            var maxRangDate = new Date($scope.newPort.StartDate.getFullYear() + 2, $scope.newPort.StartDate.getMonth(), $scope.newPort.StartDate.getUTCDate());
            if (maxRangDate < $scope.newPort.EndDate) {
                utilitiesSvc.showOKMessage('message', "Date range maximum is 2 years", 'OK');
                return false;
            }

            if (!$scope.customType || $scope.customType == '') {
                utilitiesSvc.showOKMessage('message', 'Please select a Building Process.', 'OK');
                return false;
            }

            if (new Date(user.Licence.ExpiryDate) < new Date()) {
                utilitiesSvc.showOKMessage('message', 'Your license has expired. You can no longer build new portfolios. Please contact us at info@gocherries.com to renew your license.', 'OK');
                return false;
            }

            if (user.Licence.Service.Iportfolios <= $scope.$parent.getPortfolioList().length) {
                utilitiesSvc.showOKMessage('message', "You've exceeded the number of portfolios you can create. Please delete any unwanted portfolios", 'OK');
                return false;
            }

            return true;
        }

        var getRiskValue = function (value) {

            switch (value) {
                case 0:
                    return 0;

                case 1:
                    return 9 / 100;

                case 2:
                    return 14 / 100

                case 3:
                    return 25 / 100;

                case 4:
                    return 40 / 100;

                case 5:
                    return 1;


            }
        }

        var getRiskByRiskType = function (type) {

            switch (type) {
                case 0: return "TP";

                case 1: return 'Solid';

                case 2: return "Low";

                case 3: return 'Moderate';

                case 4: return "High";

                case 5: return 'Very High';

            }
        }

        var isPrecentValueInRange = function (model) {

            var value = user.Currency.CurrencyId != '9001' ? model.avgYield : model.avgYieldNIS;
            value = $filter('number')(value * 100, 2);
            return value * 1 >= $scope.returnValueFilter * 1;
        }

        var getMaxRisk = function (value) {

            switch ($scope.riskFilter) {
                case 'Solid':
                    return 9;
                case 'Low':
                    return 14;
                case 'Moderate':
                    return 25;
                case 'High':
                    return 40;
                case 'Very High':
                    return 100;
                default:
                    return 0;
            }

        }

        var getSecurities = function (page) {
            page = page == null ? 1 : page;
            $scope.tableParams = new ngTableParams({
                page: page,            // show first page
                count: $scope.searchParams.pageSize         // count per page

            }, {
                    total: $scope.data.length, // length of data
                    counts: [],
                    getData: function ($defer, params) {
                        $scope.searchParams.pageNumber = $scope.tableParams.page();
                        $scope.searchParams.pageSize = $scope.tableParams.count();
                        $scope.searchParams.exchangesPackagees = getSelectedExchanges();
                        $scope.searchParams.sectors = getSelectedSectors();
                        $scope.searchParams.hideDisqualified = !$scope.showDisqualified;

                        securitiesSvc.getAllSecurities($scope.searchParams).$promise.then(function (data) {
                            $timeout(function () {
                                if (!data.Securities || data.Securities.length == 0) {
                                    $scope.data = [];
                                    utilitiesSvc.showOKMessage('error', 'There are no securities available', 'OK');
                                    return;
                                }
                                //utilitiesSvc.showOKMessage('message', 'Please choose Exchanges/Stocks/Sector for showing securties', 'OK');
                                $scope.data = data.Securities;
                                if ($scope.selectedSecurities.length > 0) {
                                    for (var i = 0; i < $scope.data.length; i++) {
                                        var items = $scope.selectedSecurities.filter(function (x) { return x.idSecurity == $scope.data[i].idSecurity });
                                        if (items.length > 0)
                                            $scope.data[i].select = true;
                                    }
                                }
                                params.total(data.NumOfRecords);
                                $scope.currenPageCount = params.page() * params.count() < data.NumOfRecords ? params.count() : data.NumOfRecords - ((params.page() - 1) * params.count());
                                $defer.resolve($scope.data);
                            }, 500);
                        }, function (error) {

                            //utilitiesSvc.showOKMessage('error', 'Failed to get securities', 'OK');

                        });
                    }
                });
        }

        var getLookupValues = function () {

            var data = $scope.$parent.getLookupValues();

            if (data != null) {
                $scope.sectorsOptions = data.Categories.Sector;
            }
            for (var i = 0; i < $scope.sectorsOptions.length; i++) {
                $scope.sectorsOptions[i].label = $scope.sectorsOptions[i].strValue;
            }
            for (var i = 0; i < $scope.stockMarket.length; i++) {
                $scope.stockMarket[i].label = $scope.stockMarket[i].HebName;
            }
        }

        var getBenchMarks = function () {
            securitiesSvc.getBenchmarkSecurities().$promise.then(function (data) {
                $scope.benchMarks = [];
                for (var i = 0; i < data.Securities.length; i++) {
                    $scope.benchMarks.push({ id: data.Securities[i].ID, label: data.Securities[i].Name });
                }
            });
        }

        var init = function () {

            $scope.$parent.selectMenu = 3;

            user = $scope.$parent.getUser();

            $scope.stocksCount = user.Licence.Stocks.length * 250;

            //var stdYield = user.Currency.CurrencyId != '9001' ? 'StdYield' : 'StdYieldNIS';
            $scope.avgYield = 'AvgYield';

            $scope.stockMarketSettings = {
                showCheckAll: true,
                showUncheckAll: true,
                idProp: 'id',
                sellectAllDefault: false,
                buttonDefaultText: 'select',
                buttonClasses: 'btn btn-default drop-btn',
                scrollable: true,
                scrollableHeight: 190,

            };

            $scope.benchMarkSettings = {
                showCheckAll: false,
                showUncheckAll: false,
                idProp: 'id',
                sellectAllDefault: false,
                buttonDefaultText: 'None',
                buttonClasses: 'btn btn-default drop-btn',
                scrollable: true,
                scrollableHeight: 190,
                selectionLimit: 3
            };

            $scope.benchMarkTexts = {
                buttonDefaultText: 'None'
            };

            $scope.sectorsSettings = {
                showCheckAll: true,
                showUncheckAll: true,
                idProp: 'iIndex',
                sellectAllDefault: false,
                buttonDefaultText: 'select',
                buttonClasses: 'btn btn-default drop-btn',
                scrollable: true,
                scrollableHeight: 190,

            };
            $scope.columns = [
                { name: 'strName', colwidth: 3, paddingLeft: "3px" },
                { name: 'strSymbol', colwidth: 2, paddingLeft: "10px" },
                { name: 'marketName', colwidth: 1, paddingLeft: "14px" },
                { name: 'sectorName', colwidth: 2, paddingLeft: "16px" },
                { name: $scope.avgYield, isPercentage: true, colwidth: 2, paddingLeft: "22px" },
                { name: 'rank', moreInfo: true, isNumber: true, colwidth: 2, paddingLeft: "22px" },
            ];

            $scope.mobileColumns = {
                title: {
                    primary: 'strName',
                    secondary: 'strSymbol'
                },
                properties: [
                    { name: 'sectorName', title: 'Sector' },
                    { name: 'marketName', title: 'Exchange' },
                    { name: 'rank', title: 'Rank', isNumber: true }
                ]
            };

            $scope.stockMarket = user.Licence.Stocks;

            getLookupValues();
            getBenchMarks();

            $scope.filterDataSource.exchanges = $scope.stockMarket.map(function (stock)
            {
                return {
                    id: stock.id,
                    label: stock.HebName
                };
            });

            $scope.filterDataSource.sectors = $scope.sectorsOptions.map(function (sector)
            {
                return {
                    id: sector.iIndex,
                    label: sector.strValue
                }
            });

            updatePopupFilter();

            if ($scope.customType == 2)
                getSecurities(1);

        }

        function updatePopupFilter()
        {
            var currentExchanges = ($scope.exchangesFilter || []).map(function (exch)
            {
                return exch.id;
            });
            angular.forEach($scope.filterDataSource.exchanges, function (exch)
            {
                exch.selected = currentExchanges.indexOf(exch.id) >= 0;
            });

            var currentSectors = ($scope.sectors || []).map(function (sector)
            {
                return sector.id;
            });
            angular.forEach($scope.filterDataSource.sectors, function (sector)
            {
                sector.selected = currentSectors.indexOf(sector.id) >= 0;
            });
            $scope.filterDataSource.riskLevel = $scope.searchParams.maxRiskLevel;
        }

        function filterChanged()
        {
            if ($scope.customType != 2)
            {
                return;
            }

            selectedSectors = $scope.sectors.map(function (selectedSector)
            {
                return selectedSector.id;
            });
            selectedExchanges = $scope.exchangesFilter.map(function (selectedExch)
            {
                return selectedExch.id;
            });

            $scope.tableParams.page(1);
            $scope.tableParams.reload();
        }

        function getSelectedExchanges()
        {
            if (selectedExchanges.length > 0)
            {
                return selectedExchanges.slice();
            }
            else
            {
                return $scope.stockMarket.map(function (exch)
                {
                    return exch.id;
                });
            }
        }

        function getSelectedSectors()
        {
            if (selectedSectors.length > 0)
            {
                return selectedSectors.slice();
            }
            else
            {
                return $scope.sectorsOptions.map(function (sect)
                {
                    return sect.iIndex;
                });
            }
        }

        init();
    }]);