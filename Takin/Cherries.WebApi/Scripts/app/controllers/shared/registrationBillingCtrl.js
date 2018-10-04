app.controller("registrationBillingCtrl", ['$scope', "$location", "$window", "lookupSvc", "utilitiesSvc", "$timeout", "ngTableParams", '$filter', 'userSvc', 'licenseSvc', '$rootScope', function ($scope, $location, $window, lookupSvc, utilitiesSvc, $timeout, ngTableParams, $filter, userSvc, licenseSvc, $rootScope) {

    var flag;

    $scope.screenName = "";

    $scope.user = {};

    $scope.data = [];

    $scope.enablePaypal = false;

    $scope.validTrailForm = false;

    $scope.submitForm = false;

    $scope.servicePack = null;

    $scope.model = {
        User: {
            Licence: {
                Service: {},
                Stocks: [],
                Transaction: {}
            }
        }


    }

    $rootScope.$on('paypalPaymentComplited', function (event, paymentResult) {
        save(paymentResult);
    });

    $scope.saveTrailUser = function () {

        if ($scope.model.Cupon == '') {
            utilitiesSvc.showOKMessage('error', "You must enter a vaild cupon", 'OK', false, true);
            return;
        }
        var today = new Date();
        setFullModel(true, 4);
        $scope.model.User.Licence.ExpiryDate = $filter('date')(new Date(today.setDate(today.getDate() + 14)), 'MMM dd/yy');
        $scope.model.User.Licence.Stocks = $scope.stockMarket;
        $scope.model.User.Licence.Transaction = { TransactionDate: $filter('date')(today, 'MMM dd/yy') };
        $scope.model.User.Licence.isTrial = true;
        userSvc.createUser($scope.model).$promise.then(successCreateUser, failedCreateUser);
    }

    $scope.calculateLicense = function (service) {
        $scope.servicePack = service;        
        if (!isValidPaymentParams(true)) {
            $scope.servicePack = null;
            return;
        }


        $scope.validForm = true;

        //var selectBilling = $filter('filter')($scope.data, { isSelect: 'true' }, true)[0];

        var params = {
            ServiceID: service.Idlicservice,
            StockCount: $scope.model.User.Licence.Stocks.length
        }

        licenseSvc.calculateLicense(params).$promise.then(function (data) {
            $scope.calcValue = data.Sum;
            $scope.enablePaypal = false; //$scope.canUsePaypal();
        }, function (error) {
            utilitiesSvc.showOKMessage('error', error.data, 'OK', false, true);
        });
    }
       
    $scope.canUsePaypal = function () {
        return !$scope.model.User.Licence.isTrial && isValidPaymentParams(false) && $scope.calcValue > 0;
    }

    $scope.setSelectCurrencySign = function () {

        $scope.selectCurrencySign = $filter('filter')($scope.currencies, { CurrencyId: $scope.model.User.Currency.CurrencyId }, true)[0].CurrencySign;
    }
    

    var isValidPaymentParams = function (showMsg) {

        //var selectBilling = $filter('filter')($scope.data, { isSelect: 'true' }, true)[0];
        if ($scope.servicePack == null) {
            $scope.validForm = false;
            if (showMsg) utilitiesSvc.showOKMessage('error', "You must choose your billing plan", 'OK', false);
            return false;
        }

        if (!$scope.model || $scope.model.User.Licence.Stocks.length < 2) {
            $scope.validForm = false;
            if (showMsg) utilitiesSvc.showOKMessage('error', "You must choose at least 2 stocks", 'OK', true);
            return false;
        }

        return true;

    }

    var setFullModel = function (isTrail, idService) {
        var today = new Date();
        $scope.model.User.Licence.Licensetype = isTrail ? Licensetype.Trial : Licensetype.FullLicense;
        $scope.model.User.Licence.Service.Idlicservice = idService;

        $scope.model.User.Licence.ActivationDate = $filter('date')(today, 'MMM dd/yy');
        $scope.model.User.Licence.PurchaseDate = $filter('date')(today, 'MMM dd/yy');
    }

    var save = function (paymentResult) {
        if (flag)
            return;
        var today = new Date();
        // var selectBilling = $filter('filter')($scope.data, { isSelect: 'true' }, true)[0];

        if (!$scope.validForm)
            return;

        setFullModel(false, $scope.servicePack.Idlicservice);
        $scope.model.User.Licence.ExpiryDate = $filter('date')(new Date(today.setMonth(today.getMonth() + $scope.servicePack.Imonths)), 'MMM dd/yy');;
        $scope.model.User.Licence.Transaction = {
            PaypalReceiptID: paymentResult.id,
            TransactionID: paymentResult.transactions[0].related_resources[0].sale.id,
            TransactionDate: $filter('date')(today, 'MMM dd/yy'),
            PaypalUserID: paymentResult.payer.payer_info.payer_id,
            idCurrency: 9001, //$scope.model.User.Currency.CurrencyId,
            dSum: paymentResult.transactions[0].amount.total,
            TransAnswer: paymentResult.transactions[0].related_resources[0].sale.state
        }
        flag = true;
        userSvc.createUser($scope.model).$promise.then(successCreateUser, failedCreateUser);

    }

    var successCreateUser = function (data) {

        utilitiesSvc.showOKMessage('message', "User created successfully", 'OK', false, true);
        $location.path('/');

    }

    var failedCreateUser = function (error) {
        //utilitiesSvc.showOKMessage('message', error[0] ? error[0].Text : error.data.Message, 'OK');
    }

    var initLookupData = function () {
        lookupSvc.getLookup().$promise.then(successGetLookup, failedGetLookup);
    }

    var successGetLookup = function (data) {

        $scope.Services = data.Services;

        $scope.currencies = data.Currencies;

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
    }

    var failedGetLookup = function (error) {

    };

    var init = function () {
               
        initLookupData();

        $scope.model = JSON.parse(sessionStorage.getItem('tempUser'));
    }

    init();
}]);