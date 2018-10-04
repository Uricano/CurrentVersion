app.controller("registrationCtrl", ['$scope', "$location", "$window", "lookupSvc", "utilitiesSvc", "$timeout", "ngTableParams", '$filter', 'userSvc', 'licenseSvc', '$rootScope', function ($scope, $location, $window, lookupSvc, utilitiesSvc, $timeout, ngTableParams, $filter, userSvc, licenseSvc, $rootScope) {

    var flag;

    $scope.screenName = "";

    $scope.user = {};

    $scope.data = [];

    $scope.years = [];

    $scope.validTrailForm = false;

    $scope.submitForm = false;
    
    $scope.model = {
        User: {
            Licence: {
                Service: {},
                Stocks: [],
                Transaction: {}
            }
        }


    }
   
    $scope.checkConfirmEmail = function () {


        if ($scope.model.User.Email) $scope.model.User.Email = $scope.model.User.Email.trim();
        if ($scope.confirmEmail) $scope.confirmEmail = $scope.confirmEmail.trim();


        if ($scope.confirmEmail != '' && $scope.confirmEmail != null && $scope.model.User.Email != null && $scope.model.User.Email != '' && $scope.confirmEmail != $scope.model.User.Email) {
            //   $scope.isDiffrentEmail = $scope.confirmEmail != '' && $scope.confirmEmail != null &&
            //                          $scope.model.User.Email != null && $scope.model.User.Email != '' &&
            //                         $scope.confirmEmail != $scope.model.User.Email;
            utilitiesSvc.showOKMessage('error', 'Confirm email not equal to email', 'OK',false, true);
            return;

        }
    }

    //$scope.sendAuthentication = function (form) {
    $scope.next = function (form) {

        $scope.submitForm = true;

        var errorStr = '';
        var minYearbirth = ((new Date).getFullYear() - 18);
        if (form.$invalid) {
            errorStr = errorStr + 'Please fill all required fields <br/>';
        }

        //if (($scope.model.User.Username && !/^([ a-zA-Z0-9])*$/.test($scope.model.User.Username))) {
        //    errorStr = errorStr + 'Insert only english characters and digits to userName<br/>';
        //    form.username.$invalid = true;
        //}


        if (($scope.model.User.Name && !/^([ a-zA-Z0-9])*$/.test($scope.model.User.Name))) {
            errorStr = errorStr + 'Insert only english characters and digits to name<br/>';
            form.fullname.$invalid = true;
        }

        if (($scope.model.Password && !/^(?=.*[0-9])(?=.*[a-zA-Z])([a-zA-Z0-9]+)$/.test($scope.model.Password))) {
            errorStr = errorStr + 'Invalid password <br/>';
            form.passInput.$invalid = true;
        }
        if (($scope.confirmPassword && !/^(?=.*[0-9])(?=.*[a-zA-Z])([a-zA-Z0-9]+)$/.test($scope.confirmPassword))) {
            errorStr = errorStr + 'Invalid confirm password <br/>';
            form.passInput2.$invalid = true;
        }

        if ($scope.model.Password != $scope.confirmPassword) {
            errorStr = errorStr + 'Your password not equal to confirm password <br/>';
            // return;
        }
        //if ($scope.model.User.YearOfBirth > minYearbirth) {
        //    errorStr = errorStr + 'Year of Birth must be smaller than ' + minYearbirth.toString() + ' <br/>';
        //    form.yearBirthDate.$invalid = true
        //}

        if (($scope.model.User.Email && !/^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$/.test($scope.model.User.Email))) {
            errorStr = errorStr + 'Invalid Email <br/>';
            form.email.$invalid = true;
        }
        if (($scope.confirmEmail && !/^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$/.test($scope.confirmEmail))) {
            errorStr = errorStr + 'Invalid confirm Email <br/>';
            form.confirmEmail.$invalid = true;
        }
        if ($scope.model.User.Email != $scope.confirmEmail) {
            errorStr = errorStr + 'Your email not equal to confirm email <br/>';
            // return;
        }

        if (errorStr != '') {
            utilitiesSvc.showOKMessage('error', errorStr, 'OK',false, true);
            return;
        }

        $scope.model.User.Username = $scope.model.User.Email;

        var params = {
            "Username": $scope.model.User.Email
        }

        userSvc.verifyUsername(params).$promise.then(
            function() {
                sessionStorage.setItem('tempUser', JSON.stringify($scope.model));
                $location.path('/registrationTrial');
            });
    }

    $scope.verifyConfirmCode = function () {

        if (!$scope.confirmCodeValue)
            return;
        userSvc.verifyConfirmCode({ "ConfirmCode": $scope.confirmCodeValue }).$promise.then(successVerifyConfirmCode, failedVerifyConfirmCode);

    };
    
    //$scope.checkValidUserName = function () {
    //    $scope.model.User.Username = $scope.model.User.Username.trim();
    //    if (!/^[A-Za-z]+$/.test($scope.newPort.Name)) {//&& !/^[0-9]*[A-Za-z]+$/.test($scope.newPort.Name) && !/^[0-9]*[A-Za-z]*[0-9]+$/.test($scope.newPort.Name)) {
    //        utilitiesSvc.showOKMessage('error', 'Invalid portfolio name – Please enter a name up to 10 characters using letters or/and numbers only', 'OK');
    //        return false;
    //    }
    //}
    
   
    var successSendAuthenticatione = function (data) {
        utilitiesSvc.showOKMessage('message', "Authentication code sent to your email", 'OK',false,true);
    };

    var failedSendAuthentication = function (error) {
        utilitiesSvc.showOKMessage('error', error[0] ? error[0].Text : error.data.Message, 'OK',false,true);
    }
    
    var successVerifyConfirmCode = function (data) {
        //utilitiesSvc.showOKMessage('message', "Validation passed successfully", 'OK');
        $scope.model.User.Username = $scope.model.User.Email;
        sessionStorage.setItem('tempUser', JSON.stringify($scope.model));
        $location.path('/registrationPayment');
    };

    var failedVerifyConfirmCode = function (error) {
        utilitiesSvc.showOKMessage('error', error[0] ? error[0].Text : error.data.Message, 'OK',false, true);
        $scope.canCalculate = false;
    };

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

        $scope.currencies = data.Currencies;
        
    }

    var failedGetLookup = function (error) {

    };

    var init = function () {

        initLookupData();

        $scope.currentYear = (new Date()).getFullYear();

        for (var i = $scope.currentYear - 18; i > $scope.currentYear - 18 - 60; i--) {
            $scope.years.push(i);
        }
    }

    init();
}]);