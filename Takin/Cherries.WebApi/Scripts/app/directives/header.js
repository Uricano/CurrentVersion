app.directive('cstHeader', ["$modal", "utilitiesSvc", "$location", "$window", "$location", "loginSvc", function ($modal, utilitiesSvc, $location, $window, $location, loginSvc) {
    return {
        restrict: 'EA',
        replace: true,
        templateUrl: 'Scripts/app/partials/header.html',
        link: function (scope, element, attrs, ngModelCtrl) {

            scope.selectMenu = 0;

            scope.saveAccountData = function (user) {

                scope.marketStock = user.Licence.Stocks;
                for (var i = 0; i < scope.marketStock.length; i++) {
                    scope.marketStock[i].Name = scope.marketStock[i].Name.substring(0,scope.marketStock[i].Name.indexOf('Stock E'));
                }
                scope.expiryDate = user.Licence.ExpiryDate;
                scope.daysLeft = getDaysBetween(new Date(scope.expiryDate), new Date());
                scope.licenseType = user.Licence.Service.StrServiceType
            }

            scope.pageClick = function ($event) {
                if ($event.target.id != 'accountDiv') {
                    scope.showAccount = false;
                }
            }

            scope.back = function () {
                $window.history.back();
            }

            scope.logout = function () {
                loginSvc.logoff().$promise.then(function (result) {
                    //Smooch.destroy();
                    scope.clearSession();
                    scope.isDataReady = false;
                    scope.isInProgress = false;
                    scope.wantToLogout = true;
                    $location.path("/");
                });
                
            }

            scope.showOrhideAccount = function () {
                scope.showAccount = !scope.showAccount;
            }


            scope.isHomePage = function () {
                return $location.$$path == '/portfolios';
                
            }

            scope.toggleMobileMenu = function () {
                
                scope.isMobileOpen = !scope.isMobileOpen;
               
            }

            scope.updateDetail = function (url) {
                scope.showAccount = false;
                $location.path(url);
            }

            var getDaysBetween = function (date1, date2) {

                var ONE_DAY = 1000 * 60 * 60 * 24;

                // Convert both dates to milliseconds
                var date1_ms = date1.getTime();
                var date2_ms = date2.getTime();

                // Calculate the difference in milliseconds
                var difference_ms = date1_ms - date2_ms

                // Convert back to days and return
                return Math.round(difference_ms / ONE_DAY);
            }

            var init = function () {
                //scope.name = scope.getUser();

                var user = scope.getUser();
                if (user)
                    scope.saveAccountData(user);
            }

            init();
        }
    };
}]);