app.controller("newPasswordCtrl", ['$scope', "$location", "$window", "utilitiesSvc", "loginSvc", function ($scope, $location, $window, utilitiesSvc, loginSvc) {

    $scope.verifyPassword;


    $scope.onKeyPress = function ($event) {
        if ($event.keyCode == 13) { //enter
            $scope.continue();
        }
    };

    $scope.continue = function () {

        if (!$scope.params || $scope.params.NewPassword == null || $scope.params.OldPassword == null || $scope.verifyPassword == null || $scope.params.Email == null||
             $scope.params.NewPassword == '' || $scope.params.OldPassword == '' || $scope.verifyPassword == '' || $scope.params.Email == '') {
            utilitiesSvc.showOKMessage('error', 'Please fill up all the fields', 'OK');
            return;
        }

        if ($scope.params.NewPassword != $scope.verifyPassword) {
            utilitiesSvc.showOKMessage('message', 'New password not equal to Verify password', 'OK');
            return;
        }

        loginSvc.updatePasswrod($scope.params).$promise.then(succsessUpdatePassword, failedUpdatePassword);
    }

    var succsessUpdatePassword = function (data) {
        if (data.Messages && data.Messages.length > 0) {
            utilitiesSvc.showOKMessage('error', data.Messages[0].Text, 'OK');
        }
        else
        {
            utilitiesSvc.showOKMessage('message', "Password updated successfully.", 'OK', false, true);
            $location.path('/');
        }
    };

    var failedUpdatePassword = function (error) {

    };

    var initialize = function () {

        $scope.params = $scope.$parent.getUserDetails();

    }

    initialize();
}]);