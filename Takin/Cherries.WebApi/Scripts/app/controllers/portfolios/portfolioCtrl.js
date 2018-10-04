app.controller("portfolioCtrl", ['$scope', '$rootScope', "$location", "$window", "ngTableParams", "portfolioSvc", "utilitiesSvc", "$filter", "lookupSvc", "backtestingSvc", "$timeout", function ($scope, $rootScope, $location, $window, ngTableParams, portfolioSvc, utilitiesSvc, $filter, lookupSvc, backtestingSvc, $timeout) {

    var currentPage;

    $scope.submit = false;

    $scope.data = [];

    $scope.dataBacktesing = [];

    $scope.newPort = { Exchanges: [] };

    $scope.exchanges = [];

    $scope.customType = "";
    $scope.searchParams = {};
    var isreversePort = true;

    var isreverseBack = true;

    $scope.getPortfolios = function () {

       
        $scope.tableParams.reload();

       
        $scope.tableParams1.reload();
    };

    $scope.refreshPortfolios = function (port) {
        $scope.data.splice($scope.data.indexOf(port), 1);
        $scope.tableParams.reload();
        savePortfolioList();
    }

    $scope.refreshBacktesting = function (backtest) {
        $scope.dataBacktesing.splice($scope.dataBacktesing.indexOf(backtest), 1);
        $scope.tableParams1.reload();
        //savePortfolioList();
    }

    $scope.sortPortfolio = function () {
        isreversePort = !isreversePort;
        $scope.data = $filter('orderBy')($scope.data, $scope.orderByPortField, isreversePort);
        $scope.tableParams.reload();
    }

    $scope.sortBacktesting = function () {
        isreverseBack = !isreverseBack;
        $scope.dataBacktesing = $filter('orderBy')($scope.dataBacktesing, $scope.orderByBackField, isreverseBack);
        $scope.tableParams1.reload();
    }

    var savePortfolioList = function () {
        portList = [];
        for (var i = 0; i < $scope.data.length; i++) {
            portList.push({ 'ID': $scope.data[i].ID, 'Name': $scope.data[i].Name });
        }
        $scope.$parent.setPortfolioList(portList);
    }

    var initGridParams = function () {

        $scope.tableParams = new ngTableParams({
            page: 1,            // show first page
            count: 5         // count per page

        }, {
                total: $scope.data.length, // length of data
                counts: [],
                getData: function ($defer, params) {

                    $scope.searchParams.pageNumber = $scope.tableParams.page();
                    $scope.searchParams.pageSize = $scope.tableParams.count();
                    $scope.searchParams.sortField = $scope.orderByPortField;
                    $scope.noPortfolios = $scope.data.length === 0;
                    portfolioSvc.getPortfolios($scope.searchParams).$promise.then(function (data) {
                        $timeout(function () {
                        
                            $scope.data = data.Portfolios;
                            params.total(data.NumOfRecords);
                            savePortfolioList();
                            currentPage = params.page() * params.count() < data.NumOfRecords ? params.count() : data.NumOfRecords - ((params.page() - 1) * params.count());
                            $scope.noPortfolios = $scope.data.length === 0;
                            if ($scope.data)
                                $defer.resolve($scope.data);
                        }, 500);
                    });
                }
            });

        $scope.tableParams1 = new ngTableParams({
            page: 1,            // show first page
            count: 5         // count per page

        }, {
                total: $scope.dataBacktesing.length, // length of data
                counts: [],
                getData: function ($defer, params) {


                    var sParams = {};
                    sParams.pageNumber = $scope.tableParams1.page();
                    sParams.pageSize = $scope.tableParams1.count();
                    sParams.sortField = $scope.orderByBackField;
                    $scope.noBacktesting = $scope.dataBacktesing.length === 0;
                    backtestingSvc.getbacktesingPortfolies(sParams).$promise.then(function (data) {

                        $timeout(function () {
                           
                            $scope.dataBacktesing = data.Ports;
                            params.total(data.NumOfRecords);
                            currentPage = params.page() * params.count() < data.NumOfRecords ? params.count() : data.NumOfRecords - ((params.page() - 1) * params.count());
                            $scope.noBacktesting = $scope.dataBacktesing.length === 0;
                            if ($scope.dataBacktesing)
                                $defer.resolve($scope.dataBacktesing);
                        }, 500);
                    });
                }
            });
    }
    var getStocks = function () {
        $scope.$parent.getUserStocks();
    }

    var failedGetLookup = function (error) { }

    var initialize = function () {

        $scope.$parent.selectMenu = 1;

        initGridParams();

        getStocks();

        $scope.getPortfolios();

        user = $scope.$parent.getUser();

        $scope.currency = user.Currency ? user.Currency.CurrencySign : '';

        var today = new Date();

        $scope.currentDate = today;
    };

    initialize();

}]);