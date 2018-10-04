app.controller("forgetPasswordCtrl", ['$scope', "$location", "$window", "loginSvc", "utilitiesSvc", "prompt", function ($scope, $location, $window, loginSvc, utilitiesSvc, prompt) {

    $scope.password;

    $scope.params = {}

    $scope.onKeyPress = function ($event) {
        if ($event.keyCode == 13) { //enter
            $scope.continue();
        }
    };

    $scope.continue = function () {

        if (!$scope.params || $scope.params.Email == null || $scope.params.Email == '') {
            utilitiesSvc.showOKMessage('error', 'Please enter a valid e-mail address', 'OK', false, true);
            return;
        }
        loginSvc.updatePasswrod($scope.params).$promise.then(succsessUpdatePassword, failedUpdatePassword);
    }

    var succsessUpdatePassword = function (data) {
        if (data.Messages && data.Messages.length > 0) {
            utilitiesSvc.showOKMessage('error', data.Messages[0].Text, 'OK');
        }
        else {
            $scope.$parent.saveUserDetails($scope.params);
            prompt({
                title: "message",
                message: "New temporary password was sent to your E-mail",
                closeAfter: 3,
                buttons: []
            }).then(function (result) {
                $location.path('newPassword/');
            }, function () {                
                $location.path('newPassword/');
            });
        }
    }

    var failedUpdatePassword = function (error) {

    }

    var initialize = function () {

    }

    initialize();
}]);