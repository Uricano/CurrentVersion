window.app = angular.module('takinApp', ['ui.select2', 'ngTable', 'ngRoute', 'ngResource', 'ui.bootstrap', 'custom-utilities', "cgPrompt", "ngSanitize", 'angularjs-dropdown-multiselect', 'pascalprecht.translate', 'ismobile', 'pasvaz.bindonce', 'takinApp.common']);//, 'wt.responsive'
window.app.config(['$routeProvider', '$locationProvider', '$httpProvider', '$provide', '$translateProvider', 'isMobileProvider', function ($routeProvider, $locationProvider, $httpProvider, $provide, $translateProvider, isMobileProvider) {
    $httpProvider.defaults.useXDomain = true;
    delete $httpProvider.defaults.headers.common['X-Requested-With'];

    // $httpProvider.defaults.headers.common['Identity-App'] = 'application/vnd.clal.employersportal.v1+json';
    $httpProvider.defaults.useXDomain = true;
    $locationProvider.html5Mode(true);
    $httpProvider.interceptors.push('httpInterceptor');
    $routeProvider

        .when('/', { templateUrl: function () { return 'scripts/app/views/shared/index.html' }, label: 'דף הבית' })
        .when('/portfolios', { templateUrl: function () { return 'scripts/app/views/portfolios/portfoliosList.html' }, controller: 'portfolioCtrl', label: 'portfolios' })
        .when('/portfolios/:id', { templateUrl: function () { return 'scripts/app/views/portfolios/portfolioDetails.html' }, controller: 'portfolioDetailsCtrl', label: 'portfolios' })
        .when('/optimizePortfolios/:name', { templateUrl: function () { return 'scripts/app/views/portfolios/optimizedPortfolio.html' }, controller: 'optimizedPortfolioCtrl', label: 'portfolios' })
        .when('/BuildNewPortfolio', { templateUrl: function () { return 'scripts/app/views/portfolios/customPortfolio.html' }, controller: 'customPortfolioCtrl', label: 'portfolios' })
        .when('/backtesting', { templateUrl: function () { return 'scripts/app/views/backtesting/backtesting.html' }, controller: 'backtestingCtrl', label: 'portfolios' })
        .when('/backtesting/:id', { templateUrl: function () { return 'scripts/app/views/backtesting/backtesting.html' }, controller: 'backtestingCtrl', label: 'portfolios' })
        .when('/createbacktesting', { templateUrl: function () { return 'scripts/app/views/backtesting/createBacktesting.html' }, controller: 'createBacktestingCtrl', label: 'portfolios' })
        .when('/forgetPassword', { templateUrl: function () { return 'scripts/app/views/shared/forgetPassword.min.html' }, controller: 'forgetPasswordCtrl' })
        .when('/newPassword', { templateUrl: function () { return 'scripts/app/views/shared/newPassword.min.html' }, controller: 'newPasswordCtrl' })
        .when('/registration', { templateUrl: function () { return 'scripts/app/views/shared/registration.min.html' }, controller: 'registrationCtrl' })
        .when('/registrationTrial', { templateUrl: function () { return 'scripts/app/views/shared/registrationTrialCode.html' }, controller: 'registrationTrialCodeCtrl' })
        .when('/registrationPayment', { templateUrl: function () { return 'scripts/app/views/shared/registration_billing.min.html' }, controller: 'registrationBillingCtrl' })
        .when('/changeAccount', { templateUrl: function () { return 'scripts/app/views/shared/changeAccountDetails.min.html' }, controller: 'changeAccountDetailsCtrl' })
        .when('/changeBilling', { templateUrl: function () { return 'scripts/app/views/shared/changeBillingDetails.min.html' }, controller: 'changeBillingDetailsCtrl' })

        .otherwise({
            redirectTo: '/'
        });
    //$locationProvider.html5Mode(true);

    // add translation tables
    $translateProvider.translations('en', translationsEN);
    $translateProvider.translations('he', translationsHE);
    $translateProvider.preferredLanguage('en');
    $translateProvider.fallbackLanguage('en');

}]);

window.utilities = angular.module("custom-utilities", []);

app.run(["$rootScope", "$location", "$document", "$window", "utilitiesSvc", "tfiCommonConstants", 'loginSvc',
    function ($rootScope, $location, $document, $window, utilitiesSvc, tfiCommonConstants, loginSvc) {

        var lastDigestRun = new Date();
        //    var wantToLogout;

        //define event when user did something
        $document.find('body').on('mousemove keydown DOMMouseScroll mousewheel mousedown touchstart', checkAndResetIdle); //monitor events

        function checkAndResetIdle() //user did something
        {
            var now = new Date();
            //check if user did  last time before 18 minutes
            if (now - lastDigestRun > (1000 * 60 * 18)) {
                lastDigestRun = now;
                $rootScope.$broadcast('idleLogoutEvent');
            }
            else {
                lastDigestRun = now;
            }
        }

        $rootScope.$on("$routeChangeStart", function (event, next, current) {
            //        if (wantToLogout == null) {
            //            wantToLogout = false;
            //        }
            //
            try {
                if (next.$$route["originalPath"] != undefined) {
                    window.scrollTo(0, 0);
                    if (current != undefined && next.$$route.originalPath == '/' && current.$$route.originalPath == '/portfolios' && !$rootScope.$$childHead.wantToLogout && sessionStorage.getItem("user") != null) {

                        event.preventDefault();
                        utilitiesSvc.showYesNoMessage("message", 'are you sure you want to leave?', "yes", "no").then(function (result) {
                            $rootScope.$$childHead.wantToLogout = true;
                            sessionStorage.removeItem("user");
                            sessionStorage.removeItem('userDetails');
                            $location.path('/');

                        });
                    }

                    else if (next.$$route.originalPath == '/portfolios/') {
                        //                    $rootScope.$broadcast(tfiCommonConstants.wantToLogout, false);
                        $rootScope.$$childHead.wantToLogout = false;
                    }
                    else {
                        $rootScope.$$childHead.isIndexPage = next.$$route.originalPath == '/';
                        $rootScope.$$childHead.isLoginPage = next.$$route.originalPath == '/' || next.$$route.originalPath == '/forgetPassword' || next.$$route.originalPath == '/newPassword';
                        $rootScope.$$childHead.isRegistrationPage = next.$$route.originalPath == '/registration' || next.$$route.originalPath == '/registrationTrial' || next.$$route.originalPath == '/registrationPayment';
                        $rootScope.$$childHead.isOpen = false;
                        if (!$rootScope.$$childHead.isIndexPage) {
                            var url = next.$$route.originalPath.substring(1).split("/");
                            $rootScope.$$childHead.screenName = screens[url[url.length - 1]];
                        }
                        if (current == null) {
                            Object.keys(sessionStorage).forEach(function (k) {
                                if (k != "user" && k != "sessionid") sessionStorage.removeItem(k);
                            });
                        }
                    }

                    if (next.$$route.originalPath == '/') {
                        current.scope.$parent.logout();
                    }
              
                }
            }
            catch (e) { }
        });

        $rootScope.$on('$locationChangeSuccess', function () {
            if ($window.Appcues) {
                $window.Appcues.start();
            }
            
        });
    }]);

//Smooch.destroy();

