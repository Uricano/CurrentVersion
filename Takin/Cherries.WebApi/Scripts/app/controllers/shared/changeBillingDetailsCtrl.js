app.controller("changeBillingDetailsCtrl", ['$scope', '$rootScope', "$location", "lookupSvc", "utilitiesSvc", '$filter', 'userSvc', 'ngTableParams', 'licenseSvc', '$window', function ($scope, $rootScope, $location, lookupSvc, utilitiesSvc, $filter, userSvc, ngTableParams, licenseSvc, $window) {

    $scope.model = { "SumInServer": null };

    $scope.data = [];

    $scope.servicePack = {};

    $scope.disablePaypal = true;

    var fullUser;

    var stockTemp = [];

    $scope.calculateLicense = function () {

        if (!Billingvalidation(true))
            return false;

        $scope.validForm = true;

        var params = {
            ServiceID: $scope.servicePack.selectBillingRow,
            StockCount: $scope.model.Stocks.length
        }

        licenseSvc.calculateLicense(params).$promise.then(function (data) {
            $scope.model.SumInServer = data.Sum;
            $scope.disablePaypal = false;

        }, function (error) {
            utilitiesSvc.showOKMessage('error', error.data, 'OK');
        });
    }
    $rootScope.$$listeners.paypalPaymentComplited = [];
    $rootScope.$on('paypalPaymentComplited', function (event, paymentResult) {
        $scope.updateUserLicence(paymentResult);
    });

    $scope.canUsePaypal = function () {
        return Billingvalidation(false) && $scope.model.SumInServer != null;
    }

    var Billingvalidation = function (showMsg) {

        if (!$scope.servicePack || !$scope.servicePack.selectBillingRow) {
            if (showMsg) utilitiesSvc.showOKMessage(messagesType.error, 'You must choose your billing plan', buttonsType.OK);
            return false;
        }
        if (!$scope.model || $scope.model.Stocks.length < 2) {
            $scope.validForm = false;
            if (showMsg) utilitiesSvc.showOKMessage('error', "You must choose at least 2 stocks", 'OK');
            return false;
        }

        return true;
    }

    $scope.updateUserLicence = function () {
        if (!Billingvalidation(true))
            return false;
        $scope.model.Idlicservice = $scope.servicePack.selectBillingRow;
        licenseSvc.updateUserLicence($scope.model).$promise.then(successUpdateUser, failedUpdateUser);

    }

    $scope.back = function () {
        $window.history.back();
    }

    var successUpdateUser = function (data) {
        $scope.$parent.updateUserLicence(data.License);
        utilitiesSvc.showOKMessage(messagesType.message, 'Account Details updated successfully', buttonsType.OK);
        $window.history.back();
    };

    var failedUpdateUser = function (error) {
        utilitiesSvc.showOKMessage(messagesType.error, error.Messages[0].Text, buttonsType.OK)
    };

    var initGridParams = function () {

        $scope.tableParams = new ngTableParams({
            page: 1,            // show first page
            count: 24         // count per page

        }, {
            total: $scope.data.length, // length of data
            counts: [],
            getData: function ($defer, params) {

                params.total($scope.data.length);

                $defer.resolve($scope.data);

                setSelectBilling();
            }
        });
    }

    var initLookupData = function () {
        lookupSvc.getLookup().$promise.then(successGetLookup, failedGetLookup);
    }

    var successGetLookup = function (data) {

        $scope.stockMarket = [];
        var stock = data.Categories.StockMarket;
        for (var i = 0; i < stock.length; i++) {
            $scope.stockMarket.push({
                id: stock[i].iIndex, label: stock[i].strValue
            });
        }

        $scope.stockMarketSettings = {
            showCheckAll: false,
            showUncheckAll: false,
            idProp: 'label',
            scrollable: true,
            scrollableHeight: 120,

        };

        $scope.data = data.Services;

        $scope.countries = data.Countries;

        $scope.districts = data.Districts;

        $scope.genders = data.Genders;

        $scope.tableParams.reload();

    }

    var setSelectBilling = function () {

        $scope.servicePack.selectBillingRow = fullUser.Licence.Service.Idlicservice
    }

    var failedGetLookup = function (error) {

    };

    var init = function () {

        initGridParams();

        initLookupData();

        fullUser = $scope.$parent.getFullUser()

        $scope.selectCurrencySign = fullUser.Currency.CurrencySign;

        for (var i = 0; i < fullUser.Licence.Stocks.length; i++) {
            stockTemp.push(fullUser.Licence.Stocks[i]);
        }


        $scope.model = {
            Transaction: fullUser.Licence.Transaction,
            Stocks: stockTemp,
            UserID: fullUser.UserID,
            LicenseID: fullUser.Licence.LicenseID,
            SumInServer: null
        }
    }

    init();
}]);

