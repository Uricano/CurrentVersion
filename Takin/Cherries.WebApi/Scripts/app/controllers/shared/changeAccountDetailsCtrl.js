app.controller("changeAccountDetailsCtrl", ['$scope', "$location", "lookupSvc", "utilitiesSvc", '$filter', 'userSvc', '$window', function ($scope, $location, lookupSvc, utilitiesSvc, $filter, userSvc, $window) {

    $scope.model = {};

    $scope.updateUserDetails = function (form) {
        if (form.$invalid) {
            utilitiesSvc.showOKMessage('error', 'Please fill all required fields', 'OK');
            return;
        }
        $scope.model.User.YearOfBirth = Number($scope.model.User.YearOfBirth);
        $scope.model.User.Currency = $filter('filter')($scope.currencies, { CurrencyId: $scope.model.User.Currency.CurrencyId }, true)[0];
        userSvc.updateUesrDetails($scope.model).$promise.then(successUpdateUser, failedUpdateUser);
    }

    $scope.back = function () {
        $window.history.back();
    }

    var successUpdateUser = function (data) {
        utilitiesSvc.showOKMessage(messagesType.message, 'Account details updated successfully', buttonsType.OK);
    };

    var failedUpdateUser = function (error) {
        utilitiesSvc.showOKMessage(messagesType.error, error.Messages[0].Text, buttonsType.OK)
    };

    var initLookupData = function () {
        lookupSvc.getLookup().$promise.then(successGetLookup, failedGetLookup);
    }

    var successGetLookup = function (data) {

        $scope.countries = data.Countries;

        $scope.districts = data.Districts;

        $scope.genders = data.Genders;

        $scope.currencies = data.Currencies;

        $scope.model = { "User": $scope.$parent.getFullUser() };

    };

    var failedGetLookup = function (error) {

    };

    var init = function () {

        initLookupData();

        $scope.currentYear = (new Date()).getFullYear();
    }

    init();
}]);

