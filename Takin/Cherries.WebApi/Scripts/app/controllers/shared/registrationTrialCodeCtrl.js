app.controller("registrationTrialCodeCtrl", ['$scope', "$location", "utilitiesSvc", '$filter', 'userSvc', 'lookupSvc',
    function ($scope, $location, utilitiesSvc, $filter, userSvc, lookupSvc)
    {
        $scope.termCondPolicy = false;

    $scope.screenName = "";

    $scope.user = {};

    $scope.data = [];

    $scope.model = {
        User: {
            Licence: {
                Service: {},
                Stocks: [],
                Transaction: {}
            }
        }
    }

    $scope.saveTrialUser = function ()
    {
        if (!$scope.termCondPolicy)
        {
            utilitiesSvc.showOKMessage('error', "You must agree to Cherries' Terms & Conditions and Privacy Policy in order to create an account", 'OK', false, true);
            return;
        }

        if (!$scope.model.Cupon || $scope.model.Cupon == '')
        {
            utilitiesSvc.showOKMessage('error', "Invalid coupon code", 'OK', false, true);
            return;
        }
        var today = new Date();
        setFullModel(true, 4);
        $scope.model.User.Licence.ExpiryDate = $filter('date')(new Date(today.setDate(today.getDate() + 14)), 'MMM dd/yy');
        $scope.model.User.Licence.Transaction = { TransactionDate: $filter('date')(today, 'MMM dd/yy') };
        $scope.model.User.Licence.isTrial = true;
        userSvc.createUser($scope.model).$promise.then(successCreateUser, failedCreateUser);
    }

    var setFullModel = function (isTrail, idService)
    {
        var today = new Date();
        $scope.model.User.Licence.Licensetype = isTrail ? Licensetype.Trial : Licensetype.FullLicense;
        $scope.model.User.Licence.Service.Idlicservice = idService;

        $scope.model.User.Licence.ActivationDate = $filter('date')(today, 'MMM dd/yy');
        $scope.model.User.Licence.PurchaseDate = $filter('date')(today, 'MMM dd/yy');
    }

    var successCreateUser = function (data)
    {
        utilitiesSvc.showOKMessage('message', "User created successfully", 'OK', false, true);
        $location.path('/');
    }

    var failedCreateUser = function (error)
    {
        utilitiesSvc.showOKMessage('message', error[0] ? error[0].Text : error.data.Message, 'OK');
    }

    var successGetLookup = function (data)
    {
        $scope.stockMarket = [];
        var stock = data.Categories.StockMarket;
        for (var i = 0; i < stock.length; i++)
        {
            $scope.stockMarket.push({
                id: stock[i].iIndex, label: stock[i].strValue
            });
        }

        // TOOD: While stock exchange editor is hidden select all stocks.
        $scope.model.User.Licence.Stocks = $scope.stockMarket;
    }

    var failedGetLookup = function (error) {
        utilitiesSvc.showOKMessage(messagesType.error, error.Messages[0].Text, buttonsType.OK);
    };

    var initLookupData = function () {
        $scope.formReady = false;
        lookupSvc.getLookup().$promise
            .then(successGetLookup)
            .catch(failedGetLookup)
            .finally(function() {
                $scope.formReady = true;
            });
    };

    var init = function () {

        $scope.model = JSON.parse(sessionStorage.getItem('tempUser'));
        initLookupData();
    }

    init();
}]);