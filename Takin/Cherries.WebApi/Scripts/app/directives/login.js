app.directive('login', ["$location", "$modal", "loginSvc", "utilitiesSvc", function ($location, $modal, loginSvc, utilitiesSvc) {
    return {
        restrict: 'AE',
        templateUrl: 'scripts/app/partials/login.min.html',
        scope: {},
        compile: compileDirective
    }

    function compileDirective(tElement)
    {
        tElement.addClass('login');
        return linkDirective;
    }

    function linkDirective(scope, element, attrs, ngModelCtrl)
    {
        scope.onKeyPress = function ($event) {
            if ($event.keyCode == 13) { //enter
                scope.enterApplicant();
            }
        };

        scope.enterApplicant = function () {
            loginSvc.get(scope.user).then(function (data) {

                if (data.User == null) {
                    utilitiesSvc.showOKMessage(messagesType.error, "There is no user", buttonsType.OK, true,true);
                    return;
                }
                else if (data.Messages.length > 0) {
                    utilitiesSvc.showOKMessage(messagesType.error, data.Messages[0].Text, buttonsType.OK, true,true);
                }

                scope.$parent.$parent.saveUser(data.User);
                scope.$parent.$parent.saveAccountData(data.User);
                scope.$parent.$parent.saveFullUser(data);

                if (data.User.isTemporary) {
                    scope.$parent.$parent.saveUserDetails({ 'UserName': data.User.Username });
                    $location.path('newPassword/');
                    return;
                }

                $location.path('portfolios/');

                if (new Date(data.User.Licence.ExpiryDate) < new Date()) {
                    utilitiesSvc.showOKMessage(messagesType.error, 'Your license has expired. You can no longer build new portfolios. Please contact us at info@gocherries.com to renew your license.', buttonsType.OK, true,true);
                }

            }, function (error)
            {
                console.log(error);
                //utilitiesSvc.showOKMessage(messagesType.message, error[0].Text, buttonsType.OK, true);
            });
        };

        scope.forgetPassword = function () {

            $location.path('forgetPassword/');
        }

        scope.logout = function () {
            loginSvc.logoff().$promise.then(function (result) {
                //                            $rootScope.$broadcast(tfiCommonConstants.wantToLogout, true);
                sessionStorage.clear();

            });
        }

        var init = function () {

            scope.user = {
                UserName: '',
                Password: ""
            }
               
        }

        init();
    }
}]);