var translationsEN = {
    Home: 'Home',
    Portfolio: 'Portfolio',
    Backtesting: 'Backtesting',
    Greeting: "Hello ",
    Help: "Help",
    Account: "My Account",
    Logout:"Logout"
};

var translationsHE = {
    Home: 'בית',
    Portfolio: 'תיקים',
    Backtesting: "חישוב לאחור",
    Greeting: "שלום ",
    Help: "עזרה",
    Account: "החשבון שלי",
    Logout: "יציאה"
};
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


app.controller("takinMngrCtrl", ['$scope', "$location", "$window", "$translate", "lookupSvc", "portfolioSvc", "utilitiesSvc", "loginSvc", "$timeout", "$http", "isMobile", "backtestingSvc", "$document",
    function ($scope, $location, $window, $translate, lookupSvc, portfolioSvc, utilitiesSvc, loginSvc, $timeout, $http, isMobile, backtestingSvc, $document)
    {

        // Alex: 18.10.2018

    $scope.screenName = "";

    $scope.rout = "";

    $scope.isLoginPage;

    $scope.isRegistrationPage;

    $scope.language = 'en';

    $scope.userName;

    $scope.isDataReady = false;
    $scope.isInProgress = false;
    $scope.isMobile = isMobile.phone;
    $scope.objName;
    isIndexPage = true;

    var currentProject;

    var user;

    var portfolio;

    var newBacktesting;

    var lookupData;

    var benchmarkSecurities;

    var backtestingParams;

    var newBacktestingParams;

    var newPortfolioData;

    var newBacktestingData;

    var UserDetails;

    var hub;

    var portfolioList;

    var fullUser;

    var isBackTesting = false;

    var progressDialogVisible = false;

    var setUserToken = function () {
        if (!$http.defaults.headers.common["Authorization"]) {
            if (sessionStorage.getItem('sessionid') && sessionStorage.getItem('sessionid') != "undefined") {
                var id = angular.fromJson(sessionStorage.getItem('sessionid'));
                $http.defaults.headers.common["Authorization"] = "Basic " + id;
            }
            else
            {
                var currentPath = $location.path();
                if (currentPath != '/registration' && currentPath != '/forgetPassword')
                {
                    //Smooch.destroy();
                    $location.path('/');
                }
            }
        }
    }

    $scope.back = function () {
        $window.history.back();
        if ($window.location.hash == "#maincontent") {
            $window.history.back();
        }
    };

    $scope.setLanguage = function () {
        $translate.use($scope.language);
    }

    $scope.saveUser = function (userDeatails) {
        user = userDeatails;
        $scope.userName = userDeatails.Name;
        sessionStorage.setItem("user", userDeatails.Username);

        sessionStorage.setItem('userDetails', angular.toJson(user));
        Appcues.identify(user.UserID, { // Unique identifier for current user
            name: user.Name, // Current user's name
            email: user.Email, // Current user's email
            created_at: Date.now() // Unix timestamp of user signup date
        });
        reconnectToHub();
    };

    $scope.getUser = function (userDeatails) {
        if (!user) {
            user = angular.fromJson(sessionStorage.getItem('userDetails'));
        }
        return user;
    };

    $scope.saveNewPortfolio = function (port) {
        portfolio = port;
        sessionStorage.setItem('newPortfolio', angular.toJson(portfolio));
    };

    $scope.saveNewBacktesting = function (backtesting) {
        newBacktesting = backtesting;
        sessionStorage.setItem('newBacktesting', angular.toJson(backtesting));
    };

    $scope.saveUserDetails = function (params) {
        UserDetails = params;
        sessionStorage.setItem('userDetails', angular.toJson(UserDetails));
    };

    $scope.saveNewBacktestingParams = function (params) {
        newBacktestingParams = params;
        sessionStorage.setItem('newBacktestingParams', angular.toJson(newBacktestingParams));
    }

    $scope.saveBacktestingParams = function (params) {
        backtestingParams = params;
        sessionStorage.setItem('backtestingParams', angular.toJson(backtestingParams));
    };

    $scope.getBacktestingParams = function () {
        if (!backtestingParams)
            backtestingParams = angular.fromJson(sessionStorage.getItem('backtestingParams'));
        return backtestingParams;
    }

    $scope.getNewBacktestingParams = function () {
        if (!newBacktestingParams)
            newBacktestingParams = angular.fromJson(sessionStorage.getItem('newBacktestingParams'));
        return newBacktestingParams;
    }

    $scope.getUserDetails = function () {
        if (!UserDetails)
            UserDetails = angular.fromJson(sessionStorage.getItem('userDetails'));
        return UserDetails;
    };

    $scope.updateUserLicence = function (Licence) {
        user.Licence = Licence;
        sessionStorage.setItem('userDetails', angular.toJson(user));
        $scope.saveAccountData(user);
        $scope.getUserStocks();
    }

    $scope.saveFullUser = function (data) {
        fullUser = data;
    }

    $scope.getFullUser = function () {
        //if (!user)
        user = angular.fromJson(sessionStorage.getItem('userDetails'));
        return user;
    }

    $scope.getPortfolioList = function () {
        if (!portfolioList)
            portfolioList = angular.fromJson(sessionStorage.getItem('myPortfolioList'));
        return portfolioList;
    }

    $scope.setPortfolioList = function (portfolios) {
        portfolioList = portfolios;
        sessionStorage.setItem('myPortfolioList', angular.toJson(portfolioList));
    }

    $scope.getNewPortfolio = function (name) {
        if (!portfolio) {
            portfolio = angular.fromJson(sessionStorage.getItem('newPortfolio'));
        }

        if (portfolio && portfolio.PortDetails && portfolio.PortDetails.Name == name)
            return portfolio;
        return null;
    };

    $scope.getNewBacktesting = function (name) {
        if (!newBacktesting) {
            newBacktesting = angular.fromJson(sessionStorage.getItem('newBacktesting'));
        }

        //if (newBacktesting && newBacktesting.Name == name)
        return newBacktesting;
        //  return null;
    };

    $scope.saveBenchmarkSecurities = function (data) {
        benchmarkSecurities = data;
        sessionStorage.setItem('benchmarkSecurities', angular.toJson(benchmarkSecurities));
    };

    $scope.getBenchmarkSecurities = function () {
        if (!benchmarkSecurities)
            benchmarkSecurities = angular.fromJson(sessionStorage.getItem('benchmarkSecurities'));
        return benchmarkSecurities;
    };

    $scope.getUserStocks = function () {
        $scope.stockMarket = [];
        user = $scope.getFullUser();
        for (var i = 0; i < user.Licence.Stocks.length; i++) {
            $scope.stockMarket.push({
                id: user.Licence.Stocks[i].id, label: user.Licence.Stocks[i].Name
            });
        }
        //return stockMarket;
    }

    $scope.getLookupValues = function () {

        if (!lookupData)
            lookupData = angular.fromJson(sessionStorage.getItem('lookupData'));

        return lookupData;


    };

    $scope.clearSession = function () {
        Object.keys(sessionStorage).forEach(function (k) {
            if (k != "lookupData") sessionStorage.removeItem(k);
        });
    }

    $scope.createPortfolio = function (parameters, fromBackTesting) {
        $scope.isInProgress = true;
        isBackTesting = fromBackTesting;
        $scope.objName = isBackTesting ? 'Backtesting' : 'Portfolio';
        var service = isBackTesting ? backtestingSvc.createBacktesting : portfolioSvc.createPortfolio;
        service(parameters).$promise
            .then(function (data)
            {
                var msgText = data.Messages ? data.Messages[0].Text : data.Message;
                var dialogPromise = utilitiesSvc.showOKMessage('message', msgText, 'OK');
                if (!/you will be notified when the portfolio is ready/i.test(msgText))
                {
                    $scope.isInProgress = false;
                }
                else
                {
                    progressDialogVisible = true;
                    dialogPromise.finally(function ()
                    {
                        progressDialogVisible = false;
                    });
                }
            })
            .catch(function (error)
            {
                $scope.isInProgress = false;
                utilitiesSvc.showOKMessage('error', error[0] ? error[0].Text : error.data, 'OK');
            });
        //utilitiesSvc.showOKMessage('message', 'The <img src="content/themes/images/notification-outline.png" class="bell-image"> icon will appear at the top of the screen when the portfolio is ready for your review.', 'OK');
        //hub.server.createPortfolio(JSON.stringify(parameters));
    }

    $scope.getNewPortfolioObj = function () {
        if (!newPortfolioData)
            newPortfolioData = angular.fromJson(sessionStorage.getItem('newPortfolio'));
        return newPortfolioData;
    }


    $scope.setBacktestingDetails = function (back) {
        newBacktesting = back;
        sessionStorage.setItem('newBacktesting', angular.toJson(newBacktesting));
    }

    function beep() {
        try {
            var snd = new Audio("data:audio/wav;base64,//uQRAAAAWMSLwUIYAAsYkXgoQwAEaYLWfkWgAI0wWs/ItAAAGDgYtAgAyN+QWaAAihwMWm4G8QQRDiMcCBcH3Cc+CDv/7xA4Tvh9Rz/y8QADBwMWgQAZG/ILNAARQ4GLTcDeIIIhxGOBAuD7hOfBB3/94gcJ3w+o5/5eIAIAAAVwWgQAVQ2ORaIQwEMAJiDg95G4nQL7mQVWI6GwRcfsZAcsKkJvxgxEjzFUgfHoSQ9Qq7KNwqHwuB13MA4a1q/DmBrHgPcmjiGoh//EwC5nGPEmS4RcfkVKOhJf+WOgoxJclFz3kgn//dBA+ya1GhurNn8zb//9NNutNuhz31f////9vt///z+IdAEAAAK4LQIAKobHItEIYCGAExBwe8jcToF9zIKrEdDYIuP2MgOWFSE34wYiR5iqQPj0JIeoVdlG4VD4XA67mAcNa1fhzA1jwHuTRxDUQ//iYBczjHiTJcIuPyKlHQkv/LHQUYkuSi57yQT//uggfZNajQ3Vmz+Zt//+mm3Wm3Q576v////+32///5/EOgAAADVghQAAAAA//uQZAUAB1WI0PZugAAAAAoQwAAAEk3nRd2qAAAAACiDgAAAAAAABCqEEQRLCgwpBGMlJkIz8jKhGvj4k6jzRnqasNKIeoh5gI7BJaC1A1AoNBjJgbyApVS4IDlZgDU5WUAxEKDNmmALHzZp0Fkz1FMTmGFl1FMEyodIavcCAUHDWrKAIA4aa2oCgILEBupZgHvAhEBcZ6joQBxS76AgccrFlczBvKLC0QI2cBoCFvfTDAo7eoOQInqDPBtvrDEZBNYN5xwNwxQRfw8ZQ5wQVLvO8OYU+mHvFLlDh05Mdg7BT6YrRPpCBznMB2r//xKJjyyOh+cImr2/4doscwD6neZjuZR4AgAABYAAAABy1xcdQtxYBYYZdifkUDgzzXaXn98Z0oi9ILU5mBjFANmRwlVJ3/6jYDAmxaiDG3/6xjQQCCKkRb/6kg/wW+kSJ5//rLobkLSiKmqP/0ikJuDaSaSf/6JiLYLEYnW/+kXg1WRVJL/9EmQ1YZIsv/6Qzwy5qk7/+tEU0nkls3/zIUMPKNX/6yZLf+kFgAfgGyLFAUwY//uQZAUABcd5UiNPVXAAAApAAAAAE0VZQKw9ISAAACgAAAAAVQIygIElVrFkBS+Jhi+EAuu+lKAkYUEIsmEAEoMeDmCETMvfSHTGkF5RWH7kz/ESHWPAq/kcCRhqBtMdokPdM7vil7RG98A2sc7zO6ZvTdM7pmOUAZTnJW+NXxqmd41dqJ6mLTXxrPpnV8avaIf5SvL7pndPvPpndJR9Kuu8fePvuiuhorgWjp7Mf/PRjxcFCPDkW31srioCExivv9lcwKEaHsf/7ow2Fl1T/9RkXgEhYElAoCLFtMArxwivDJJ+bR1HTKJdlEoTELCIqgEwVGSQ+hIm0NbK8WXcTEI0UPoa2NbG4y2K00JEWbZavJXkYaqo9CRHS55FcZTjKEk3NKoCYUnSQ0rWxrZbFKbKIhOKPZe1cJKzZSaQrIyULHDZmV5K4xySsDRKWOruanGtjLJXFEmwaIbDLX0hIPBUQPVFVkQkDoUNfSoDgQGKPekoxeGzA4DUvnn4bxzcZrtJyipKfPNy5w+9lnXwgqsiyHNeSVpemw4bWb9psYeq//uQZBoABQt4yMVxYAIAAAkQoAAAHvYpL5m6AAgAACXDAAAAD59jblTirQe9upFsmZbpMudy7Lz1X1DYsxOOSWpfPqNX2WqktK0DMvuGwlbNj44TleLPQ+Gsfb+GOWOKJoIrWb3cIMeeON6lz2umTqMXV8Mj30yWPpjoSa9ujK8SyeJP5y5mOW1D6hvLepeveEAEDo0mgCRClOEgANv3B9a6fikgUSu/DmAMATrGx7nng5p5iimPNZsfQLYB2sDLIkzRKZOHGAaUyDcpFBSLG9MCQALgAIgQs2YunOszLSAyQYPVC2YdGGeHD2dTdJk1pAHGAWDjnkcLKFymS3RQZTInzySoBwMG0QueC3gMsCEYxUqlrcxK6k1LQQcsmyYeQPdC2YfuGPASCBkcVMQQqpVJshui1tkXQJQV0OXGAZMXSOEEBRirXbVRQW7ugq7IM7rPWSZyDlM3IuNEkxzCOJ0ny2ThNkyRai1b6ev//3dzNGzNb//4uAvHT5sURcZCFcuKLhOFs8mLAAEAt4UWAAIABAAAAAB4qbHo0tIjVkUU//uQZAwABfSFz3ZqQAAAAAngwAAAE1HjMp2qAAAAACZDgAAAD5UkTE1UgZEUExqYynN1qZvqIOREEFmBcJQkwdxiFtw0qEOkGYfRDifBui9MQg4QAHAqWtAWHoCxu1Yf4VfWLPIM2mHDFsbQEVGwyqQoQcwnfHeIkNt9YnkiaS1oizycqJrx4KOQjahZxWbcZgztj2c49nKmkId44S71j0c8eV9yDK6uPRzx5X18eDvjvQ6yKo9ZSS6l//8elePK/Lf//IInrOF/FvDoADYAGBMGb7FtErm5MXMlmPAJQVgWta7Zx2go+8xJ0UiCb8LHHdftWyLJE0QIAIsI+UbXu67dZMjmgDGCGl1H+vpF4NSDckSIkk7Vd+sxEhBQMRU8j/12UIRhzSaUdQ+rQU5kGeFxm+hb1oh6pWWmv3uvmReDl0UnvtapVaIzo1jZbf/pD6ElLqSX+rUmOQNpJFa/r+sa4e/pBlAABoAAAAA3CUgShLdGIxsY7AUABPRrgCABdDuQ5GC7DqPQCgbbJUAoRSUj+NIEig0YfyWUho1VBBBA//uQZB4ABZx5zfMakeAAAAmwAAAAF5F3P0w9GtAAACfAAAAAwLhMDmAYWMgVEG1U0FIGCBgXBXAtfMH10000EEEEEECUBYln03TTTdNBDZopopYvrTTdNa325mImNg3TTPV9q3pmY0xoO6bv3r00y+IDGid/9aaaZTGMuj9mpu9Mpio1dXrr5HERTZSmqU36A3CumzN/9Robv/Xx4v9ijkSRSNLQhAWumap82WRSBUqXStV/YcS+XVLnSS+WLDroqArFkMEsAS+eWmrUzrO0oEmE40RlMZ5+ODIkAyKAGUwZ3mVKmcamcJnMW26MRPgUw6j+LkhyHGVGYjSUUKNpuJUQoOIAyDvEyG8S5yfK6dhZc0Tx1KI/gviKL6qvvFs1+bWtaz58uUNnryq6kt5RzOCkPWlVqVX2a/EEBUdU1KrXLf40GoiiFXK///qpoiDXrOgqDR38JB0bw7SoL+ZB9o1RCkQjQ2CBYZKd/+VJxZRRZlqSkKiws0WFxUyCwsKiMy7hUVFhIaCrNQsKkTIsLivwKKigsj8XYlwt/WKi2N4d//uQRCSAAjURNIHpMZBGYiaQPSYyAAABLAAAAAAAACWAAAAApUF/Mg+0aohSIRobBAsMlO//Kk4soosy1JSFRYWaLC4qZBYWFRGZdwqKiwkNBVmoWFSJkWFxX4FFRQWR+LsS4W/rFRb/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////VEFHAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAU291bmRib3kuZGUAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAMjAwNGh0dHA6Ly93d3cuc291bmRib3kuZGUAAAAAAAAAACU=");
            snd.play();
        }
        catch (er) {

        }
    }

    var setLookupData = function () {
        lookupSvc.getLookup().$promise.then(function (data) {

            lookupData = data;
            sessionStorage.setItem('lookupData', angular.toJson(lookupData));

        }, function (error) { });

    }

    var reconnectToHub = function () {
        if (!user)
            $scope.getUser();
        if (user != null) {
            $.connection.hub.transportConnectTimeout = 20000;
            $.connection.hub.start({ jsonp: true }).done(function () {
                hub.server.registerClient(user.UserID);
            }).fail(function (reason) {
                $timeout(function () {
                    reconnectToHub();
                }, 100);
            });
            $.connection.hub.disconnected(function () {
                reconnectToHub();
            });
        }
    }

    var initialize = function () {

        setUserToken();

        setLookupData();

        if (angular.fromJson(sessionStorage.getItem('userDetails'))) {
            $scope.userName = angular.fromJson(sessionStorage.getItem('userDetails')).Name;
        }

        //window.addEventListener("beforeunload", function (event) {
        //    loginSvc.logoff();
        //    return undefined;
        //});

        hub = $.connection.takin;
        hub.client.update = function (res) {
            if (isBackTesting)
                newBacktestingData = JSON.parse(res)
            else
                newPortfolioData = JSON.parse(res);

            if (progressDialogVisible)
            {
                closeCurrentDialog();
                progressDialogVisible = false;
            }

            $timeout(function () {
                beep();
                $scope.isInProgress = false;
                if (isBackTesting)
                    creatBacktestingEnded();
                else
                    creatPortfoliEnded();

            }, 200);
        }

        hub.client.disconnected = function (res) {
            reconnectToHub();
        }

        hub.client.sessionEnded = function (data) {
            $scope.isInProgress = false;
            $scope.isDataReady = false;
            if ($location.$$path != '/') {
                utilitiesSvc.showOKMessage('message', 'Your session has expired, please relogin', 'OK');
            }
            sessionStorage.removeItem("user");
            sessionStorage.removeItem('userDetails');
            //Smooch.destroy();
            $location.path('/');
        }
        reconnectToHub();

    }

    var creatPortfoliEnded = function ()
    {
        if (newPortfolioData.Messages.length == 0)
        {
            //sessionStorage.setItem('newPortfolioData', angular.toJson(newPortfolioData));
            $scope.saveNewPortfolio(newPortfolioData);
            $scope.isDataReady = true;

            utilitiesSvc.showOKMessage('message', 'Your optimized portfolio is ready.', 'View Portfolio')
                .then(function (btn)
                {
                    $scope.showNewPortfolio();
                });
        }
        else {
            utilitiesSvc.showOKMessage('message', newPortfolioData.Messages[0].Text);
            $scope.isDataReady = false;
        }
    }

    var creatBacktestingEnded = function () {
        if (newBacktestingData.Messages.length == 0) {

            $scope.saveNewBacktesting(newBacktestingData);
            $scope.isDataReady = true;

            utilitiesSvc.showOKMessage('message', 'Your backtesting portfolio is ready.', 'View Portfolio')
                .then(function (btn)
                {
                    $scope.showNewPortfolio();
                });
        }
        else {
            utilitiesSvc.showOKMessage('message', newBacktestingData.Messages[0].Text);
            $scope.isDataReady = false;
        }
    }

    function closeCurrentDialog()
    {
        var closeBtn = $document.find('.modal-dialog').find('.close');
        closeBtn.trigger('click');
    }

    $scope.showNewPortfolio = function () {

        $scope.isDataReady = false;
        $scope.isInProgress = false;

        if (!isBackTesting) {
            if (portfolio)
                $location.path('optimizePortfolios/' + portfolio.PortDetails.Name);
        }
        else
            $location.path('backtesting/' + newBacktestingParams.Name);

        isBackTesting = false;

    }

    initialize();
}]);
app.controller("portfolioCtrl", ['$scope', '$rootScope', "$location", "$window", "ngTableParams", "portfolioSvc", "utilitiesSvc", "$filter", "lookupSvc", "backtestingSvc", "$timeout", function ($scope, $rootScope, $location, $window, ngTableParams, portfolioSvc, utilitiesSvc, $filter, lookupSvc, backtestingSvc, $timeout) {

    var currentPage;

    $scope.submit = false;

    $scope.data = [];

    $scope.dataBacktesing = [];

    $scope.newPort = { Exchanges: [] };

    $scope.exchanges = [];

    $scope.customType = "";
    $scope.searchParams = {};
    var isreversePort = true;

    var isreverseBack = true;

    $scope.getPortfolios = function () {

       
        $scope.tableParams.reload();

       
        $scope.tableParams1.reload();
    };

    $scope.refreshPortfolios = function (port) {
        $scope.data.splice($scope.data.indexOf(port), 1);
        $scope.tableParams.reload();
        savePortfolioList();
    }

    $scope.refreshBacktesting = function (backtest) {
        $scope.dataBacktesing.splice($scope.dataBacktesing.indexOf(backtest), 1);
        $scope.tableParams1.reload();
        //savePortfolioList();
    }

    $scope.sortPortfolio = function () {
        isreversePort = !isreversePort;
        $scope.data = $filter('orderBy')($scope.data, $scope.orderByPortField, isreversePort);
        $scope.tableParams.reload();
    }

    $scope.sortBacktesting = function () {
        isreverseBack = !isreverseBack;
        $scope.dataBacktesing = $filter('orderBy')($scope.dataBacktesing, $scope.orderByBackField, isreverseBack);
        $scope.tableParams1.reload();
    }

    var savePortfolioList = function () {
        portList = [];
        for (var i = 0; i < $scope.data.length; i++) {
            portList.push({ 'ID': $scope.data[i].ID, 'Name': $scope.data[i].Name });
        }
        $scope.$parent.setPortfolioList(portList);
    }

    var initGridParams = function () {

        $scope.tableParams = new ngTableParams({
            page: 1,            // show first page
            count: 5         // count per page

        }, {
                total: $scope.data.length, // length of data
                counts: [],
                getData: function ($defer, params) {

                    $scope.searchParams.pageNumber = $scope.tableParams.page();
                    $scope.searchParams.pageSize = $scope.tableParams.count();
                    $scope.searchParams.sortField = $scope.orderByPortField;
                    $scope.noPortfolios = $scope.data.length === 0;
                    portfolioSvc.getPortfolios($scope.searchParams).$promise.then(function (data) {
                        $timeout(function () {
                        
                            $scope.data = data.Portfolios;
                            params.total(data.NumOfRecords);
                            savePortfolioList();
                            currentPage = params.page() * params.count() < data.NumOfRecords ? params.count() : data.NumOfRecords - ((params.page() - 1) * params.count());
                            $scope.noPortfolios = $scope.data.length === 0;
                            if ($scope.data)
                                $defer.resolve($scope.data);
                        }, 500);
                    });
                }
            });

        $scope.tableParams1 = new ngTableParams({
            page: 1,            // show first page
            count: 5         // count per page

        }, {
                total: $scope.dataBacktesing.length, // length of data
                counts: [],
                getData: function ($defer, params) {


                    var sParams = {};
                    sParams.pageNumber = $scope.tableParams1.page();
                    sParams.pageSize = $scope.tableParams1.count();
                    sParams.sortField = $scope.orderByBackField;
                    $scope.noBacktesting = $scope.dataBacktesing.length === 0;
                    backtestingSvc.getbacktesingPortfolies(sParams).$promise.then(function (data) {

                        $timeout(function () {
                           
                            $scope.dataBacktesing = data.Ports;
                            params.total(data.NumOfRecords);
                            currentPage = params.page() * params.count() < data.NumOfRecords ? params.count() : data.NumOfRecords - ((params.page() - 1) * params.count());
                            $scope.noBacktesting = $scope.dataBacktesing.length === 0;
                            if ($scope.dataBacktesing)
                                $defer.resolve($scope.dataBacktesing);
                        }, 500);
                    });
                }
            });
    }
    var getStocks = function () {
        $scope.$parent.getUserStocks();
    }

    var failedGetLookup = function (error) { }

    var initialize = function () {

        $scope.$parent.selectMenu = 1;

        initGridParams();

        getStocks();

        $scope.getPortfolios();

        user = $scope.$parent.getUser();

        $scope.currency = user.Currency ? user.Currency.CurrencySign : '';

        var today = new Date();

        $scope.currentDate = today;
    };

    initialize();

}]);
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
app.controller("customPortfolioCtrl", ['$scope', "$location", "$window", "$document", "ngTableParams", "securitiesSvc", '$routeParams',
    'utilitiesSvc', '$filter', "lookupSvc", "$timeout", "isMobile", 'VisibilitySpy', 'visibilityHelper',
    function ($scope, $location, $window, $document, ngTableParams, securitiesSvc, $routeParams,
        utilitiesSvc, $filter, lookupSvc, $timeout, isMobile, VisibilitySpy, visibilityHelper)
    {
        var currentPage;
        var user;
        var timeoutId = 0;
        var minInvestmentAmount = 0;
        var lastKnownExchanges = [];
        var lastKnownSectors = [];
        var selectedExchanges = [];
        var selectedSectors = [];

        $scope.searchParams = {
            exchangesPackagees: [],
            sectors: [],
            maxRiskLevel: 0,
            pageNumber: 1,
            pageSize: isMobile.phone ? 20 : 50,
            searchText: "",
            field: "strName",
            direction: "asc",
            hideDisqualified: true
        };

        $scope.showDisqualified = false;

        $scope.riskFilter;

        $scope.data = [];

        $scope.selectedSecurities = [];

        $scope.selctesItemsNum = null;

        $scope.searchText;

        $scope.selectSecurityNum = 0;

        $scope.isRiskFilter = null;

        $scope.dataLength;

        $scope.exchanges = [];
        $scope.customExchanges = [];
        $scope.sectorsOptions = [];
        $scope.sectors = [];
        $scope.newPort = { Exchanges: [] };
        $scope.filterDataSource = {
            exchanges: [],
            sectors: []
        };

        $scope.selectOrUnSelectAll = function () {

            if ($scope.customType == 1 || !$scope.customType)
                return;

            var maxSecs = $scope.stocksCount < $scope.tableParams.total() ? $scope.stocksCount : $scope.tableParams.total();

            if ($scope.selectedSecurities.length == 0)
            {
                var page = $scope.searchParams.pageNumber;
                var size = $scope.searchParams.pageSize;
                $scope.searchParams.pageNumber = 1;
                $scope.searchParams.pageSize = $scope.stocksCount;
                var hide = $scope.searchParams.hideDisqualified;
                $scope.searchParams.hideDisqualified = true;
                $scope.selectedSecurities = [];
                $scope.selectSecurityNum = maxSecs;
                securitiesSvc.getAllSecurities($scope.searchParams).$promise.then(function (data)
                {
                    $scope.searchParams.pageNumber = page;
                    $scope.searchParams.pageSize = size;
                    for (var i = 0; i < data.Securities.length; i++)
                    {
                        $scope.selectedSecurities.push({ select: true, idSecurity: data.Securities[i].idSecurity });
                    }
                    for (var i = 0; i < $scope.data.length; i++)
                    {
                        var items = $scope.selectedSecurities.filter(function (x) { return x.idSecurity == $scope.data[i].idSecurity });
                        if (items.length > 0)
                            $scope.data[i].select = true;
                    }
                });
                $scope.searchParams.hideDisqualified = hide;
            }
            else
            {
                $scope.selectSecurityNum = 0;
                for (var i = 0; i < $scope.data.length; i++) {
                    $scope.data[i].select = false;
                }
            }
                
            $scope.selectedSecurities = [];
        }

        $scope.createPortfolio = function () {

            $scope.submit = true;

            $scope.newPort.Exchanges = $scope.exchanges.map(function (exchange)
            {
                return exchange.id;
            });

            if ($scope.$parent.isInProgress) {
                utilitiesSvc.showOKMessage('message', 'The system is already building a portfolio, please wait', 'OK');
                return;
            }
            if ($scope.newPort.Name) {
                if (!$scope.isValidPortfolioName()) return;
            }

            if ($scope.portForm.$invalid) {
                utilitiesSvc.showOKMessage('message', "Please fill in all required fields.", 'OK'); return;
            }

            if (!$scope.customType || $scope.customType == '') {
                utilitiesSvc.showOKMessage('message', 'Please select a Building Process.', 'OK');
                return;
            }

            if (new Date(user.Licence.ExpiryDate) < new Date()) {
                utilitiesSvc.showOKMessage('message', 'Your license has expired. You can no longer build new portfolios. Please contact us at info@gocherries.com to renew your license.', 'OK');
                return;
            }
            if ($scope.$parent.getPortfolioList() != null  && user.Licence.Service.Iportfolios <= $scope.$parent.getPortfolioList().length) {
                utilitiesSvc.showOKMessage('message', "You've exceeded the number of portfolios you can create. Please delete any unwanted portfolios", 'OK');
                return;
            }
            if ($scope.customType == 1)
                $scope.createAutoPortfolio();
            else {
                $scope.createCustomPortfolio();

            }

        }

        $scope.createCustomPortfolio = function () {

            if (!$scope.newPort.Name || !$scope.newPort.Equity) {
                utilitiesSvc.showOKMessage('error', 'Fill all portfolio details', 'OK');
                return;
            }

            $scope.createNewPortfolio = true;

            if ($scope.selectedSecurities.length == 0) {
                utilitiesSvc.showOKMessage('message', 'Please select at least 1 security from the list below.', 'OK');
                return;
            }
            $scope.newPort.Securities = [];
            for (var i = 0; i < $scope.selectedSecurities.length; i++) {
                $scope.newPort.Securities.push($scope.selectedSecurities[i].idSecurity);
            }

            $scope.newPort.Exchanges = [];
            for (var i = 0; i < $scope.customExchanges.length; i++) {
                $scope.newPort.Exchanges.push($scope.customExchanges[i].id);
            }

            $scope.$parent.saveNewPortfolio($scope.newPort);

            var parameters = {
                Name: $scope.newPort.Name,
                Securities: $scope.newPort.Securities,
                Equity: $scope.newPort.Equity,
                Risk: 0,
                CalcType: 4,
                Exchanges: $scope.newPort.Exchanges
            };

            $scope.$parent.createPortfolio(parameters);
        };

        $scope.createAutoPortfolio = function () {

            if (!$scope.newPort.Name || !$scope.newPort.Equity || !$scope.newPort.PreferedRisk || $scope.exchanges.length == 0) {
                utilitiesSvc.showOKMessage('error', 'Fill all portfolio details', 'OK');
                return;
            }

            else if ($scope.newPort.Equity < minInvestmentAmount) {
                utilitiesSvc.showOKMessage('error', 'Investment amount must be ' + $filter('number')(minInvestmentAmount, 0) + $scope.currency + ' or higher', 'OK');
                return;
            }
            $scope.newPort.PreferedRisk.Name = getRiskByRiskType($scope.newPort.PreferedRisk.RiskType);

            $scope.$parent.saveNewPortfolio($scope.newPort);


            var parameters = {
                Name: $scope.newPort.Name,
                Securities: [],
                Equity: $scope.newPort.Equity,
                Risk: $scope.newPort.PreferedRisk.RiskType,
                Exchanges: $scope.newPort.Exchanges,
                CalcType: $scope.newPort.PreferedRisk.RiskType > 0.03 ? enumEfCalculationType.BestRisk : enumEfCalculationType.BestTP
            };

            $scope.createNewPortfolio = true;

            $scope.$parent.createPortfolio(parameters);

        };

        $scope.isValidPortfolioName = function () {
            if ($scope.newPort && $scope.newPort.Name && $scope.newPort.Name.length > 15) {
                utilitiesSvc.showOKMessage('error', 'Invalid portfolio name – Please enter a name up to 15 characters', 'OK');
                return false;
            }
            else if ($scope.newPort.Name.indexOf('/') > -1) {
                utilitiesSvc.showOKMessage('error', 'Invalid portfolio name – "/" is not allowed in portfolio name', 'OK');
                return false;
            }
            else {
                return true;
            }
        }

        $scope.search = function () {
            clearTimeout(timeoutId);
            timeoutId = setTimeout(function () {
                $scope.tableParams.page(1);
                $scope.tableParams.reload();
            }, 500);
        };

        $scope.addOrRemoveToSectorList = function (model) {
            if (!model)
                return;

            if (model.select) {
                if ($scope.selectSecurityNum == $scope.stocksCount) {
                    utilitiesSvc.showOKMessage('message', 'You have reach the max securities you can select', 'OK');
                    model.select = false;
                    return;
                }
                $scope.selectSecurityNum = $scope.selectSecurityNum + 1;
                $scope.selectedSecurities.push({ select: true, idSecurity: model.idSecurity });

            }
            else {
                $scope.selectSecurityNum = $scope.selectSecurityNum - 1;
                for (var i = 0; i < $scope.selectedSecurities.length; i++) {
                    if ($scope.selectedSecurities[i].idSecurity == model.idSecurity) {
                        $scope.selectedSecurities.splice(i, 1);
                        break;
                    }
                }
            }
        }

        $scope.filterRiskChanged = function ()
        {
            $scope.searchParams.maxRiskLevel = $scope.filterRiskLevel;
            updatePopupFilter();
            filterChanged();
        }

        function filterChanged()
        {
            if ($scope.customType != 2)
            {
                return;
            }

            selectedSectors = $scope.sectors.map(function (selectedSector)
            {
                return selectedSector.id;
            });
            selectedExchanges = $scope.customExchanges.map(function (selectedExch)
            {
                return selectedExch.id;
            });
            $scope.tableParams.page(1);
            $scope.tableParams.reload();
        }

        $scope.exchangesClosed = function ()
        {
            updatePopupFilter();
            var currentExchanges = $scope.customExchanges.slice();
            if (!arrayEquals(lastKnownExchanges, currentExchanges, 'id'))
            {
                filterChanged();
            }
            lastKnownExchanges = currentExchanges;
        }

        $scope.sectorsClosed = function ()
        {
            updatePopupFilter();
            var currentSectors = $scope.sectors.slice();
            if (!arrayEquals(lastKnownSectors, currentSectors, 'id'))
            {
                filterChanged();
            }
            lastKnownSectors = currentSectors;
        }

        $scope.applyFilter = function (filterData)
        {
            $scope.sectors = filterData.sectors.slice();
            $scope.customExchanges = filterData.exchanges.slice();
            $scope.filterRiskLevel = filterData.riskLevel;
            $scope.searchParams.maxRiskLevel = filterData.riskLevel;
            filterChanged();
        }

        $scope.filterChanged = filterChanged;

        $scope.setNewPortRiskType = function (type) {
            if (!$scope.newPort.PreferedRisk) { $scope.newPort.PreferedRisk = {}; }

            if ($scope.customType != 2) {
                $scope.RiskType = type;
                $scope.newPort.PreferedRisk.RiskType = getRiskValue(type);
            }
            else {
                $scope.RiskType = null;
                $scope.newPort.PreferedRisk.RiskType = null;
            }
        }

        $scope.$watch('customType', function (newValue, oldValue) {
            if (newValue == oldValue) return;
            if (newValue == "2") getSecurities(1);
        }, true);

        $scope.$on('$destroy', function () {
            for (var i in $scope) {
                if (i.indexOf('$') == 1)
                    $scope[i] = null;
            }
        })

        function arrayEquals(array_1, array_2, fieldName)
        {
            if (fieldName)
            {
                array_1.sort(function (x, y)
                {
                    return x[fieldName] - y[fieldName];
                });
                array_2.sort(function (x, y)
                {
                    return x[fieldName] - y[fieldName];
                });
            }
            else
            {
                array_1.sort();
                array_2.sort();
            }

            return angular.equals(array_1, array_2);
        }

        var getRiskValue = function (value) {

            switch (value) {
                case 0:
                    return 0;

                case 1:
                    return 9 / 100;

                case 2:
                    return 14 / 100

                case 3:
                    return 25 / 100;

                case 4:
                    return 40 / 100;

                case 5:
                    return 1;


            }
        }

        var getRiskByRiskType = function (type) {

            switch (type) {
                case 0: return "TP";

                case 1: return 'Solid';

                case 2: return "Low";

                case 3: return 'Moderate';

                case 4: return "High";

                case 5: return 'Very High';

            }
        }

        var isPrecentValueInRange = function (model) {

            var value = user.Currency.CurrencyId != '9001' ? model.avgYield : model.avgYieldNIS;
            value = $filter('number')(value * 100, 2);
            return value * 1 >= $scope.returnValueFilter * 1;
        }

        var getSecurities = function (page) {
            page = page == null ? 1 : page;
            $scope.tableParams = new ngTableParams({
                page: page,            // show first page
                count: $scope.searchParams.pageSize         // count per page

            }, {
                    total: $scope.data.length, // length of data
                    counts: [],
                    getData: function ($defer, params) {
                        $scope.searchParams.pageNumber = $scope.tableParams.page();
                        $scope.searchParams.pageSize = $scope.tableParams.count();
                        $scope.searchParams.exchangesPackagees = getSelectedExchanges();
                        $scope.searchParams.sectors = getSelectedSectors();
                        $scope.searchParams.hideDisqualified = !$scope.showDisqualified;
        
                        securitiesSvc.getAllSecurities($scope.searchParams).$promise.then(function (data) {
                            $timeout(function () {
                                if (!data.Securities || data.Securities.length == 0) {
                                    utilitiesSvc.showOKMessage('error', 'There are no securities available', 'OK');
                                    return
                                }
                                //utilitiesSvc.showOKMessage('message', 'Please choose Exchanges/Stocks/Sector for showing securties', 'OK');
                                $scope.data = data.Securities;
                                if ($scope.selectedSecurities.length > 0) {
                                    for (var i = 0; i < $scope.data.length; i++) {
                                        var items = $scope.selectedSecurities.filter(function (x) { return x.idSecurity == $scope.data[i].idSecurity });
                                        if (items.length > 0)
                                            $scope.data[i].select = true;
                                    }
                                }
                                params.total(data.NumOfRecords);
                                $scope.currenPageCount = params.page() * params.count() < data.NumOfRecords ? params.count() : data.NumOfRecords - ((params.page() - 1) * params.count());
                                $defer.resolve($scope.data);
                            }, 500);
                        }, function (error) {

                            //utilitiesSvc.showOKMessage('error', 'Failed to get securities', 'OK');

                        });
                    }
                });
        };

        var getLookupValues = function () {

            var data = $scope.$parent.getLookupValues();

            if (data != null) {
                $scope.sectorsOptions = data.Categories.Sector;
            }
            for (var i = 0; i < $scope.sectorsOptions.length; i++) {
                $scope.sectorsOptions[i].label = $scope.sectorsOptions[i].strValue;
            }
            for (var i = 0; i < $scope.stockMarket.length; i++) {
                $scope.stockMarket[i].label = $scope.stockMarket[i].HebName;
            }
        };

        var init = function () {

            $scope.$parent.selectMenu = 2;

            $scope.newPortfolio = $scope.$parent.getNewPortfolio($routeParams.name);

            user = $scope.$parent.getUser();

            $scope.stocksCount = user.Licence.Stocks.length * 250;

            //var stdYield = user.Currency.CurrencyId != '9001' ? 'StdYield' : 'StdYieldNIS';
            $scope.avgYield = 'AvgYield';
            $scope.stockMarketSettings = {
                showCheckAll: true,
                showUncheckAll: true,
                idProp: 'id',
                sellectAllDefault: false,
                buttonDefaultText: 'select',
                buttonClasses: 'btn btn-default drop-btn',
                scrollable: true,
                scrollableHeight: 190,

            };
            $scope.sectorsSettings = {
                showCheckAll: true,
                showUncheckAll: true,
                idProp: 'iIndex',
                sellectAllDefault: false,
                buttonDefaultText: 'select',
                buttonClasses: 'btn btn-default drop-btn',
                scrollable: true,
                scrollableHeight: 190,

            };
            $scope.columns = [
                { name: 'strName', colwidth: 3, paddingLeft: "3px" },
                { name: 'strSymbol', colwidth: 2, paddingLeft: "10px" },
                { name: 'marketName', colwidth: 1, paddingLeft: "14px" },
                { name: 'sectorName', colwidth: 2, paddingLeft: "16px" },
                { name: $scope.avgYield, isPercentage: true, colwidth: 2, paddingLeft: "22px" },
                { name: 'rank', moreInfo: true, isNumber: true, colwidth: 2, paddingLeft: "22px" },
            ];

            $scope.mobileColumns = {
                title: {
                    primary: 'strName',
                    secondary: 'strSymbol'
                },
                properties: [
                    { name: 'sectorName', title: "Sector" },
                    { name: 'marketName', title: "Exchange" },
                    { name: 'rank', title: "Rank", isNumber: true }
                ]
            };

            $scope.stockMarket = user.Licence.Stocks;

            getLookupValues();

            $scope.filterDataSource.exchanges = $scope.stockMarket.map(function (stock)
            {
                return {
                    id: stock.id,
                    label: stock.HebName
                };
            });

            $scope.filterDataSource.sectors = $scope.sectorsOptions.map(function (sector)
            {
                return {
                    id: sector.iIndex,
                    label: sector.strValue
                }
            });

            updatePopupFilter();

            if ($scope.customType == "2")
                getSecurities(1);

            var goBtn = $document.find('.portfolio-custom__go-btn');

            var btnSpy = new VisibilitySpy(goBtn, btnVisibilityChange);
            visibilityHelper.register(btnSpy);
            function btnVisibilityChange(isVisible)
            {
                $scope.$apply(function ()
                {
                    $scope.bottomBtnVisible = !isVisible;
                });
            }
        }

        function getSelectedExchanges()
        {
            if (selectedExchanges.length > 0)
            {
                return selectedExchanges.slice();
            }
            else
            {
                return $scope.stockMarket.map(function (exch)
                {
                    return exch.id;
                });
            }
        }

        function getSelectedSectors()
        {
            if (selectedSectors.length > 0)
            {
                return selectedSectors.slice();
            }
            else
            {
                return $scope.sectorsOptions.map(function (sect)
                {
                    return sect.iIndex;
                });
            }
        }

        function updatePopupFilter()
        {
            var selectedExchanges = ($scope.customExchanges || []).map(function (exch)
            {
                return exch.id;
            });
            angular.forEach($scope.filterDataSource.exchanges, function (exch)
            {
                exch.selected = selectedExchanges.indexOf(exch.id) >= 0;
            });

            var selectedSectors = ($scope.sectors || []).map(function (sector)
            {
                return sector.id;
            });
            angular.forEach($scope.filterDataSource.sectors, function (sector)
            {
                sector.selected = selectedSectors.indexOf(sector.id) >= 0;
            });
            $scope.filterDataSource.riskLevel = $scope.searchParams.maxRiskLevel;
        }

        init();
    }]);
app.controller("optimizedPortfolioCtrl", ['$scope', '$rootScope', "$location", "$window", "ngTableParams", "securitiesSvc", '$routeParams', '$filter', 'portfolioSvc', 'utilitiesSvc', 'exportDataSvc', 'isMobile',
    function ($scope, $rootScope, $location, $window, ngTableParams, securitiesSvc, $routeParams, $filter, portfolioSvc, utilitiesSvc, exportDataSvc, isMobile) {

        var allData;
        var portRisks;
        var alllineData = [];
        var portfoliosTemp;
        var currentPage;
        var itemsInPage = 12;
        var isMsgOpen = false;

        $scope.solidData = [];
        $scope.lowData = [];
        $scope.moderateData = [];
        $scope.heighData = [];
        $scope.veryHighData = [];
        $scope.data = [];
        $scope.cashData = {};
        $scope.selectPortfolio;
        $scope.selectProfessional = 'yourPort';
        $scope.chartOrderByField = 'percentage';
        $scope.isProfessionalView = true;
        $scope.searchText = '';
        $scope.weightChart = 'sector';

        $scope.search = function () {

            if ($scope.searchText == "") {
                $scope.data = $scope.selectPortfolio.Securities;
            }
            else {
                $scope.data = $filter('filter')($scope.selectPortfolio.Securities, filterTest, true);
            }
            $scope.data = $scope.data.filter(function (security)
            {
                return security.Quantity >= 1;
            });
            //if ($scope.data[0].Name == 'Cash') {
            //  var cash = $scope.data.splice(0, 1);
            // $scope.cashData = cash[0];
            // }

            $scope.tableParams.page(1);
            $scope.tableParams.reload();
        };

        $scope.setSelectPortfolio = function (index) {
            if (index >= $scope.portfolios.length) index = $scope.portfolios.length - 1;
            $scope.selectPortfolio = $scope.portfolios[index];
            //if ($scope.selectPortfolio.Securities.filter(function (x) { return x.Name == 'Cash'; }).length == 0) {
            //    $scope.selectPortfolio.Securities.push({ Name: 'Cash', 'Symbol': '', 'marketName': '', 'sectorName': '', 'Quantity': '', 'Value': $scope.portfolios[index].Cash });
            //}
            //else if ($scope.selectPortfolio.Securities[0].Name == 'Cash') {
            //    var cash = $scope.selectPortfolio.Securities.splice(0, 1);
            //    $scope.selectPortfolio.Securities.push(cash[0]);
            //}
            allData.PortNumA = index;

            if (($scope.selectPortfolio.Securities.length - 1 < 4 && !isMsgOpen) && !isMobile.phone) {
                isMsgOpen = true;
                utilitiesSvc.showOKMessageWithIndication('Warning', 'Your portfolio is not well diversified. For a more diversified portfolio, please select any of the portfolios to the left of yours along the Efficient Frontier.', 'OK').then(function (result) {
                    isMsgOpen = false;
                });
            }
            allData.PortNumA = index;
            setRiskPortfolio();
            reloadData();
        }

        $scope.setSelectProfessional = function (selectName) {

            $scope.selectProfessional = selectName;

            switch (selectName) {
                case 'MVP':
                    $scope.selectPortfolio = $scope.mvpPort;
                    allData.PortNumA = 0;
                    break;
                case 'yourPort':
                    $scope.selectPortfolio = $scope.yourPort;
                    allData.PortNumA = allData.OrigPortPos;
                    break;
                case 'TP':
                    $scope.selectPortfolio = $scope.tpPort;
                    allData.PortNumA = allData.TangencyPos;
                    break;
            }
            setRiskPortfolio();
            reloadData(true);
        }
        $scope.updatePortdolio = function () {

            if (allData.PortNumA != allData.OrigPortPos) {
                var params = {
                    PortID: allData.PortID,
                    Securities: $scope.selectPortfolio.Securities.filter(function (x) {
                        return x.Name != 'Cash';
                    }),
                    Equity: $scope.newPort.PortDetails.Equity,
                    Risk: $scope.selectPortfolio.Risk,
                    CalcType: 1

                }
                portfolioSvc.updatePortfolio(params).$promise.then(function (data) {

                    utilitiesSvc.showOKMessage('message', 'Portfolio saved successfully.', 'OK');

                    allData.OrigPortPos = allData.PortNumA;
                    $rootScope.$broadcast('updateScetcherTagsAndRedPoint');

                    initChartLine();
                    reloadData();

                }, function (error) {
                    utilitiesSvc.showOKMessage('error', 'failed save data', 'OK');
                });
            }
            else {
                utilitiesSvc.showOKMessage('message', 'Portfolio saved successfully.', 'OK');
            }
        }

        $scope.export = function () {

            var params = {
                portID: allData.PortID,
                OptimalPort: $scope.selectPortfolio
            }

            portfolioSvc.exportOptimize(params).$promise.then(function (data) {
                exportDataSvc.download(data.Data, "", data.Name)
            });
        }

        $scope.changeOrderByBarChart = function () {
            initBarChartSettings();
        }

        var saveNewPort = function () {

            allData = $scope.$parent.getNewPortfolioObj();

            //allData = getData();

            $scope.portfolios = allData.Portfolios;
            $scope.newPort.PortID = allData.PortID;
            $scope.portDetails = allData.PortDetails;
            $scope.changeFromYesterday = $scope.portDetails.CurrEquity - $scope.portDetails.Equity + $scope.portDetails.Profit;
            $scope.equityCalc = $scope.portDetails.Equity + $scope.portDetails.Profit;

            if ($scope.portfolios == null)
                return;
            $scope.selectPortfolio = $scope.portfolios[allData.PortNumA];

            setRiskPortfolio();

            //set basic objects
            $scope.mvpPort = $scope.portfolios[0];
            $scope.yourPort = $scope.portfolios[allData.OrigPortPos];
            $scope.tpPort = $scope.portfolios[allData.TangencyPos];

            initChartLine();
            reloadData();

        }

        var setRiskPortfolio = function () {

            if ($scope.selectPortfolio) {
                $scope.riskColorName = getRiskName($scope.selectPortfolio.Risk);
                $scope.riskColor = $scope.riskColors[$scope.riskColorName];
            }
        }

        var getRiskName = function (val) {

            var value = $filter('number')(val * 100, 2) * 1;
            switch (true) {
                case (value > 0 && value <= 9):
                    return 'Solid'
                    break;
                case (value > 9 && value <= 14):
                    return 'Low';
                    break;
                case (value > 14 && value <= 25):
                    return 'Moderate';
                    break;
                case (value > 25 && value <= 40):
                    return 'High';
                    break;
                case (value > 40 && value <= 100):
                    return 'Very High';
                    break;
            }
            return 'none';

        }

        var filterTest = function (value, index, array) {
            if (!value)
                return false;

            return value['Name'] && value['Name'].toLowerCase().indexOf($scope.searchText.toLowerCase()) != -1;
        }

        var reloadData = function () {
            $scope.data = $scope.selectPortfolio.Securities.slice();
            $scope.data = $scope.data.filter(function (security)
            {
                return security.Quantity >= 1;
            });
            var cashData = { Name: 'Cash', 'Symbol': '', 'marketName': '', 'sectorName': '', 'Quantity': '', 'DisplayValue': $scope.selectPortfolio.Cash };
            $scope.data.push(cashData);

            initBarChartSettings();
            //initChartLine();
            initChartPie();
            $scope.tableParams.page(1);
            $scope.tableParams.reload();
        }

        var initTableParams = function () {

            $scope.tableParams = new ngTableParams({
                page: 1,            // show first page
                count: itemsInPage         // count per page

            }, {
                    total: $scope.data.length, // length of data
                    counts: [],
                    getData: function ($defer, params) {
                        currentPage = params.page();
                        params.total($scope.data.length);
                        if ($scope.data && $scope.data.length > 0)
                            $defer.resolve($scope.data.slice((params.page() - 1) * params.count(), params.page() * params.count()));
                        else {
                            $defer.resolve($scope.data);
                        }

                    }
                });
        }

        var getLineLabel = function (val, nextval, index, last) {

            if (index == 0 || index == last) {
                return val + "%";
            }

            if ((val > 0 && val <= 9 && nextval > 9 && nextval <= 23) ||
                (val > 9 && val <= 23 && nextval > 23 && nextval <= 40) ||
                (val > 23 && val <= 40 && nextval > 40 && nextval <= 70) ||
                (val > 40 && val <= 70 && nextval > 70 && nextval <= 100)
            ) {
                return val + "%";

            }
            return '';

        }

        var populateRiskDataChart = function (minVal, maxVal) {

            var xDataArrays = ['solidData', 'lowData', 'moderateData', 'heighData', 'veryHighData'];

            for (var i = 0; i < 5; i++) {

                $scope[xDataArrays[i]].push({ x: $filter('number')(portRisks[i + 1].LowerBound * 100, 2), y: 0 });

                if (maxVal >= portRisks[i + 1].UpperBound) {
                    $scope[xDataArrays[i]].push({ x: $filter('number')(portRisks[i + 1].UpperBound * 100, 2), y: 0 });
                }
                else if (maxVal < portRisks[i + 1].UpperBound) {
                    $scope[xDataArrays[i]].push({ x: $filter('number')(maxVal * 100, 2), y: 0 });
                    return;
                }
            }
        }

        String.format = function (str) {
            var args = arguments;
            return str.replace(/{[0-9]}/g, function (matched) { args[parseInt(matched.replace(/[{}]/g, '')) + 1] });
        };

        var initChartLine = function (refreshAfterReload) {

            $scope.lineLabels = [];
            $scope.lineData = [];


            $scope.portfolios = $filter('orderBy')($scope.portfolios, 'Risk', false);
            portfoliosTemp = $scope.portfolios;
            $scope.solidData = [];
            $scope.lowData = [];
            $scope.moderateData = [];
            $scope.heighData = [];
            $scope.veryHighData = [];
            alllineData = [];

            populateRiskDataChart($filter('number')($scope.portfolios[0].Risk, 2), $filter('number')($scope.portfolios[$scope.portfolios.length - 1].Risk, 5));
            var index = 0;
            var itemToRemoveIndex = [];
            for (var i = 0; i < $scope.portfolios.length; i++) {

                var xVal = $filter('number')($scope.portfolios[i].Risk * 100, 1);
                var temp = $filter('filter')(alllineData, { x: xVal }, true);
                if (!temp || temp.length == 0) {

                    alllineData.push({ x: xVal, y: $filter('number')($scope.portfolios[i].Return * 100, 2) });
                    if (i == 0) alllineData[index].tagLabel = "MVP";
                    if (i == allData.OrigPortPos) {

                        if (i == 0) {
                            alllineData[index].tagLabel += " and Opt";
                        }
                        else {
                            alllineData[index].tagLabel = "Opt";
                        }
                    }

                    if (i == allData.TangencyPos) {
                        if (alllineData[index].tagLabel != undefined)
                            alllineData[index].tagLabel += " and TP";
                        else
                            alllineData[index].tagLabel = "TP";
                    }


                    index = index + 1;
                }
                else {
                    itemToRemoveIndex.push(i)

                }
            }
            for (var i = 0; i < itemToRemoveIndex.length; i++) {

                $scope.portfolios.splice(itemToRemoveIndex[i], 1);
            }

            $scope.lineData = [
                {
                    pointDot: true,
                    strokeColor: 'rgb(218,218,218)',
                    pointColor: 'rgb(45,62,80)',
                    scaleSteps: 10,
                    scaleStepWidth: 10,
                    scaleLineWidth: 5,
                    xScaleOverride: true,
                    scaleShowLabels: true,
                    data: alllineData
                }];
            if ($scope.solidData.length > 0) {
                $scope.lineData.push({
                    pointDot: false,
                    strokeColor: 'green',
                    data: $scope.solidData,
                });
            }
            if ($scope.lowData.length > 0 || $scope.solidData.length > 0) {
                $scope.lineData.push({
                    pointDot: false,
                    strokeColor: 'yellow',
                    data: $scope.lowData,
                });
            }
            if ($scope.moderateData.length > 0 || $scope.lowData.length > 0) {
                $scope.lineData.push({
                    pointDot: false,
                    strokeColor: 'rgb(240, 187, 2)',
                    data: $scope.moderateData,
                });
            }
            if ($scope.heighData.length > 0 || $scope.moderateData.length > 0) {
                $scope.lineData.push({
                    pointDot: false,
                    strokeColor: 'darkorange',
                    data: $scope.heighData,
                });
            }
            if ($scope.veryHighData.length > 0 || $scope.heighData.length > 0) {
                $scope.lineData.push({
                    pointDot: false,
                    strokeColor: 'red',
                    data: $scope.veryHighData,

                });
            };

            var max = $filter('number')($scope.portfolios[$scope.portfolios.length - 1].Risk * 100, 2) * 1
            var min = $filter('number')($scope.portfolios[0].Risk * 100, 2) * 1;
            var width = 1;
            var chartWidth = (max - min) * width;

            $scope.lineOptions = {
                selectPointColor: 'red',
                scaleLabel: "<%=value%>%",
                scaleArgLabel: "<%=value%>%",
                precenLabel: false,
                tagWithPath: true,
                //xScaleOverride: true,
                xScaleSteps: 20,
                selectionLine: false,
                xScaleStepWidth: 20,//(window.innerWidth - 250) / max,
                xScaleStartValue: 0,
                scaleShowLabels: true,
                scaleBeginAtZero: true,
                ySelect: $filter('number')($scope.portfolios[0].Return, 2),
                xSelect: $filter('number')($scope.portfolios[0].Risk * 100, 2),
                series: [{
                    text: "Solid: 0%-9%",
                    color: '#2fd074'
                },
                {
                    text: "Low: 9%-14%",
                    color: '#f9ec23'
                },
                {
                    text: "Moderate: 14%-25%",
                    color: '#f9b50a'
                },
                {
                    text: "High:25%-40%",
                    color: '#ff8812'
                },
                {
                    text: "Very High:40%-100%",
                    color: '#fc3d5e'
                }],
                tooltipTemplate: function (label, obj) {
                    if (!label.display) {
                        return '';
                    }
                    //display the clients and  Conversion in toolTip
                    if (label.index != null && label.index >= 0) {
                        var obj = portfoliosTemp[label.index];
                        // var s = String.format("Risk: {0}% \n Average yearly return: {1}% \n Return To Risk Ratio: {2} \n Diversification:{3}% \n ",$filter('number')(obj.Risk * 100, 2), $filter('number')(obj.Return * 100, 2), $filter('number')(obj.RateToRisk, 2), $filter('number')(obj.Diversification * 100, 1));

                        var s = "Risk: " + $filter('number')(obj.Risk * 100, 2) + '% \n Average yearly return: ' + $filter('number')(obj.Return * 100, 2) + '% \n Return To Risk Ratio: ' + $filter('number')(obj.RateToRisk, 2) + ' \n Diversification: ' + $filter('number')(obj.Diversification * 100, 1) + '% \n ';
                        return s;
                    }
                    return label.label + ': ' + label.value;
                },
                showMultiToolTip: false
            }

            var isIE = /(MSIE|Trident\/|Edge\/)/i.test($window.navigator.userAgent);
            var delay = isIE ? 6000 : 2000;

            setTimeout(function () {
                $rootScope.$broadcast('initLineScattertest', refreshAfterReload);
                $scope.setSelectPortfolio(allData.PortNumA);
            }, delay);
        }

        var initBarChartSettings = function () {

            // if (!$scope.barData) {
            $scope.barLabels = [];
            $scope.barData = [];
            $scope.barData1 = [];

            var orderByField = $scope.chartOrderByField == 'percentage' ? 'Weight' : 'Symbol';
            var orderByType = $scope.chartOrderByField == 'percentage' ? true : false;
            $scope.selectPortfolio.Securities = $filter('orderBy')($scope.selectPortfolio.Securities, orderByField, orderByType);

            var max = $scope.selectPortfolio.Securities.length < 325 ? $scope.selectPortfolio.Securities.length : 325;
            for (var i = 0; i < max; i++) {
                if ($scope.selectPortfolio.Securities[i].Symbol != '') {
                    var a = $filter('number')($scope.selectPortfolio.Securities[i].Weight * 100, 2);

                    $scope.barLabels.push($scope.selectPortfolio.Securities[i].Symbol);
                    $scope.barData.push(a * 1);
                    $scope.barData1.push({ id: a * 1 });
                }
            }
            //  }

            $scope.barOptions = {
                scaleLabel: "<%=value%>%",
                fillColor: ['#6C94BF', '#2D3E50', '#496481'],
                strokeColor: ['#6C94BF', '#2D3E50', '#496481'],
                pointColor: "#6C94BF",
                barStrokeWidth: 5,
                barValueSpacing: 1,
                barDatasetSpacing: 10,
            }

            setTimeout(function () {
                $rootScope.$broadcast('initChartBar', 'optSecurities', screen.width - 200);
            }, 1500);

        }

        var initChartPie = function () {

            var chartColors = ["#364a60", '#496481', "#7eaddf", "#2d3e58", "#5a7b9f", "#6c94bf", "#89BDF3"];
            var marketData = [];
            var sectors = []
            $scope.stockData = [];
            $scope.sectorData = [];
            var stokeSeries = [];
            var sectorSeries = [];
            var stokeIndex = 0;
            var sectorIndex = 0;
            var securities = $scope.selectPortfolio.Securities.filter(function (x) { return x.sectorName != '' });
            for (var i = 0; i < securities.length; i++) {
                var a = $filter('filter')(marketData, { id: securities[i].idMarket }, true);
                if (a.length > 0) {
                    a[0].value = a[0].value + 1;
                }
                else {
                    marketData.push({ name: securities[i].marketName, value: 1, id: securities[i].idMarket });
                }

            }

            for (var i = 0; i < marketData.length; i++) {

                if (chartColors.length < i)
                    stokeIndex = 0;
                stokeSeries.push({
                    text: marketData[i].name,
                    color: chartColors[stokeIndex]
                });
                $scope.stockData.push({
                    label: marketData[i].name,
                    value: (marketData[i].value * 100) / securities.length,
                    color: chartColors[stokeIndex],
                    labelColor: 'white',
                    labelFontSize: '16'
                });
                stokeIndex = stokeIndex + 1;
            }

            for (var i = 0; i < securities.length; i++) {
                var a = $filter('filter')(sectors, { id: securities[i].idSector }, true);
                if (a.length > 0) {
                    a[0].value = a[0].value + 1;
                }
                else {
                    sectors.push({ name: securities[i].sectorName, value: 1, id: securities[i].idSector });
                }

            }
            for (var i = 0; i < sectors.length; i++) {

                if (chartColors.length < i)
                    sectorIndex = 0;
                sectorSeries.push({
                    text: sectors[i].name,
                    color: chartColors[sectorIndex]
                });

                $scope.sectorData.push({
                    label: sectors[i].name,
                    value: (sectors[i].value * 100) / securities.length,
                    color: chartColors[i],
                    labelColor: 'white',
                    labelFontSize: '16'
                });
                sectorIndex = sectorIndex + 1;
            }

            $scope.stockOptions = {
                segmentShowStroke: false,
                showTooltips: false,
                series: stokeSeries,
                tooltipTemplate: function (label) {
                    //display the clients and  Conversion in toolTip
                    return label.label + ': ' + label.value;
                }
            };

            $scope.sectorOptions = {
                showTooltips: false,
                segmentShowStroke: false,
                series: sectorSeries,
                tooltipTemplate: function (label) {
                    //display the clients and  Conversion in toolTip
                    return label.label + ': ' + label.value;
                }
            };


            setTimeout(function () {
                $rootScope.$broadcast('initChartPie', 'stockPieChart');
                $rootScope.$broadcast('initChartPie', 'sectorPieChart');
                $rootScope.$broadcast('initChartPie', 'stockPieChartMobile');
                $rootScope.$broadcast('initChartPie', 'sectorPieChartMobile');
            }, 1000);
        }

        var getRiskLookup = function () {

            var data = $scope.$parent.getLookupValues();

            if (data != null) {
                portRisks = data.PortRisks;
            }
        }

        var init = function () {

            $scope.$parent.selectMenu = 0;

            $scope.riskColors = riskColors;

            $scope.chartName = 'lineChart';

            initTableParams();

            $scope.newPort = $scope.$parent.getNewPortfolio($routeParams.name);

            getRiskLookup();

            saveNewPort();

            $scope.columns = [
                { name: 'Name', colwidth: 3 },
                { name: 'Symbol', colwidth: 1 },
                { name: 'marketName', colwidth: 2 },
                { name: 'Weight', colwidth: 1, isPercentage: true },
                { name: 'sectorName', colwidth: 2, paddingLeft: "3px" },
                { name: 'Quantity', isNumber: true, truncate: true, colwidth: 1 },
                { name: 'DisplayValue', isNumber: true, decimalPoins: 2, colwidth: 2, moreInfo: true }
            ];

            $scope.mobileColumns = {
                title: {
                    primary: 'Name',
                    secondary: 'Symbol'
                },
                properties: [
                    { name: 'Quantity', title: 'Quantity', truncate: true },
                    { name: 'Weight', title: 'Weight', isPercentage: true },
                    { name: 'DisplayValue', title: 'Value', isNumber: true, decimalPoins: 2 }
                ]
            };

            $scope.mobileCashColumns = {
                isCash: true,
                title: {
                    primary: 'Name',
                },
                cacheField: 'DisplayValue'
            };

            user = $scope.$parent.getUser();

            $scope.currency = $scope.$parent.getLookupValues().Currencies.filter(
                function (x) { return x.CurrencyId == $scope.portDetails.CalcCurrency })[0].CurrencySign
            $scope.currencyId = $scope.$parent.getLookupValues().Currencies.filter(
                function (x) { return x.CurrencyId == $scope.portDetails.CalcCurrency })[0].CurrencyId;

            $scope.currDay = new Date();
            if (isMobile.phone) utilitiesSvc.showOKMessage('message', 'Please enable popups for portfolio export', 'OK');
        }

        init();

    }]);
app.controller("portfolioDetailsCtrl", ['$scope', '$rootScope', "$location", "$window", "ngTableParams", "securitiesSvc", '$routeParams', '$filter', 'portfolioSvc', 'utilitiesSvc', function ($scope, $rootScope, $location, $window, ngTableParams, securitiesSvc, $routeParams, $filter, portfolioSvc, utilitiesSvc) {

    $scope.portfolio;

    $scope.data = [];

    $scope.chartOrderByField = 'percentage';

    $scope.cashData = {};

    $scope.searchText = '';

    var currentPage = -1;

    var itemsInPage = 12;

    $scope.search = function () {

        if ($scope.searchText == "") {
            $scope.data = $scope.portfolio.SecurityData;
        }
        else {
            $scope.data = $filter('filter')($scope.portfolio.SecurityData, filterTest, true);
        }

        $scope.data = $scope.data.filter(function (security)
        {
            return security.flQuantity >= 1;
        });

        //if ($scope.data[0].strName == 'Cash') {
        //  var cash = $scope.data.splice(0, 1);
        //  $scope.data.push(cash[0]);
        //}
        $scope.tableParams.page(1);
        $scope.tableParams.reload();
    };

    $scope.updatechartSize = function () {
        var a = 5;
    }

    $scope.calculate = function () {

        if (!$scope.lastPeriod || !$scope.Lenchmark) {
            utilitiesSvc.showOKMessage('message', 'Please fill benchMark index and last period.', 'OK');
            return;
        }
        var x = 1;
        if ($scope.lastPeriod == 2)
            x = 6;
        if ($scope.lastPeriod == 3)
            x = 12;
        var startDate = new Date();
        startDate.setMonth(startDate.getMonth() - x);

        var params = {
            portName: $scope.portfolio.Name,
            PortID: $scope.portfolio.ID,
            Equity: $scope.portfolio.Equity,
            StartDate: startDate,
            EndDate: new Date(),
            BenchMarkID: $scope.Lenchmark,
        }

        $scope.$parent.saveBacktestingParams(params);
        $location.path('backtesting');
    };

    $scope.changePortfolio = function () {
        getPortfolio();
    }

    $scope.exportToPdf = function () {

        var dataCount = $scope.data.length < 39 ? $scope.data.length : 39;

        reloadSecTable(dataCount, 1);
        utilitiesSvc.exportToPdf('portDetails', 'portfolio ' + $scope.portfolio.Name).then(function () {
            if ($scope.data.length > 39) {
                exportPdf(dataCount, 2, $scope.data.length / 39);
            }
            else {
                reloadSecTable(12, 1);
            }
        });
    }

    var reloadSecTable = function (count, page) {

        $scope.tableParams.count(count);
        $scope.tableParams.page(page);
        $scope.tableParams.reload();

    }

    var exportPdf = function (dataCount, index, max) {

        reloadSecTable(dataCount, index);

        utilitiesSvc.exportToPdf('portSecurities', 'portfolio ' + $scope.portfolio.Name).then(function () {
            if (index < max) {
                exportPdf(dataCount, index + 1, max);
                index = index + 1;
            }
            else {
                reloadSecTable(12, 1);
            }
        });
    }

    var filterTest = function (value, index, array) {
        if (!value)
            return false;

        return (value['strName'] && value['strName'].toLowerCase().indexOf($scope.searchText.toLowerCase()) != -1);
        //|| (value['strSymbol'] && value['strSymbol'].toLowerCase().indexOf($scope.searchText.toLowerCase()) != -1)
        //|| (value['marketName'] && value['marketName'].toLowerCase().indexOf($scope.searchText.toLowerCase()) != -1)
        //|| (value['sectorName'] && value['sectorName'].toLowerCase().indexOf($scope.searchText.toLowerCase()) != -1);
    };

    var getPortfolio = function () {

        portfolioSvc.getSinglePortfolio($scope.portfolioId).$promise.then(function (data) {
            $scope.portfolio = data.Details[0];
            $scope.data = $filter('orderBy')($scope.portfolio.SecurityData, 'SecValue', true);
            $scope.data = $scope.data.filter(function (security)
            {
                return security.flQuantity >= 1;
            });
            $scope.riskName = getRiskColorName($scope.portfolio.CurrentStDev);
            $scope.riskColor = $scope.riskColors[$scope.riskName];
            $scope.changeFromYesterday = $scope.portfolio.CurrEquity - $scope.portfolio.Equity + $scope.portfolio.Profit;

            $scope.changeIcon = $scope.changeFromYesterday > 0 ? 'arrow_up' : 'arrow_down';
            $scope.cashData = { 'strName': 'Cash', 'strSymbol': '', 'marketName': '', 'sectorName': '', 'flQuantity': '', 'SecValue': $scope.portfolio.Cash };
            //$scope.data.push({ 'strName': 'Cash', 'strSymbol': '', 'marketName': '', 'sectorName': '', 'flQuantity': '', 'SecValue': $scope.portfolio.Cash });
            $scope.currency = $scope.$parent.getLookupValues().Currencies.filter(
                function (x) { return x.CurrencyId == $scope.portfolio.CalcCurrency })[0].CurrencySign
            $scope.currencyId = $scope.$parent.getLookupValues().Currencies.filter(
                function (x) { return x.CurrencyId == $scope.portfolio.CalcCurrency })[0].CurrencyId;
            var dValueUSAFields = $scope.portfolio.CalcCurrency != '9001' ? 'dValueUSA' : 'dValueNIS';

            $scope.columns = [
                { name: 'strName', colwidth: 3, paddingLeft: '2px' },
                { name: 'strSymbol', colwidth: 1 },
                { name: 'marketName', colwidth: 2, margin: '-12px' },
                { name: 'portSecWeight', colwidth: 1, isPercentage: true, margin: '-8px' },
                { name: 'sectorName', colwidth: 2, paddingLeft: '7px' },
                { name: 'flQuantity', isNumber: true, colwidth: 1 },
                { name: 'SecValue', isNumber: true, decimalPoins: 2, colwidth: 1, paddingLeft: '10px' },
                { name: 'Profit', colwidth: 1, moreInfo: true, isProfit: true }
            ];

            $scope.cacheColumns = [
                { name: 'strName', colwidth: 3, paddingLeft: '2px' },
                { name: 'Mock', colwidth: 7 },
                { name: 'SecValue', isNumber: true, decimalPoins: 2, colwidth: 2, paddingLeft: '5px', moreInfo: true }
            ];

            $scope.mobileColumns = {
                isCash: false,
                title: {
                    primary: 'strName',
                    secondary: 'strSymbol'
                },
                profit: 'Profit',
                properties: [
                    { name: 'flQuantity', title: 'Quantity', isNumber: true },
                    { name: 'portSecWeight', title: 'Weight', isPercentage: true },
                    { name: 'SecValue', title: 'Current Value', isNumber: true, decimalPoins: 2 }
                ]
            };

            $scope.mobileCachColumns = {
                isCash: true,
                title: {
                    primary: 'strName'
                },
                cacheField: 'SecValue',
                profitSpacer: true
            };

            reloadData();
        }, function (error) {
            //utilitiesSvc.showOKMessage('error', 'Failed to get portfolio', 'OK');
        });
    }

    var reloadData = function () {

        initBarChartSettings();
        initChartPie();
        $scope.tableParams.page(1);
        $scope.tableParams.reload();
    }

    var initTableParams = function () {

        $scope.tableParams = new ngTableParams({
            page: 1,            // show first page
            count: itemsInPage         // count per page

        }, {
                total: $scope.data.length, // length of data
                counts: [],
                getData: function ($defer, params) {
                    currentPage = params.page();
                    params.total($scope.data.length);
                    if ($scope.data && $scope.data.length > 0)
                        $defer.resolve($scope.data.slice((params.page() - 1) * params.count(), params.page() * params.count()));
                    else {
                        $defer.resolve($scope.data);
                    }

                }
            });
    }

    //$rootScope.$on('resizeWindow', function (event, width) {

    //    $rootScope.$broadcast('initChartBar', 'portSecurities', width-200);

    //});

    $scope.showCashRow = function () {

        //check last page in grid and search condition
        if (!$scope.data)
        {
            return false;
        }
        var str = 'cash';
        var numPages = Math.floor($scope.data.length / itemsInPage);
        return ((currentPage - 1) == numPages || ($scope.data.length % itemsInPage == 0 && currentPage == numPages)) && ($scope.searchText == "" || (str.toLowerCase().indexOf($scope.searchText.toLowerCase()) != -1));
    }

    $scope.changeOrderByBarChart = function () {
        initBarChartSettings();
    }

    var initBarChartSettings = function () {

        $scope.barLabels = [];
        $scope.barData = [];
        $scope.barData1 = [];
        $scope.deleteSecuritiesMone = 0;

        var maxBarLength = $scope.portfolio.SecurityData.length < 327 ? $scope.portfolio.SecurityData.length : 327;
        var orderByField = $scope.chartOrderByField == 'percentage' ? 'portSecWeight' : 'strSymbol';
        var orderByType = $scope.chartOrderByField == 'percentage' ? true : false;
        $scope.portfolio.SecurityData = $filter('orderBy')($scope.portfolio.SecurityData, orderByField, orderByType);
        for (var i = 0; i < maxBarLength; i++) {
            if ($scope.portfolio.SecurityData[i].strSymbol != '') {
                $scope.barLabels.push($scope.portfolio.SecurityData[i].strSymbol);
                $scope.barData.push($filter('number')($scope.portfolio.SecurityData[i].portSecWeight * 100, 2));
                $scope.barData1.push({ id: $filter('number')($scope.portfolio.SecurityData[i].portSecWeight * 100, 2) });
            }
        }

        $scope.barOptions = {
            scaleLabel: "<%=value%>%",
            fillColor: ['#6C94BF', '#2D3E50', '#496481'],
            strokeColor: ['#6C94BF', '#2D3E50', '#496481'],
            pointColor: "#6C94BF",
            barStrokeWidth: 5,
            barValueSpacing: 1,
            barDatasetSpacing: 10,
        }

        setTimeout(function () {
            $rootScope.$broadcast('initChartBar', 'portSecurities', screen.width - 200);
        }, 1000);

    }

    var initChartPie = function () {

        $scope.stockData = [];
        $scope.sectorData = [];

        var chartColors = ["#364a60", '#496481', "#7eaddf", "#2d3e58", "#5a7b9f", "#89BDF3", "#6c94bf"];
        var marketData = [];
        var sectors = []
        var stokeSeries = [];
        var sectorSeries = [];
        var stokeIndex = 0;
        var securities = $scope.portfolio.SecurityData.filter(function (x) { return x.sectorName != '' });
        for (var i = 0; i < securities.length; i++) {

            var a = $filter('filter')(marketData, { id: securities[i].idMarket }, true);
            if (a.length > 0) {
                a[0].value = a[0].value + 1;
            }
            else {
                marketData.push({ name: securities[i].marketName, value: 1, id: securities[i].idMarket });
            }

        }
        for (var i = 0; i < marketData.length; i++) {

            if (chartColors.length - 1 < stokeIndex) {
                if (chartColors.length + 1 == marketData.length)
                    stokeIndex = 1;
                else stokeIndex = 0;

            }
            stokeSeries.push({
                text: marketData[i].name,
                color: chartColors[stokeIndex]
            });

            $scope.stockData.push({
                label: marketData[i].name,
                value: (marketData[i].value * 100) / securities.length,
                color: chartColors[stokeIndex],
                labelColor: '#ffffff',
                labelFontSize: '16'
            });
            stokeIndex = stokeIndex + 1;
        }

        for (var i = 0; i < securities.length; i++) {

            var a = $filter('filter')(sectors, { id: securities[i].idSector }, true);
            if (a.length > 0) {
                a[0].value = a[0].value + 1;
            }
            else {
                sectors.push({ name: securities[i].sectorName, value: 1, id: securities[i].idSector });
            }

        }
        var sectorIndex = 0;
        for (var i = 0; i < sectors.length; i++) {

            if (chartColors.length - 1 < sectorIndex) {
                if (chartColors.length + 1 == sectors.length)
                    sectorIndex = 1;
                else sectorIndex = 0;
            }
            sectorSeries.push({
                text: sectors[i].name,
                color: chartColors[sectorIndex]
            });
            $scope.sectorData.push({

                value: (sectors[i].value * 100) / securities.length,
                color: chartColors[sectorIndex],
                label: sectors[i].name,
                labelColor: 'white',
                labelFontSize: '16'
            });

            sectorIndex = sectorIndex + 1;
        }


        $scope.stockOptions = {
            segmentShowStroke: false,
            showTooltips: false,
            series: stokeSeries,
            tooltipTemplate: function (label) {
                //display the clients and  Conversion in toolTip
                return label.label + ': ' + label.value;
            }
        };

        $scope.sectorOptions = {
            showTooltips: false,
            segmentShowStroke: false,
            series: sectorSeries,
            tooltipTemplate: function (label) {
                //display the clients and  Conversion in toolTip
                return label.label + ': ' + label.value;
            }
        };


        setTimeout(function () {
            $rootScope.$broadcast('initChartPie', 'stockPieChart');
            $rootScope.$broadcast('initChartPie', 'sectorPieChart');
            $rootScope.$broadcast('initChartPie', 'stockPieChartMobile');
            $rootScope.$broadcast('initChartPie', 'sectorPieChartMobile');
        }, 1000);

    }

    var initBenchmarkSecurities = function () {

        $scope.benchmarkSecurities = $scope.$parent.getBenchmarkSecurities();
        if ($scope.benchmarkSecurities != null)
            return;
        securitiesSvc.getBenchmarkSecurities().$promise.then(function (data) {
            $scope.benchmarkSecurities = data.Securities;
            $scope.$parent.saveBenchmarkSecurities(data.Securities);
        }, function (error) {
            //utilitiesSvc.showOKMessage('error', 'Failed to get benchmark Securities', 'OK');
            var d = [
                {
                    "$id": "2",
                    "ID": "0.1297",
                    "Name": "NYSE Composite"
                },
                {
                    "$id": "3",
                    "ID": "0.2020",
                    "Name": "S&P 500 Index"
                },
                {
                    "$id": "4",
                    "ID": "0.5177",
                    "Name": "Nasdaq 100"
                },
                {
                    "$id": "5",
                    "ID": "137",
                    "Name": "TA – 100"
                },
                {
                    "$id": "6",
                    "ID": "142",
                    "Name": "TA – 25"
                },
                {
                    "$id": "7",
                    "ID": "143",
                    "Name": "TA – 75"
                },
                {
                    "$id": "8",
                    "ID": "147",
                    "Name": "TA MidCap – 50"
                },
                {
                    "$id": "9",
                    "ID": "164",
                    "Name": "TA – Banking"
                },
                {
                    "$id": "10",
                    "ID": "168",
                    "Name": "TA – Composite"
                },
                {
                    "$id": "11",
                    "ID": "2",
                    "Name": "General Share"
                },
                {
                    "$id": "12",
                    "ID": "709",
                    "Name": "Tel – Bond 60"
                }
            ];
            $scope.$parent.saveBenchmarkSecurities(d);
        });

    }

    var getRiskColorName = function (val) {
        var value = $filter('number')(val * 100, 2) * 1;

        switch (true) {
            case (value > 0 && value <= 9):
                return 'Solid';
                break;
            case (value > 9 && value <= 14):
                return 'Low';
                break;
            case (value > 14 && value <= 25):
                return 'Moderate';
                break;
            case (value > 25 && value <= 40):
                return 'High';
                break;
            case (value > 40 && value <= 100):
                return 'Very High';
                break;
        }
        return '#ffffff';
    }

    var init = function () {

        $scope.$parent.selectMenu = 0;

        $scope.portfolioId = $routeParams.id;

        $scope.portfolios = $scope.$parent.getPortfolioList();

        getPortfolio();

        initTableParams();

        $scope.periods = [{ ID: 1, Name: 'last month' }, { ID: 2, Name: 'last 6 month' }, { ID: 3, Name: 'last year' }];

        //initBenchmarkSecurities();

        $scope.riskColors = riskColors;

        var user = $scope.$parent.getUser();



        $scope.currDay = new Date();

    }

    init();
}]);
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


app.controller("createBacktestingCtrl", ['$scope', "$location", "$window", "ngTableParams", "securitiesSvc", '$routeParams', 'utilitiesSvc', '$filter', "lookupSvc", "$timeout", "isMobile",
    function ($scope, $location, $window, ngTableParams, securitiesSvc, $routeParams, utilitiesSvc, $filter, lookupSvc, $timeout, isMobile) {

        var currentPage;
        var user;
        var timeoutId = 0;
        var minInvestmentAmount = 0;
        var lastKnownExchanges = [];
        var lastKnownSectors = [];
        var selectedExchanges = [];
        var selectedSectors = [];

        $scope.searchParams = {
            exchangesPackagees: [],
            sectors: [],
            maxRiskLevel: 0,
            pageNumber: 1,
            pageSize: isMobile.phone ? 20 : 50,
            searchText: "",
            field: "strName",
            direction: "asc",
            hideDisqualified: true
        };

        $scope.showDisqualified = false;

        $scope.riskFilter;

        $scope.data = [];

        $scope.selectedSecurities = [];

        $scope.selctesItemsNum = null;

        $scope.searchText;

        $scope.selectSecurityNum = 0;

        $scope.isRiskFilter = null;

        $scope.dataLength;

        $scope.exchanges = [];
        $scope.exchangesFilter = [];
        $scope.sectorsOptions = [];
        $scope.sectors = [];
        $scope.newPort = { Exchanges: [], benchMarks: [] };
        $scope.filterDataSource = {
            exchanges: [],
            sectors: []
        };

        $scope.selectOrUnSelectAll = function ()
        {

            if (!$scope.customType || $scope.customType == 1)
                return;

            var maxSecs = $scope.stocksCount < $scope.tableParams.total() ? $scope.stocksCount : $scope.tableParams.total();

            if ($scope.selectedSecurities.length == 0)
            {
                var page = $scope.searchParams.pageNumber;
                var size = $scope.searchParams.pageSize;
                $scope.searchParams.pageNumber = 1;
                $scope.searchParams.pageSize = $scope.stocksCount;
                var hide = $scope.searchParams.hideDisqualified;
                $scope.searchParams.hideDisqualified = true;
                $scope.selectedSecurities = [];
                $scope.selectSecurityNum = maxSecs;
                securitiesSvc.getAllSecurities($scope.searchParams).$promise.then(function (data)
                {
                    $scope.searchParams.pageNumber = page;
                    $scope.searchParams.pageSize = size;

                    for (var i = 0; i < data.Securities.length; i++)
                    {
                        $scope.selectedSecurities.push({ select: true, idSecurity: data.Securities[i].idSecurity });
                    }
                    for (var i = 0; i < $scope.data.length; i++)
                    {
                        var items = $scope.selectedSecurities.filter(function (x) { return x.idSecurity == $scope.data[i].idSecurity });
                        if (items.length > 0)
                            $scope.data[i].select = true;
                    }
                });
                $scope.searchParams.hideDisqualified = hide;
            }
            else
            {
                $scope.selectSecurityNum = 0;
                for (var i = 0; i < $scope.data.length; i++)
                {
                    $scope.data[i].select = false;
                }
            }

            $scope.selectedSecurities = [];
        }

        $scope.createBacktesting = function () {

            $scope.submit = true;
            // $scope.open();
            // return;

            $scope.newPort.Exchanges = $scope.exchanges.map(function (exchange)
            {
                return exchange.id;
            });

            if (!isValidBacktestingParams())
                return;

            if ($scope.customType == 1)
                $scope.createAutoBacktesting();
            else {
                $scope.createCustomBacktesting();
            }
        }

        $scope.createCustomBacktesting = function () {

            if (!$scope.newPort.Name || !$scope.newPort.Equity) {
                utilitiesSvc.showOKMessage('error', 'Fill all portfolio details', 'OK');
                return;
            }

            $scope.createNewPortfolio = true;

            if ($scope.selectedSecurities.length == 0) {
                utilitiesSvc.showOKMessage('message', 'Please select at least 1 security from the list below.', 'OK');
                return;
            }
            $scope.newPort.Securities = [];
            for (var i = 0; i < $scope.selectedSecurities.length; i++) {
                $scope.newPort.Securities.push($scope.selectedSecurities[i].idSecurity);
            }

            $scope.newPort.Exchanges = [];
            for (var i = 0; i < $scope.exchangesFilter.length; i++) {
                $scope.newPort.Exchanges.push($scope.exchangesFilter[i].id);
            }

            buildAndCreateBacktesting(4, $scope.newPort.Securities);
        };

        $scope.createAutoBacktesting = function () {

            if (!$scope.newPort.Name || !$scope.newPort.Equity || $scope.exchanges.length == 0) {
                utilitiesSvc.showOKMessage('error', 'Fill all portfolio details', 'OK');
                return;
            }

            else if ($scope.newPort.Equity < minInvestmentAmount) {
                utilitiesSvc.showOKMessage('error', 'Investment amount must be ' + $filter('number')(minInvestmentAmount, 0) + $scope.currency + ' or higher', 'OK');
                return;
            }

            var securities = [];
            buildAndCreateBacktesting(enumEfCalculationType.BestTP, securities);
        };

        $scope.$watch('customType', function (newValue, oldValue) {
            if (newValue == oldValue) return;
            if (newValue == "2") getSecurities(1);
        }, true);

        var buildAndCreateBacktesting = function (calcType, Securities) {

            $scope.newPort.benchMarks = $scope.newPort.benchMarks.map(function (x) { return x && x.id });
            $scope.newPort.FullbenchMarks = $filter('filter')($scope.benchMarks, filterTest, true);
            $scope.$parent.saveNewBacktestingParams($scope.newPort);

            var parameters = {
                StartDate: $scope.newPort.StartDate,
                EndDate: $scope.newPort.EndDate,
                PortfolioCommand: {
                    Name: $scope.newPort.Name,
                    Securities: Securities,
                    Equity: $scope.newPort.Equity,
                    Risk: 0,
                    Exchanges: $scope.newPort.Exchanges,
                    CalcType: calcType
                },
                BenchMarkID: $scope.newPort.benchMarks
            };

            $scope.createNewPortfolio = true;

            $scope.$parent.createPortfolio(parameters, true);
        }

        var filterTest = function (value, index, array) {
            if (!value)
                return false;
            return $scope.newPort.benchMarks.indexOf(value['id']) >= 0;
        }

        $scope.isValidPortfolioName = function () {
            if ($scope.newPort && $scope.newPort.Name && $scope.newPort.Name.length > 15 ) {
                utilitiesSvc.showOKMessage('error', 'Invalid portfolio name – Please enter a name up to 15 characters', 'OK');
                return false;
            }
            else if ($scope.newPort.Name.indexOf('/') > -1) {
                utilitiesSvc.showOKMessage('error', 'Invalid portfolio name – "/" is not allowed in portfolio name', 'OK');
                return false;
            }
            else {
                return true;
            }
        }

        $scope.search = function () {
            clearTimeout(timeoutId);
            timeoutId = setTimeout(function () {
                $scope.tableParams.page(1);
                $scope.tableParams.reload();
            }, 500);
        }

        $scope.addOrRemoveToSectorList = function (model) {
            if (!model)
                return;

            if (model.select) {
                if ($scope.selectSecurityNum == $scope.stocksCount) {
                    utilitiesSvc.showOKMessage('message', 'You have reach the max securities you can select', 'OK');
                    model.select = false;
                    return;
                }
                $scope.selectSecurityNum = $scope.selectSecurityNum + 1;
                $scope.selectedSecurities.push({ select: true, idSecurity: model.idSecurity });

            }
            else {
                $scope.selectSecurityNum = $scope.selectSecurityNum - 1;
                for (var i = 0; i < $scope.selectedSecurities.length; i++) {
                    if ($scope.selectedSecurities[i].idSecurity == model.idSecurity) {
                        $scope.selectedSecurities.splice(i, 1);
                        break;
                    }
                }
            }
        }

        $scope.setRiskFilter = function (risk) {
            if (risk == 'None') {
                $scope.isRiskFilter = false;
                $scope.riskFilter = null
            } else {
                $scope.isRiskFilter = true;
                $scope.riskFilter = risk;
            }
            $scope.searchParams.maxRiskLevel = getMaxRisk(risk);
            updatePopupFilter();
            filterChanged();
        }

        $scope.exchangesClosed = function ()
        {
            updatePopupFilter();
            var currentExchanges = $scope.exchangesFilter.slice();
            if (!arrayEquals(lastKnownExchanges, currentExchanges, 'id'))
            {
                filterChanged();
            }
            lastKnownExchanges = currentExchanges;
        }

        $scope.sectorsClosed = function ()
        {
            updatePopupFilter();
            var currentSectors = $scope.sectors.slice();
            if (!arrayEquals(lastKnownSectors, currentSectors, 'id'))
            {
                filterChanged();
            }
            lastKnownSectors = currentSectors;
        }

        function arrayEquals(array_1, array_2, fieldName)
        {
            if (fieldName)
            {
                array_1.sort(function (x, y)
                {
                    return x[fieldName] - y[fieldName];
                });
                array_2.sort(function (x, y)
                {
                    return x[fieldName] - y[fieldName];
                });
            }
            else
            {
                array_1.sort();
                array_2.sort();
            }

            return angular.equals(array_1, array_2);
        }

        $scope.setNewPortRiskType = function (type) {
            if (!$scope.newPort.PreferedRisk) { $scope.newPort.PreferedRisk = {}; }

            if ($scope.customType != 2) {
                $scope.RiskType = type;
                $scope.newPort.PreferedRisk.RiskType = getRiskValue(type);
            }
            else {
                $scope.RiskType = null;
                $scope.newPort.PreferedRisk.RiskType = null;
            }
        }

        $scope.applyFilter = function (filterData)
        {
            $scope.sectors = filterData.sectors.slice();
            $scope.exchangesFilter = filterData.exchanges.slice();
            $scope.searchParams.maxRiskLevel = filterData.riskLevel;
            filterChanged();
        }

        $scope.$on('$destroy', function () {
            for (var i in $scope) {
                if (i.indexOf('$') == 1)
                    $scope[i] = null;
            }
        })

        var isValidBacktestingParams = function () {

            if ($scope.$parent.isInProgress) {
                utilitiesSvc.showOKMessage('message', 'The system is already building a portfolio, please wait', 'OK');
                return false;
            }

            if ($scope.portForm.$invalid) {
                utilitiesSvc.showOKMessage('message', "Please fill in all required fields.", 'OK'); return false;
            }

            if ($scope.newPort.Name) {
                if (!$scope.isValidPortfolioName()) return false;
            }

            var oneday = 24 * 60 * 60 * 1000;
            if (($scope.newPort.EndDate.getTime() - $scope.newPort.StartDate.getTime()) / oneday < 30) {
                utilitiesSvc.showOKMessage('message', 'Must select a minimum of 90 days', 'OK');
                return false;
            }

            if (new Date($scope.newPort.EndDate) > new Date()) {
                utilitiesSvc.showOKMessage('message', "Can't select future date", 'OK');
                return false;
            }

            var maxRangDate = new Date($scope.newPort.StartDate.getFullYear() + 2, $scope.newPort.StartDate.getMonth(), $scope.newPort.StartDate.getUTCDate());
            if (maxRangDate < $scope.newPort.EndDate) {
                utilitiesSvc.showOKMessage('message', "Date range maximum is 2 years", 'OK');
                return false;
            }

            if (!$scope.customType || $scope.customType == '') {
                utilitiesSvc.showOKMessage('message', 'Please select a Building Process.', 'OK');
                return false;
            }

            if (new Date(user.Licence.ExpiryDate) < new Date()) {
                utilitiesSvc.showOKMessage('message', 'Your license has expired. You can no longer build new portfolios. Please contact us at info@gocherries.com to renew your license.', 'OK');
                return false;
            }

            if (user.Licence.Service.Iportfolios <= $scope.$parent.getPortfolioList().length) {
                utilitiesSvc.showOKMessage('message', "You've exceeded the number of portfolios you can create. Please delete any unwanted portfolios", 'OK');
                return false;
            }

            return true;
        }

        var getRiskValue = function (value) {

            switch (value) {
                case 0:
                    return 0;

                case 1:
                    return 9 / 100;

                case 2:
                    return 14 / 100

                case 3:
                    return 25 / 100;

                case 4:
                    return 40 / 100;

                case 5:
                    return 1;


            }
        }

        var getRiskByRiskType = function (type) {

            switch (type) {
                case 0: return "TP";

                case 1: return 'Solid';

                case 2: return "Low";

                case 3: return 'Moderate';

                case 4: return "High";

                case 5: return 'Very High';

            }
        }

        var isPrecentValueInRange = function (model) {

            var value = user.Currency.CurrencyId != '9001' ? model.avgYield : model.avgYieldNIS;
            value = $filter('number')(value * 100, 2);
            return value * 1 >= $scope.returnValueFilter * 1;
        }

        var getMaxRisk = function (value) {

            switch ($scope.riskFilter) {
                case 'Solid':
                    return 9;
                case 'Low':
                    return 14;
                case 'Moderate':
                    return 25;
                case 'High':
                    return 40;
                case 'Very High':
                    return 100;
                default:
                    return 0;
            }

        }

        var getSecurities = function (page) {
            page = page == null ? 1 : page;
            $scope.tableParams = new ngTableParams({
                page: page,            // show first page
                count: $scope.searchParams.pageSize         // count per page

            }, {
                    total: $scope.data.length, // length of data
                    counts: [],
                    getData: function ($defer, params) {
                        $scope.searchParams.pageNumber = $scope.tableParams.page();
                        $scope.searchParams.pageSize = $scope.tableParams.count();
                        $scope.searchParams.exchangesPackagees = getSelectedExchanges();
                        $scope.searchParams.sectors = getSelectedSectors();
                        $scope.searchParams.hideDisqualified = !$scope.showDisqualified;

                        securitiesSvc.getAllSecurities($scope.searchParams).$promise.then(function (data) {
                            $timeout(function () {
                                if (!data.Securities || data.Securities.length == 0) {
                                    $scope.data = [];
                                    utilitiesSvc.showOKMessage('error', 'There are no securities available', 'OK');
                                    return;
                                }
                                //utilitiesSvc.showOKMessage('message', 'Please choose Exchanges/Stocks/Sector for showing securties', 'OK');
                                $scope.data = data.Securities;
                                if ($scope.selectedSecurities.length > 0) {
                                    for (var i = 0; i < $scope.data.length; i++) {
                                        var items = $scope.selectedSecurities.filter(function (x) { return x.idSecurity == $scope.data[i].idSecurity });
                                        if (items.length > 0)
                                            $scope.data[i].select = true;
                                    }
                                }
                                params.total(data.NumOfRecords);
                                $scope.currenPageCount = params.page() * params.count() < data.NumOfRecords ? params.count() : data.NumOfRecords - ((params.page() - 1) * params.count());
                                $defer.resolve($scope.data);
                            }, 500);
                        }, function (error) {

                            //utilitiesSvc.showOKMessage('error', 'Failed to get securities', 'OK');

                        });
                    }
                });
        }

        var getLookupValues = function () {

            var data = $scope.$parent.getLookupValues();

            if (data != null) {
                $scope.sectorsOptions = data.Categories.Sector;
            }
            for (var i = 0; i < $scope.sectorsOptions.length; i++) {
                $scope.sectorsOptions[i].label = $scope.sectorsOptions[i].strValue;
            }
            for (var i = 0; i < $scope.stockMarket.length; i++) {
                $scope.stockMarket[i].label = $scope.stockMarket[i].HebName;
            }
        }

        var getBenchMarks = function () {
            securitiesSvc.getBenchmarkSecurities().$promise.then(function (data) {
                $scope.benchMarks = [];
                for (var i = 0; i < data.Securities.length; i++) {
                    $scope.benchMarks.push({ id: data.Securities[i].ID, label: data.Securities[i].Name });
                }
            });
        }

        var init = function () {

            $scope.$parent.selectMenu = 3;

            user = $scope.$parent.getUser();

            $scope.stocksCount = user.Licence.Stocks.length * 250;

            //var stdYield = user.Currency.CurrencyId != '9001' ? 'StdYield' : 'StdYieldNIS';
            $scope.avgYield = 'AvgYield';

            $scope.stockMarketSettings = {
                showCheckAll: true,
                showUncheckAll: true,
                idProp: 'id',
                sellectAllDefault: false,
                buttonDefaultText: 'select',
                buttonClasses: 'btn btn-default drop-btn',
                scrollable: true,
                scrollableHeight: 190,

            };

            $scope.benchMarkSettings = {
                showCheckAll: false,
                showUncheckAll: false,
                idProp: 'id',
                sellectAllDefault: false,
                buttonDefaultText: 'None',
                buttonClasses: 'btn btn-default drop-btn',
                scrollable: true,
                scrollableHeight: 190,
                selectionLimit: 3
            };

            $scope.benchMarkTexts = {
                buttonDefaultText: 'None'
            };

            $scope.sectorsSettings = {
                showCheckAll: true,
                showUncheckAll: true,
                idProp: 'iIndex',
                sellectAllDefault: false,
                buttonDefaultText: 'select',
                buttonClasses: 'btn btn-default drop-btn',
                scrollable: true,
                scrollableHeight: 190,

            };
            $scope.columns = [
                { name: 'strName', colwidth: 3, paddingLeft: "3px" },
                { name: 'strSymbol', colwidth: 2, paddingLeft: "10px" },
                { name: 'marketName', colwidth: 1, paddingLeft: "14px" },
                { name: 'sectorName', colwidth: 2, paddingLeft: "16px" },
                { name: $scope.avgYield, isPercentage: true, colwidth: 2, paddingLeft: "22px" },
                { name: 'rank', moreInfo: true, isNumber: true, colwidth: 2, paddingLeft: "22px" },
            ];

            $scope.mobileColumns = {
                title: {
                    primary: 'strName',
                    secondary: 'strSymbol'
                },
                properties: [
                    { name: 'sectorName', title: 'Sector' },
                    { name: 'marketName', title: 'Exchange' },
                    { name: 'rank', title: 'Rank', isNumber: true }
                ]
            };

            $scope.stockMarket = user.Licence.Stocks;

            getLookupValues();
            getBenchMarks();

            $scope.filterDataSource.exchanges = $scope.stockMarket.map(function (stock)
            {
                return {
                    id: stock.id,
                    label: stock.HebName
                };
            });

            $scope.filterDataSource.sectors = $scope.sectorsOptions.map(function (sector)
            {
                return {
                    id: sector.iIndex,
                    label: sector.strValue
                }
            });

            updatePopupFilter();

            if ($scope.customType == 2)
                getSecurities(1);

        }

        function updatePopupFilter()
        {
            var currentExchanges = ($scope.exchangesFilter || []).map(function (exch)
            {
                return exch.id;
            });
            angular.forEach($scope.filterDataSource.exchanges, function (exch)
            {
                exch.selected = currentExchanges.indexOf(exch.id) >= 0;
            });

            var currentSectors = ($scope.sectors || []).map(function (sector)
            {
                return sector.id;
            });
            angular.forEach($scope.filterDataSource.sectors, function (sector)
            {
                sector.selected = currentSectors.indexOf(sector.id) >= 0;
            });
            $scope.filterDataSource.riskLevel = $scope.searchParams.maxRiskLevel;
        }

        function filterChanged()
        {
            if ($scope.customType != 2)
            {
                return;
            }

            selectedSectors = $scope.sectors.map(function (selectedSector)
            {
                return selectedSector.id;
            });
            selectedExchanges = $scope.exchangesFilter.map(function (selectedExch)
            {
                return selectedExch.id;
            });

            $scope.tableParams.page(1);
            $scope.tableParams.reload();
        }

        function getSelectedExchanges()
        {
            if (selectedExchanges.length > 0)
            {
                return selectedExchanges.slice();
            }
            else
            {
                return $scope.stockMarket.map(function (exch)
                {
                    return exch.id;
                });
            }
        }

        function getSelectedSectors()
        {
            if (selectedSectors.length > 0)
            {
                return selectedSectors.slice();
            }
            else
            {
                return $scope.sectorsOptions.map(function (sect)
                {
                    return sect.iIndex;
                });
            }
        }

        init();
    }]);
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
app.directive('portfolio', ["$location", "$modal", "utilitiesSvc", "portfolioSvc", function ($location, $modal, utilitiesSvc, portfolioSvc) {
    return {
        restrict: 'E',
        templateUrl: 'scripts/app/partials/portfolio.min.html',
        scope: {
            model: '=',
            refreshAll: '&'
        },
        compile: function(tElement)
        {
            tElement.addClass('portfolio');

            return function (scope, element, attrs, ngModelCtrl) {
            
                scope.openPortfolio = function (port) {
                    $location.path('/portfolios/' + scope.model.ID);
               
                }

                scope.deletePortfolio = function ($event) {
               
                    $event.stopPropagation();
                    utilitiesSvc.showYesNoMessage("message", 'This portfolio will be deleted, continue?', "yes", "no").then(function (result) {
                        portfolioSvc.deletePortfolio(scope.model.ID).$promise.then(successDeletePortfolio, failedDeletePortfolio);
                   
                    });

                };

                scope.toggleSectors = function ($event) {
                    $event.stopPropagation();
                    scope.showAll = !scope.showAll;
                }

                var successDeletePortfolio = function (data) {
                    utilitiesSvc.showOKMessage('message', 'Portfolio: ' + scope.model.Name + ' deleted.', 'OK');
                    scope.refreshAll({ port: scope.model });
                };

                var failedDeletePortfolio = function (error) {
                    utilitiesSvc.showOKMessage('error', 'failed to delete portfolio: ' + scope.model.Name, 'OK');
                };

                var setRiskColor = function () {

                    var value = scope.model.CurrentStDev * 100;
                    var riskName;

                    switch (true) {
                        case (value > 0 && value <= 9):
                            riskName = 'Solid'
                            break;
                        case (value > 9 && value <= 14):
                            riskName = 'Low';
                            break;
                        case (value > 14 && value <= 25):
                            riskName = 'Moderate';
                            break;
                        case (value > 25 && value <= 40):
                            riskName = 'High';
                            break;
                        case (value > 40 && value <= 100):
                            riskName = 'Very High';
                            break;
                    }

                    scope.riskColor = scope.riskColors[riskName];

                    if (!scope.riskColor)
                        scope.riskColor = '#ffffff';

                    scope.getCurrency = function getCurrency(currency) {
                        return scope.$parent.getLookupValues().Currencies.filter(
                            function (x) { return x.CurrencyId == currency })[0].CurrencySign;
                    }
                }

                var init = function () {

                    user = scope.$parent.getUser();

                    scope.currency = user.Currency.CurrencySign;

                    scope.riskColors = riskColors;

                    setRiskColor();
                }

                init();
            }
        }
    }
}]);
app.directive('backtesting', ["$location", "$modal", "utilitiesSvc", "backtestingSvc", function ($location, $modal, utilitiesSvc, backtestingSvc) {
    return {
        restrict: 'E',
        templateUrl: 'scripts/app/partials/backtesting.html',
        scope: {
            model: '=',
            refreshAll: '&'
        },
        compile: function (tElement) {
            tElement.addClass('backtesting');

            return function (scope, element, attrs, ngModelCtrl) {
                scope.$on('$destroy', function () {
                    for (var i in scope) {
                        if (i.indexOf('$') == 1)
                            scope[i] = null;
                    }
                })
                scope.openBacktesting = function (port) {
                    backtestingSvc.getbacktesingPortfoliesById(scope.model.Details.ID).$promise.then(function (res) {
                        var a = res;
                        var fullbenchMarks = [];
                        if (res.benchMarkResult)
                        {
                            for (var i = 0; i < res.benchMarkResult.length; i++) {
                                res.benchMarkResult[i].Item1 = res.benchMarkResult[i].m_Item1;
                                res.benchMarkResult[i].Item2 = res.benchMarkResult[i].m_Item2;
                                if (res.benchMarkResult[i].Item1.ID != '0000')
                                    fullbenchMarks.push({ id: res.benchMarkResult[i].Item1.ID, label: res.benchMarkResult[i].Item1.Name });
                            }
                        }

                        var params = {
                            Equity: res.Equity,
                            FullbenchMarks: fullbenchMarks,
                            Name: scope.model.Details.Name,
                            StartDate: new Date(scope.model.StartDate),
                            EndDate: new Date(scope.model.EndDate)
                        }
                        res.Equity = scope.model.Details.Equity;
                        scope.$parent.saveBacktestingParams(params);
                        scope.$parent.setBacktestingDetails(res);
                        $location.path('/backtesting');
                    });


                    // $location.path('/backtesting');

                }

                scope.deleteBacktesting = function ($event) {

                    $event.stopPropagation();
                    utilitiesSvc.showYesNoMessage("message", 'This backtesting will be deleted, continue?', "yes", "no").then(function (result) {
                        backtestingSvc.deleteBacktesting(scope.model.Details.ID).$promise.then(successDeleteBacktesting, failedDeleteBacktesting);
                    });

                };

                scope.toggleSectors = function ($event) {
                    $event.stopPropagation();
                    scope.showAll = !scope.showAll;
                }

                var successDeleteBacktesting = function (data) {
                    utilitiesSvc.showOKMessage('message', 'Backtesting: ' + scope.model.Details.Name + ' deleted.', 'OK');
                    scope.refreshAll({ backtest: scope.model });
                };

                var failedDeleteBacktesting = function (error) {
                    utilitiesSvc.showOKMessage('error', 'failed to delete backtesting: ' + scope.model.Details.Name, 'OK');
                };

                var setRiskColor = function () {

                    var value = scope.model.Details.CurrentStDev * 100;
                    var riskName;

                    switch (true) {
                        case (value > 0 && value <= 9):
                            riskName = 'Solid'
                            break;
                        case (value > 9 && value <= 14):
                            riskName = 'Low';
                            break;
                        case (value > 14 && value <= 25):
                            riskName = 'Moderate';
                            break;
                        case (value > 25 && value <= 40):
                            riskName = 'High';
                            break;
                        case (value > 40 && value <= 100):
                            riskName = 'Very High';
                            break;
                    }

                    scope.riskColor = scope.riskColors[riskName];

                    if (!scope.riskColor)
                        scope.riskColor = '#ffffff';

                    scope.getCurrency = function getCurrency(currency) {
                        return scope.$parent.getLookupValues().Currencies.filter(
                            function (x) { return x.CurrencyId == currency })[0].CurrencySign;
                    }
                }

                var init = function () {

                    user = scope.$parent.getUser();

                    scope.currency = user.Currency.CurrencySign;

                    scope.riskColors = riskColors;

                    setRiskColor();

                }

                init();

            }
        }
    }
}]);
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
app.directive('loginHeader',  function () {
    return {
        restrict: 'EA',
        replace: true,
        templateUrl: 'Scripts/app/partials/loginHeader.min.html',
       
    };
});
"use strict";
utilities.directive('cstLoadingOverlay', ['$timeout', '$q', 'httpInterceptor', 'errorHandler', 'utilitiesSvc', '$location', function ($timeout, $q, httpInterceptor, errorHandler, utilitiesSvc, $location) {
    var IS_HTML_PAGE = /\.html$|\.html\?/i;
    var modifiedTemplates = {};

    return {
        restrict: 'EA',
        templateUrl: 'scripts/app/partials/loader.min.html',
        link: function (scope, element, attribute) {
            var requestQueue = [];
            httpInterceptor.request = function (config) {
                //console.log('request: ' + config.url);
                requestQueue.push({});
                if (requestQueue.length == 1) {
                    showOverlay(element);
                }
                return config || $q.when(config);
            };
            httpInterceptor.response = function (response) {

                //console.log('response: ' + response.config.url);
                requestQueue.pop();
                if (requestQueue.length === 0) {
                    $timeout(function () {
                        if (requestQueue.length === 0) {
                            hideOverlay(element);
                        }
                    }, 500);
                }
                if (response && response.data && response.data.Messages && errorHandler.checkErrors(response.data)) return $q.reject(response.data.Messages);
                
                return response || $q.when(response);
            };
            httpInterceptor.responseError = function (response) {
                requestQueue.pop();
                if (response && response.data && response.data.ModelState && errorHandler.checkErrors(response.data)) return $q.reject(response.data.ModelState);
                if (response.status == 401 && sessionStorage.getItem('user') != null) {
                    utilitiesSvc.showOKMessage('error', 'Your session has expired, please relogin', 'OK');
                    sessionStorage.removeItem("user");
                    sessionStorage.removeItem('userDetails');
                    $location.path('/');
                    //Smooch.destroy();
                    return $q.reject(null);
                }
                if (requestQueue.length === 0) {
                    $timeout(function () {
                        if (requestQueue.length === 0) {
                            hideOverlay(element);
                        }
                    }, 500);
                }
             
                    //window.location.href = "401.html";
                    return $q.reject(response);
            };
        }
    };

    function showOverlay(overlayDiv) {
        overlayDiv.removeClass('hide');
        overlayDiv.addClass('show');
    }

    function hideOverlay(overlayDiv) {
        overlayDiv.removeClass('show');
        overlayDiv.addClass('hide');
    }

}]);

utilities.factory('httpInterceptor', function () {
    return {};
});


app.directive('numberOnly', ["$timeout", "$compile", "$filter", function ($timeout, $compile, $filter) {
    return {
        require: 'ngModel',
        scope: {
            negativeNumber: '=?',
            isNumberFormat: '=?',
            numberLength: '=?',
            limitDecimal: '=?'
        },
        link: function (scope, element, attr, modelCtrl) {
            modelCtrl.$parsers.push(function (inputValue) {

                if (inputValue == "") {
                    return "";
                }

                var transformedInput = inputValue.toString();


                //remove chartes that not number or comma
                if (scope.negativeNumber == true) {
                    transformedInput = transformedInput.replace(/[^-?0-9,\.]/g, '');
                }
                else if (scope.isNumberFormat == true) {
                    if (scope.limitDecimal == true)
                        transformedInput = transformedInput.replace(/[^0-9,]/g, '');
                    else
                        transformedInput = transformedInput.replace(/[^0-9,\.]/g, '');
                }
                else {
                    transformedInput = transformedInput.replace(/[^0-9]/g, '');
                }
                //limit number length
                if (scope.numberLength && transformedInput.replace(/[^0-9]/g, '').length > scope.numberLength) {
                    transformedInput = transformedInput.replace(/[^0-9]/g, '');
                    transformedInput = transformedInput.substring(0, scope.numberLength);

                }

                if (transformedInput != inputValue) {
                    modelCtrl.$setViewValue(transformedInput);
                    modelCtrl.$render();
                }
                //set default number foramt
                if (scope.isNumberFormat == true) {
                    var tr = transformedInput.replace(/,/g, '');

                    if (inputValue[inputValue.length - 1] != '-' && inputValue[inputValue.length - 1] != '.' && $filter('number')(tr) != transformedInput) {
                        if (tr != '') {
                            modelCtrl.$setViewValue($filter('number')(tr, 0));
                        }
                        else {
                            modelCtrl.$setViewValue(tr);
                        }
                        modelCtrl.$render();
                    }
                }
                return transformedInput.replace(/,/g, '');
            });

            modelCtrl.$formatters.push(function (inputValue) {
                if (scope.isNumberFormat == true) {
                    return $filter('number')(inputValue);
                }
                return inputValue;
            });
        }
    };
}]);
app.directive('security', ["$location", "$modal", "utilitiesSvc", "generalSvc", "$filter", "priceSvc", "$rootScope", function ($location, $modal, utilitiesSvc, generalSvc, $filter, priceSvc, $rootScope) {
    return {
        restrict: 'E',
        templateUrl: 'scripts/app/partials/security.html',
        scope: {
            model: '=',
            index: '=',
            columns: '=',
            mobileColumns: '=',
            riskColorField: '=',
            disableMode:'=',
            canSelect: '=',
            chartWidth: '=',
            isFull: '=',
            selectModel: '&',
            currency: '=',
            isLast:'='
        },
        compile: compileDirective
    }

    function compileDirective(tElement)
    {
        tElement.addClass('security');
        return linkDirective;
    }

    function linkDirective(scope, element, attrs, ngModelCtrl)
    {
            if (!scope.model) scope.model = {};
            scope.isNIS = scope.currency != '9001';
            //scope.canSelect = scope.canSelect == 'true' ? true : false;

            scope.model.AvgYield = scope.isNIS ? scope.model.AvgYieldNIS : scope.model.AvgYield
            scope.riskColors = riskColors;

            var rateData;

            scope.changeModelSelect = function () {

                if (scope.selectModel)
                    scope.selectModel({ model: scope.model });
            }

            scope.showInfo = function () {

                if (scope.showPrices) {
                    scope.showPrices = false;
                    return;
                }

                if (scope.lineOptions != null) {
                    scope.showPrices = true;
                    return;
                }
                priceSvc.getPrices({
                    'secId': scope.model.idSecurity, 'currency': scope.currency }).$promise.then(function (data) {
                    rateData = data.Rates;
                    initChartLine(data.Rates);
                    scope.showPrices = true;
                    setTimeout(function () {
                        var secWidth = document.getElementById("sec").offsetWidth;

                        $rootScope.$broadcast('initChartLineScroll', scope.model.idSecurity, secWidth);
                    }, 100);


                    // initChartLine(data.Rates);
                });
            }

            scope.updatechartSize = function () {
                var a = 5;
            }

            scope.getRiskColor = function () {
                if (scope.model[scope.riskColorField]) {
                    var value = $filter('number')(scope.model[scope.riskColorField] * 100, 2) * 1;
                    switch (true) {
                        case (value >= 0 && value <= 9):
                            return scope.riskColors['Solid'];
                            break;
                        case (value > 9 && value <= 14):
                            return scope.riskColors['Low'];
                            break;
                        case (value > 14 && value <= 25):
                            return scope.riskColors['Moderate'];
                            break;
                        case (value > 25 && value <= 40):
                            return scope.riskColors['High'];
                            break;
                        case (value > 40 && value <= 100):
                            return scope.riskColors['Very High'];
                            break;
                    }
                }
                return '#ffffff';

            }

            $rootScope.$on('resizeWindow', function (event, width) {

                if (scope.rowHeight == 50 || scope.rowHeight == 60)
                    scope.rowHeight = width <= 1366 ? 60 : 50;
                if (scope.showPrices) {
                    var secWidth = document.getElementById("sec").offsetWidth;
                    $rootScope.$broadcast('initChartLineScroll', scope.model.idSecurity, secWidth);
                }
            });

            String.format = function (str) {
                var args = arguments;


                return str.replace(/{[0-9]}/g, function (matched) { args[parseInt(matched.replace(/[{}]/g, '')) + 1] });
            };

            var initChartLine = function (data) {

                //scope.chartWidth = 1000;

                scope.lineLabels = [];
                scope.lineLabelTags = [];
                scope.lineData = [];

                for (var i = 0; i < data.length; i++) {

                    var dsplit = data[i].Date.substring(0, data[i].Date.indexOf('T')).split("-");
                    var d = new Date(dsplit[0], dsplit[1] - 1, dsplit[2]);
                    var d2;

                    if (i > 0) {
                        dsplit = data[i - 1].Date.substring(0, data[i - 1].Date.indexOf('T')).split("-");
                        d2 = new Date(dsplit[0], dsplit[1] - 1, dsplit[2]);
                    }
                    scope.lineLabels.push($filter('date')(data[i].Date, "MM/dd/yyyy"));
                    scope.lineLabelTags.push(data[i].Label);
                    //if (i == 0 || d.getFullYear() != d2.getFullYear()) {

                    //    //var s = String.format("{0} years return:{2}{1} %", d.getFullYear().toString(), $filter('number')(data[i].RateVal, 2), '\n');
                    //    var s = d.getFullYear().toString() + ' years return: ' + $filter('number')(data[i].RateVal, 2);
                    //    scope.lineLabels.push(s);

                    //}
                    //else {

                    //    scope.lineLabels.push($filter('date')(data[i].Date, "dd/MM/yy"));
                    //}
                    scope.lineData.push(parseInt($filter('number')(data[i].RateVal, 2)));
                }

                scope.startDate = $filter('date')(data[0].Date, "MM/dd/yyyy");

                scope.endDate = $filter('date')(data[data.length - 1].Date, "MM/dd/yyyy");

                scope.lineOptions = {
                    showTooltips: true,
                    notRotateLabel: true,
                    tooltipTemplate: function (label, value) {

                        if (label.index != null && label.index >= 0) {
                            var obj = rateData[label.index];
                            //var s = String.format("Date : {0} \n Price: {1}", $filter('date')(obj.Date, "dd/MM/yy"), $filter('number')(obj.RateVal, 2));
                            var s = obj.Tooltip; //'Date : ' + $filter('date')(obj.Date, "dd/MM/yy") + ' Price: ' + $filter('number')(obj.RateVal, 2);
                            return s;
                        }
                        //display the clients and  Conversion in toolTip
                        // return label.label + ': ' + label.value;
                    },
                    fillColor: '#ADC0DA',
                    strokeColor: '#ADC0DA',
                    pointColor: '#FFFFFF',
                    barValueSpacing: 1,
                    barDatasetSpacing: 10,
                    scaleShowLabels: true,
                    scaleBeginAtZero: true
                    //datasetFill: false
                }
            }

            var initChartData = function (data) {

                scope.ChartLinePrices = {
                    labels: labels,
                    lineLabelTags: lineLabelTags,
                    series: ["Solid: 0%-9%", "Low: 9%-14%", "Moderate: 14%-25%", "Hight:25%-40%", " very Hight:40%-100%"],
                    data: allData,
                    options: {
                        //Boolean - Whether the scale should start at zero, or an order of magnitude down from the lowest value
                        scaleBeginAtZero: false,
                        //Boolean - Whether grid lines are shown across the chart
                        scaleShowGridLines: true,
                        //String - Colour of the grid lines
                        scaleGridLineColor: "rgba(0,0,0,.05)",
                        //Number - Width of the grid lines
                        scaleGridLineWidth: 1,
                        //Boolean - Whether to show horizontal lines (except X axis)
                        scaleShowHorizontalLines: false,
                        //Boolean - Whether to show vertical lines (except Y axis)
                        scaleShowVerticalLines: false,
                        //define y axis values
                        scaleShowLabels: true,
                        //Boolean - If there is a stroke on each bar
                        barShowStroke: true,
                        //Number - Pixel width of the bar stroke
                        barStrokeWidth: 5,
                        //Number - Spacing between each of the X value sets
                        barValueSpacing: 15,
                        //Number - Spacing between data sets within X values
                        barDatasetSpacing: 3,
                        //String - A legend template
                        legendTemplate: "<ul class=\"<%=name.toLowerCase()%>-legend\"><% for (var i=0; i<datasets.length; i++){%><li><span style=\"background-color:<%=datasets[i].strokeColor%>\"></span><%if(datasets[i].label){%><%=datasets[i].label%><%}%></li><%}%></ul>",
                        //Boolean - whether to make the chart responsive
                        responsive: true,
                        //datasetFill: false,
                        maintainAspectRatio: false,

                        scales: {
                            xAxes: [{
                                ticks: {
                                    display: false
                                }
                            }]
                        }
                    },
                    colours: [
                        { fillColor: '#ADC0DA', strokeColor: '#ADC0DA', pointColor: '#FFFFFF' },
                    ],
                };
            };

            var init = function () {
                scope.showPrices = false;
                scope.rowHeight = 45;
                scope.is_iPad = navigator.userAgent.match(/iPad/i) != null;
            };

            init();

            scope.$on('$destroy', function () {
                for (var i in scope) {
                    if (i.indexOf('$') == 1)
                        scope[i] = null;
                }
            })
    }
}]);
app.directive('chartLine', ["$modal", "utilitiesSvc", "$location", "$window", "$rootScope", function ($modal, utilitiesSvc, $location, $window, $rootScope) {
    return {
        restrict: 'EA',
        replace: true,
        templateUrl: 'Scripts/app/partials/chartLine.min.html',
        scope: {
            name: '=',
            labels: '=',
            allData: '=',
            options: '=',
            chartWidth: '=',
            showTooltip: '='
        },
        link: function (scope, element, attrs, ngModelCtrl) {
            var chart
            $rootScope.$on('initChartLine', function (event, chartName) {

                if (scope.name == chartName)
                    init();

            });
            scope.$on('$destroy', function () {
                if (chart != null)
                    chart.destroy();
                for (var i in scope) {
                    if (i.indexOf('$') == 1)
                        scope[i] = null;
                }
                chart = null;
            })
            var init = function () {

                if (!scope.options || scope.flag)
                    return;
                scope.flag = true;
                var ctx = document.getElementById(scope.name).getContext("2d");
                ctx.canvas.width = scope.chartWidth ? scope.chartWidth : 1280;
                ctx.canvas.height = scope.chartHeight ? scope.chartHeight : 300;
                var data = {
                    labels: scope.labels,
                    datasets: [
                        {
                            label: scope.labels,
                            fillColor: scope.options.fillColor,
                            strokeColor: scope.options.strokeColor,
                            pointColor: scope.options.pointColor,
                            barStrokeWidth: scope.options.barStrokeWidth,
                            barValueSpacing: scope.options.barValueSpacing,
                            barDatasetSpacing: scope.options.barValueSpacing,
                            data: scope.allData.filter(function (x) { return x.sectorName != '' })
                        }
                    ]
                };
                chart = new Chart(ctx).Line(data, { showTooltips: scope.showTooltip });
                

                init();
            }
        }
    }
}]);

app.directive('chartLineScroll', ["$modal", "utilitiesSvc", "$location", "$window", "$rootScope", function ($modal, utilitiesSvc, $location, $window, $rootScope) {
    return {
        restrict: 'EA',
        replace: true,
        templateUrl: 'Scripts/app/partials/chartLineScroll.min.html',
        scope: {
            name: '=',
            labels: '=',
            allData: '=',
            options: '=',
            chartWidth: '=',
            showTooltip: '=',
            labelTags: '=',
            selectPointEvent: '&'
        },
        link: function (scope, element, attrs, ngModelCtrl) {

            var chart;
            var ctx;
            var dataSource;
            scope.maxChartWidth;
            scope.$on('$destroy', function () {
                if (chart != null)
                    chart.destroy();
                for (var i in scope) {
                    if (i.indexOf('$') == 1)
                        scope[i] = null;
                }
                chart = null
            })
            $rootScope.$on('initChartLineScroll', function (event, chartName, maxWidth) {

                if (scope.name == chartName) {

                    if (!scope.chartWidth && scope.warpperWidth != (maxWidth - 50)) {
                        scope.flag = false;
                        scope.warpperWidth = maxWidth - 50;
                        scope.maxChartWidth = (window.innerWidth - 200);
                        init();
                        angular.element('#chartlinecanvas').trigger('click');
                    }
                }

            });

            var init = function () {
               if (!scope.options || scope.flag || !document.getElementById(scope.name))
                    return;
                scope.flag = true;
                dataSource = scope.allData;
                ctx = document.getElementById(scope.name).getContext("2d");
                var chartWidth = scope.allData && (scope.allData.length * 100 > scope.maxChartWidth) ? scope.allData.length * 100 : scope.maxChartWidth;
                var is_iPad = navigator.userAgent.match(/iPad/i) != null;
                ctx.canvas.width = chartWidth > 11000 && is_iPad ? 11000 : chartWidth;
                ctx.canvas.height = scope.chartHeight ? scope.chartHeight : 300;
                var data = {
                    labels: scope.labels,
                    labelTags: scope.labelTags,
                    datasets: [
                        {
                            label: scope.labels,
                            labelTags: scope.labelTags,
                            fillColor: scope.options.fillColor,
                            strokeColor: scope.options.strokeColor,
                            pointColor: scope.options.pointColor,
                            data: scope.allData.filter(function (x) { return x.sectorName != '' })
                        }
                    ]
                };

                scope.options.onAnimationProgress = function () { var self = this; setTimeout(function () { drawDatasetPointsLabels(self); }, 1); }
                scope.options.onAnimationComplete = function () { var self = this; drawYAxis(self); setTimeout(function () { drawDatasetPointsLabels(self); }, 1); }
                var canvas = document.getElementById(scope.name);

                canvas.onmousemove = function (evt) {
                    var self = this;
                    setTimeout(function () { drawDatasetPointsLabels(self); }, 1);
                }

                canvas.onmouseover = function (evt) {
                    var self = this;
                    setTimeout(function () { drawDatasetPointsLabels(self); }, 1);
                }

                canvas.onmouseenter = function (evt) {
                    var self = this;
                    setTimeout(function () { drawDatasetPointsLabels(self); }, 1);
                }

                canvas.onmouseout = function (evt) {
                    var self = this;
                    setTimeout(function () { drawDatasetPointsLabels(self); }, 1);
                }

                chart = new Chart(ctx).Line(data, scope.options);
            }

            
            init();

            function drawYAxis(obj) {
                if (obj.scale != undefined) {
                    var sourceCanvas = obj.chart.ctx.canvas;
//                    var copyWidth = obj.scale.xScalePaddingLeft + 5;
                    var copyWidth = obj.scale.xScalePaddingLeft - 4;
                    // the +5 is so that the bottommost y axis label is not clipped off
                    // we could factor this in using measureText if we wanted to be generic
                    var copyHeight = obj.scale.height - 16;
                    var targetCtx = document.getElementById(scope.name + "Axis").getContext("2d");
                    var targetContainer = element.find(".chartWrapper__axis");
                    targetContainer.width(copyWidth);
                    targetContainer.height(copyHeight);
                    targetCtx.canvas.width = copyWidth;
//                    targetCtx.canvas.height = sourceCanvas.height;
                    targetCtx.canvas.height = copyHeight;

                    var imageData = obj.chart.ctx.getImageData(0, 0, copyWidth, copyHeight);
                    targetCtx.putImageData(imageData, 0, 0);
//                    targetCtx.drawImage(sourceCanvas.toDataURL(), 0, 0, copyWidth, copyHeight);
//                    targetCtx.beginPath();
//                    targetCtx.moveTo(0, 0);
//                    targetCtx.lineTo(0, copyHeight);
//                    targetCtx.strokeStyle = '#000000';
//                    targetCtx.stroke();
                }
            }

            function drawDatasetPointsLabels(obj) {
                
                ctx.font = '16px "Gotham Book",sans-serif';
                ctx.fontWeight = 'bold';
                ctx.fillStyle = '#000';
                ctx.textAlign = "center";

                $(chart.datasets).each(function (idx, dataset) {
                    $(dataset.points).each(function (pdx, pointinfo) {
                        var label = dataset.labelTags[pdx];
                        if (label) {
                                if (pointinfo.y > 230 ){
                                ctx.fillText(label, pointinfo.x, pointinfo.y - 45);
                                ctx.beginPath();
                                ctx.moveTo(pointinfo.x, pointinfo.y - 30);
                                ctx.lineTo(pointinfo.x, pointinfo.y - 2);
                            }
                            else {
                                ctx.fillText(label, pointinfo.x, pointinfo.y + 45);
                                ctx.beginPath();
                                //ctx.setLineDash([1, 3]);
                                ctx.moveTo(pointinfo.x, pointinfo.y + 30);
                                ctx.lineTo(pointinfo.x, pointinfo.y + 2);
                            }
                            ctx.strokeStyle = '#000000';
                            ctx.stroke();
                        }
                    });
                });

                
            }
        }
    };
}]);
app.directive('chartPie', ["$modal", "utilitiesSvc", "$location", "$window", "$rootScope", function ($modal, utilitiesSvc, $location, $window, $rootScope) {
    return {
        restrict: 'EA',
        replace: true,
        templateUrl: 'Scripts/app/partials/chartPie.min.html',
        scope: {
            name: '=',
            labels: '=',
            data: '=',
            options: '=',
            chartWidth: '=',
            chartHeight: '='
        },
        link: function (scope, element, attrs, ngModelCtrl) {

            var totalValue;
            var myPieChart;
            var midX;
            var midY;
            var ctx;
            var canvas;
            scope.$on('$destroy', function () {
                if (myPieChart != null)
                    myPieChart.destroy();
                for (var i in scope) {
                    if (i.indexOf('$') == 1)
                        scope[i] = null;
                }
                myPieChart = null
            })
            $rootScope.$on('initChartPie', function (event, chartName) {

                if (scope.name == chartName)
                    init();

            });

            scope.seriesClick = function (index) {

                for (var i = 0; i < scope.options.series.length; i++) {
                    scope.options.series[i].select = false;
                }

                scope.options.series[index].select = true;
                drawSegmentValues();
            }

            var selectPoint = function (evt) {
                if (!myPieChart)
                    return;

                var points = myPieChart.getSegmentsAtEvent(evt);
                var index = myPieChart.segments.indexOf(points[0]);

                // scope.options.ySelect = points[0].value;
                // scope.options.xSelect = Number(points[0].label);
                for (var i = 0; i < scope.options.series.length; i++) {
                    scope.options.series[i].select = false;
                }

                scope.options.series[index].select = true;

                return;



            };

            var init = function () {
                if (myPieChart != null)
                    myPieChart.destroy();
                var divContainer = document.getElementById('container-' + scope.name);
                canvas = document.getElementById(scope.name);
                if (canvas) $(canvas).remove();
                canvas = document.createElement('canvas');
                canvas.id = scope.name;
                canvas.style.marginLeft = "-20px";
                divContainer.appendChild(canvas);
                ctx = canvas.getContext("2d");
                ctx.clearRect(0, 0, canvas.width, canvas.height);
                ctx.canvas.width = scope.chartWidth ? scope.chartWidth : 350;
                ctx.canvas.height = scope.chartHeight ? scope.chartHeight : 250;
                midX = canvas.width / 2;
                midY = canvas.height / 2
                totalValue = getTotalValue(scope.data);
                canvas.onclick = selectPoint;
                scope.options.showTooltips = true;
                scope.options.tooltipTemplate = "<%if (label){%><%=label %>: <%}%><%= Math.floor(value) + ' %' %>";
                scope.options.onAnimationProgress = drawSegmentValues;

                // Create a pie chart
                myPieChart = new Chart(ctx).Pie(scope.data, scope.options);

            }

            var getTextSize = function (value, isBold) {

               
                return 8;

            }

            var drawSegmentValues = function () {

                for (var i = 0; i < myPieChart.segments.length; i++) {
                    drawSegment(myPieChart.segments[i], scope.options.series[i].select)
                }
            }

            var drawSegment = function (segment, isBold) {
                var radius = myPieChart.outerRadius;
                ctx.fillStyle = "black";
                var textSize = getTextSize(segment.value, isBold);
                ctx.font = textSize + "px Verdana";
                // Get needed variables
                var value = Math.floor(segment.value);
                var startAngle = segment.startAngle;
                var endAngle = segment.endAngle;
                var middleAngle = startAngle + ((endAngle - startAngle) / 2);

                // Compute text location
                var posX = (radius / 0.98) * Math.cos(middleAngle) + midX;
                var posY = (radius / 0.98) * Math.sin(middleAngle) + midY;

                // Text offside by middle
                var w_offset = ctx.measureText(value).width / 2;
                var h_offset = textSize / 2;

                //ctx.fillText(value + '%', posX - w_offset, posY + h_offset);
            }

            var getTotalValue = function (arr) {
                if (!arr)
                    return 0;
                var total = 0;
                for (var i = 0; i < arr.length; i++)
                    total += arr[i].value;
                return total;
            }

        }

    }

}]);
app.directive('lineScattertest', ["$modal", "utilitiesSvc", "$location", "$window", "$rootScope", "underscore",
    function ($modal, utilitiesSvc, $location, $window, $rootScope, underscore)
    {
    return {
        restrict: 'EA',
        replace: true,
        templateUrl: 'Scripts/app/partials/lineScattertest.min.html',
        scope: {
            labels: '=',
            allData: '=',
            options: '=',
            chartWidth: '=',
            multiLines:'=',
            selectPointEvent: '&'
        },
        link: function (scope, element, attrs, ngModelCtrl) {

            var chart;
            var ctx;
            var canvas;
            var bgCanvas = element.find('.bg-canvas')[0];
            scope.flag;
            scope.isInit = true;
            scope.$on('$destroy', function () {
                if (chart != null)
                    chart.destroy();
                for (var i in scope) {
                    if (i.indexOf('$') == 1)
                        scope[i] = null;
                }
                chart = null
            });
            $rootScope.$on('initLineScattertest', function (event, refreshAfterReload) {

                scope.warpperWidth = window.innerWidth - 250;
                init(refreshAfterReload);

            });

            $rootScope.$on('resizeWindow', function (event, width) {
                if (scope.warpperWidth != (window.innerWidth - 250)) {
                    scope.warpperWidth = window.innerWidth - 250;
                    if (!chart)
                        return;
                    chart.destroy();
                    buildChart();
                    var prevWidth = bgCanvas.width - 68;
                    var currentWidth = (scope.chartWidth ? scope.chartWidth : scope.warpperWidth) - 68;
                    if (currentSelectionX != null)
                    {
                        currentSelectionX = ((currentSelectionX - 40) * (currentWidth / prevWidth)) + 40;
                        drawSelectionLine(currentSelectionX);
                    }
                }
            });

            var init = function (refreshAfterReload) {

                if (scope.flag)
                    return;
                scope.flag = true;
                buildChart(refreshAfterReload);
            }

            var currentPoint = null;
            var currentSelectionX = null;

            var selectPoint = function (evt) {
                if (!chart)
                    return;

                var points = chart.getPointsAtEvent(evt);
                var index = chart.datasets[0].points.indexOf(points[0]);

                if (scope.options.selectionLine && points.length > 0)
                {

                    currentPoint = points[0];
                    currentSelectionX = currentPoint.x;
                    drawSelectionLine(currentSelectionX);
                }

                for (var i = 0; i < chart.datasets.length; i++) {
                    if (chart.datasets[i].points.indexOf(points[0]) > -1) {
                        scope.options.ySelect = points[0].value;
                        scope.options.xSelect = Number(points[0].label);
                        scope.selectPointEvent({ index: chart.datasets[i].points.indexOf(points[0]), indexline: i });
                        chart.destroy();
                        buildChart();
                        return;
                    }
                }
            };

            function drawSelectionLine(x)
            {
                bgCanvas.height = '450';
                bgCanvas.width = scope.chartWidth ? scope.chartWidth : scope.warpperWidth;

                var bgCtx = bgCanvas.getContext('2d');
                bgCtx.strokeStyle = '#ff0000';
                bgCtx.lineWidth = 2;
                bgCtx.beginPath();
                bgCtx.moveTo(x, 0);
                bgCtx.lineTo(x, canvas.height);
                bgCtx.stroke();
            }

            var buildChart = function (refreshAfterReload) {
                if (chart != null)
                    chart.destroy();
                var divContainer = document.getElementById('container-lsChart');
                canvas = document.getElementById("lsChart");
                if (canvas) $(canvas).remove();
                canvas = document.createElement('canvas');
                canvas.id = "lsChart";
                canvas.height = "450";
                angular.element(canvas).css('position', 'relative');
                divContainer.appendChild(canvas);
                ctx = canvas.getContext("2d");
                ctx.canvas.width = scope.chartWidth ? scope.chartWidth : scope.warpperWidth;
                scope.options.onAnimationProgress = function () { setTimeout(function () { drawDatasetPointsLabels(); }, 0); }
                scope.options.onAnimationComplete = function () { setTimeout(function () { drawDatasetPointsLabels(); }, 0); }

                
                canvas.onclick = selectPoint;
                canvas.onmousemove = underscore.throttle(mouseMoveEventHandler, 100, { leading: false });

                function mouseMoveEventHandler(evt)
                {
                    var point = chart.getPointsAtEvent(evt);
                    if (point && point.length > 0)
                    {
                        drawDatasetPointsLabels(true);
                    }
                    else
                        drawDatasetPointsLabels();
                }

                canvas.onmouseover = function (evt) {
                    setTimeout(function () { drawDatasetPointsLabels(true); }, 0);
                }

                canvas.onmouseenter = function (evt) {
                    setTimeout(function () { drawDatasetPointsLabels(true); }, 0);
                }

                canvas.onmouseout = function (evt) {
                    setTimeout(function () { drawDatasetPointsLabels(); }, 0);
                }
                chart = new Chart(ctx).Scatter(scope.allData, scope.options);

                setTimeout(function () {
                    if (refreshAfterReload) {
                        chart.draw();
                        angular.element('#chartScatchercanvas').trigger('click');
                    }
                }, 100)
            }



            function drawDatasetPointsLabels(showPointLine) {
                ctx.font = '16px "Gotham Book",sans-serif';
                ctx.fontWeight = 'bold';
                ctx.fillStyle = '#000';
                ctx.textAlign = "center";

                $(chart.datasets).each(function (idx, dataset) {
                    $(dataset.points).each(function (pdx, pointinfo) {
                        //pointinfo.valueLabel = pointinfo.valueLabel + '%';
                        //ctx.fillText(pointinfo.value + '%', pointinfo.x, pointinfo.y - 10);
                        if (pointinfo.tagLabel !== undefined) {
                            if (pointinfo.tagLabel != 'overPoint') {

                                if (pointinfo.tagLabel && pointinfo.tagLabel.indexOf('Opt') > -1 && scope.isInit) {
                                    scope.options.ySelect = pointinfo.value;
                                    scope.options.xSelect = Number(pointinfo.label);
                                    scope.selectPointEvent({ index: pdx, indexline: idx });
                                    scope.isInit = false;
                                    chart.destroy();
                                    buildChart();
                                }
                                var pointXTag = pointinfo.x;
                                //if (chart.datasets[0].points.length - 1 == pdx)
                                //    pointXTag = scope.options.leftTag ? pointXTag - 15 : pointXTag - 25;
                                
                                if (scope.options.tagWithPath) {
                                    ctx.beginPath();
                                    var offset = 0;
                                    //ctx.setLineDash([1, 3]);
                                    if (pointinfo.y > 60)
                                    {
                                        ctx.fillText(pointinfo.tagLabel, pointXTag, scope.options.leftTag ? pointinfo.y + 20 : pointinfo.y - 50);
                                        ctx.moveTo(pointXTag, pointinfo.y - 40);
                                        ctx.lineTo(pointXTag, pointinfo.y - 5);
                                    }
                                    else {
                                        ctx.fillText(pointinfo.tagLabel, pointXTag, scope.options.leftTag ? pointinfo.y - 20 : pointinfo.y + 50);
                                        ctx.moveTo(pointXTag, pointinfo.y + 40);
                                        ctx.lineTo(pointXTag, pointinfo.y + 5);
                                    }
                                    ctx.strokeStyle = '#000000';
                                    ctx.stroke();
                                }
                            }
                            else if (showPointLine) {
                                ctx.beginPath();
                                var pointXTag = pointinfo.x;
                                var pointYTag = pointinfo.y;
                                //ctx.setLineDash([1, 3]);
                                ctx.moveTo(pointXTag, pointinfo.y);
                                ctx.lineTo(pointXTag, pointinfo.y + (ctx.canvas.height - pointinfo.y -30));
                                ctx.strokeStyle = '#000000';
                                ctx.stroke();

                                ctx.beginPath();
                                //ctx.setLineDash([1, 3]);
                                ctx.moveTo(pointXTag, pointinfo.y);
                                ctx.lineTo(45, pointYTag);
                                ctx.strokeStyle = '#000000';
                                ctx.stroke();

                            }
                        }

                    });
                });
            }

            $rootScope.$on('updateScetcherTagsAndRedPoint', function (event, index) {
                scope.isInit = true
                scope.flag = false;

            });
        }
    };
}]);
app.directive('resize', ['$window', '$rootScope', function ($window, $rootScope)
{
    return {
        restrict: 'A',
        link: function (scope, element)
        {
            var w = angular.element($window);
            scope.getWindowDimensions = function ()
            {
                return {
                    'h': w.height(),
                    'w': w.width()
                };
            };
            scope.$watch(scope.getWindowDimensions, function (newValue, oldValue)
            {
                scope.windowHeight = newValue.h;
                scope.windowWidth = newValue.w;
                setTimeout(function ()
                {
                    $rootScope.$broadcast('resizeWindow', newValue.w);
                }, 100);
            }, true);
            w.bind('resize', function ()
            {
                scope.$apply();
            });
        }
    };
}]);
app.directive('paypalPayment', ["$rootScope", "utilitiesSvc", function ($rootScope, utilitiesSvc) {
    return {
        restrict: 'E',
        templateUrl: 'scripts/app/partials/paypalPayment.min.html',
        scope: {
            model: '=',
            disable: '=',
            total: '=',
            currency:'='
        },
        link: function (scope, element, attrs, ngModelCtrl) {

            var init = function () {

                paypal.Button.render({

                    // Set your environment

                    env: 'sandbox', // sandbox | production

                    // PayPal Client IDs - replace with your own
                    // Create a PayPal app: https://developer.paypal.com/developer/applications/create

                    client: {
                        sandbox: 'AZDxjDScFpQtjWTOUtWKbyN_bDt4OgqaF4eYXlewfBP4-8aqX3PiV8e1GWU6liB2CUXlkA59kJXE7M6R',
                        production: 'Aco85QiB9jk8Q3GdsidqKVCXuPAAVbnqm0agscHCL2-K2Lu2L6MxDU2AwTZa-ALMn_N0z-s2MXKJBxqJ'
                    },

                    // Wait for the PayPal button to be clicked

                    payment: function () {

                        // Make a client-side call to the REST api to create the payment
                        
                        return paypal.rest.payment.create(this.props.env, this.props.client, {
                            transactions: [
                                {
                                    amount: { total: scope.total, currency: scope.currency }
                                }
                            ]
                        });
                    },

                    // Wait for the payment to be authorized by the customer

                    onAuthorize: function (data, actions) {

                        // Execute the payment

                        return actions.payment.execute().then(function (res) {

                         //   document.querySelector('#paypal-button-container').innerText = 'Payment Complete!';
                            $rootScope.$broadcast('paypalPaymentComplited', res);

                        }, function (error) {
                            utilitiesSvc.showOKMessage('error', 'Payment failed', 'OK');
                        });
                    }

                }, '#paypal-button-container');

            }

            init();

        }
    }
}]);

app.directive('cstNumber', ['$compile', 'utilitiesSvc',
    function ($compile, utilitiesSvc) {
        'use strict';
        return {
            restrict: "A",
            require: 'ngModel',
            compile: function (element) {

                return {
                    pre: function (scope, element, attrs, modelCtrl) {
                        setElementNameAndModel(scope, modelCtrl, element);
                        var elementName = element.closest('form')[0].name + "." + element[0].name;

                        var errorDivMax = angular.element(document.createElement('div')).attr({
                            "ng-show": '(' + $("form")[0].name + '.invalidSubmitAttempt || ' + elementName + '.$dirty ) && ' + elementName + '.$error.maxNumber',
                            "class": "error-message"
                        }).text('מספר גדול מ ' + element.attr("max"));
                        element.parent().append(errorDivMax);
                        $compile(errorDivMax)(scope);
                        var errorDivMin = angular.element(document.createElement('div')).attr({
                            "ng-show": '(' + $("form")[0].name + '.invalidSubmitAttempt || ' + elementName + '.$dirty ) && ' + elementName + '.$error.minNumber',
                            "class": "error-message"
                        }).text('מספר קטן מ ' + element.attr("min"));
                        element.parent().append(errorDivMin);
                        $compile(errorDivMin)(scope);


                        modelCtrl.errorMessage = modelCtrl.errorMessage || {};
                        modelCtrl.errorMessage.minNumber = attrs.label + ": מספר קטן מ " + attrs.min;

                        //For DOM -> model validation
                        modelCtrl.$parsers.unshift(function (value) {
                            var valid = setValid(value, "min");
                            value = getNumber(value);
                            return valid ? value : undefined;
                        });

                        //For model -> DOM validation
                        modelCtrl.$formatters.unshift(function (value) {
                            setValid(value, "min");
                            value = getNumber(value);
                            return value;
                        });

                        modelCtrl.errorMessage = modelCtrl.errorMessage || {};
                        modelCtrl.errorMessage.maxNumber = attrs.label + ": מספר גדול מ " + attrs.max;

                        modelCtrl.$parsers.unshift(function (value) {
                            var valid = setValid(value, "max");
                            value = getNumber(value);
                            return valid ? value : undefined;
                        });

                        //For model -> DOM validation
                        modelCtrl.$formatters.unshift(function (value) {
                            setValid(value, "max");
                            value = getNumber(value);
                            return value;
                        });

                        function setValid(value, type) {
                            var valid = true;
                            if (attrs[type] && value != "" && !isNaN(value)) {
                                //modelCtrl.$modelValue = value;
                                if (type == "max")
                                    valid = value <= parseFloat(attrs[type]);
                                else
                                    valid = value >= parseFloat(attrs[type]);
                            }
                            modelCtrl.$setValidity(type + 'Number', valid);
                            return valid;
                        }
                        function getNumber(val) {
                            if (val)
                                if (val.toString().indexOf('.') > -1 && val.toString().indexOf('.') < val.toString().length - 2) val = parseFloat(val).toFixed(2);
                            return val;
                        }
                    },
                    post: function (scope, element, attrs, modelCtrl) {
                        element.bind("keypress", function () {
                            var digits = this.value.replace(/\d+\.?\d{0,1}/, "");
                            for (var i = this.value.length - 1 ; i > -1 ; i--) {
                                if (digits.indexOf(this.value.charAt(i)) > -1) {
                                    this.value = this.value.slice(0, i) + this.value.slice(i + 1);
                                    break;
                                }
                            }

                            //if (this.value.indexOf('.') > -1 && this.value.indexOf('.') == this.value.length - 3) this.value = parseFloat(this.value).toFixed(2);
                            modelCtrl.$setViewValue(this.value);
                            modelCtrl.$render();
                            scope.$apply();
                        });
                    }
                }
            }
        }
    }]);

app.directive('cstZehut', ['$compile',
    function ($compile) {
        'use strict';
        return {
            restrict: "A",
            require: 'ngModel',
            compile: function (element) {

                return {
                    pre: function (scope, element, attrs, modelCtrl) {
                        setElementNameAndModel(scope, modelCtrl, element);
                        var elementName = element.closest('form')[0].name + "." + element[0].name;

                        var errorDiv = angular.element(document.createElement('div')).attr({
                            "ng-show": '(' + $("form")[0].name + '.invalidSubmitAttempt || ' + elementName + '.$dirty ) && ' + elementName + '.$error.zehut',
                            "class": "error-message"
                        }).text('ספרת ביקורת שגויה');
                        element.parent().append(errorDiv);
                        $compile(errorDiv)(scope);

                        modelCtrl.errorMessage = modelCtrl.errorMessage || {};
                        modelCtrl.errorMessage.zehut = attrs.label + ": ספרת ביקורת שגויה ";

                    },
                    post: function (scope, element, attrs, modelCtrl) {
                        element.bind("keyup", function () {
                            var digits = this.value.split('').filter(function (s) { return (!isNaN(s) && s != ' '); }).join('');
                            modelCtrl.$viewValue = digits;
                            modelCtrl.$render();
                            scope.$apply();
                        });
                        element.on("blur", function () {
                            var digits = this.value;
                            if (digits != "") {
                                digits = digits.padLeft(9);
                                var counter = 0, incNum;
                                for (var i = 0 ; i < digits.length ; i++) {
                                    incNum = Number(digits.toString().charAt(i)) * ((i % 2) + 1);//multiply digit by 1 or 2
                                    counter += (incNum > 9) ? incNum - 9 : incNum;//sum the digits up and add to counter
                                }

                                modelCtrl.$setValidity('zehut', counter % 10 == 0);
                            }
                            else
                                modelCtrl.$setValidity('zehut', true);
                            modelCtrl.$setViewValue(digits);

                            modelCtrl.$render();
                            scope.$apply();
                        });
                    }
                }
            }
        }
    }]);

app.directive('cstBirthDateYear', ['$compile',
    function ($compile) {
        'use strict';
        return {
            restrict: "A",
            require: 'ngModel',
            compile: function (element) {

                return {
                    pre: function (scope, element, attrs, ngModel) {
                        setElementNameAndModel(scope, ngModel, element);
                        var elementName = element.closest('form')[0].name + "." + element[0].name;

                        var errorDiv = angular.element(document.createElement('div')).attr({
                            "ng-show": '(' + $("form")[0].name + '.invalidSubmitAttempt || ' + elementName + '.$dirty ) && ' + elementName + '.$error.birthDateYear',
                            "class": "error-message"
                        }).text(attrs.labelTxt);
                        element.parent().append(errorDiv);
                        $compile(errorDiv)(scope);
                    },
                    post: function (scope, element, attrs, ngModel) {
                        ngModel.errorMessage = ngModel.errorMessage || {};
                        ngModel.errorMessage.birthDateYear = attrs.label + attrs.labelMin;
                        var validate = function (viewValue) {
                            var comparisonModel = attrs.minYear;
                            var comparisonModelmax = attrs.maxYear;

                            if (!viewValue || viewValue == "") {
                                // It's valid because we have nothing to compare against
                                ngModel.$setValidity('birthDateYear', true);
                                return viewValue;
                            }

                            var birthDateYear = /^\d{4}$/.test(viewValue);
                            // It's valid if model is lower than the model we're comparing against
                            ngModel.$setValidity('birthDateYear', birthDateYear && (viewValue >= attrs.minYear || viewValue <= attrs.maxYear));
                            return viewValue;
                        };

                        ngModel.$parsers.unshift(validate);
                        ngModel.$formatters.push(validate);

                        attrs.$observe('dateMinModel', function (comparisonModel) {
                            // Whenever the comparison model changes we'll re-validate
                            return validate(ngModel.$viewValue);
                        });
                        attrs.$observe('dateMaxModel', function (comparisonModelmax) {
                            // Whenever the comparison model changes we'll re-validate
                            return validate(ngModel.$viewValue);
                        });
                    }
                }
            }

        };
    }]);

app.directive('cstHebrewText', ['$compile',
    function ($compile) {
        'use strict';
        return {
            restrict: "A",
            require: 'ngModel',
            compile: function (element) {

                return {

                    pre: function (scope, element, attrs, modelCtrl) {
                        setElementNameAndModel(scope, modelCtrl, element);
                        var elementName = element.closest('form')[0].name + "." + element[0].name;

                        if (scope.textLength == null) {
                            scope.textLength = [];
                        }
                        scope.textLength = attrs.maxLength;
                        var errorDiv = angular.element(document.createElement('div')).attr({
                            "ng-show": '(' + $("form")[0].name + '.invalidSubmitAttempt || ' + elementName + '.$dirty ) && ' + elementName + '.$error.hebrew',
                            "class": "error-message"
                        }).text('יש להכניס אותיות בעברית בלבד');
                        element.parent().append(errorDiv);
                        $compile(errorDiv)(scope);

                        modelCtrl.errorMessage = modelCtrl.errorMessage || {};
                        modelCtrl.errorMessage.hebrew = attrs.label + ": יש להכניס אותיות בעברית בלבד";

                        var errorLongTextDiv = angular.element(document.createElement('div')).attr({
                            "ng-show": '(' + $("form")[0].name + '.invalidSubmitAttempt || ' + elementName + '.$dirty ) && ' + elementName + '.$error.longtext',
                            "class": "error-message"
                        }).text("יש להכניס עד {{textLength}} תווים");
                        element.parent().append(errorLongTextDiv);
                        $compile(errorLongTextDiv)(scope);

                        modelCtrl.errorMessage = modelCtrl.errorMessage || {};
                        modelCtrl.errorMessage.longtext = attrs.label + ": יש להכניס עד {{textLength}} תווים";


                    },
                    post: function (scope, element, attrs, modelCtrl) {
                        element.bind("keyup", function () {

                        });
                        element.on("blur", function (attrs) {
                            var text = this.value;
                            scope.textLength = attrs.target.attributes.getNamedItem('max-length').nodeValue;
                            if (text != "" && text != null) {
                                var testHebrew = /^([ א-ת])*$/.test(text);
                                modelCtrl.$setValidity('hebrew', testHebrew);
                                if (testHebrew && scope.textLength) {
                                    var patt = new RegExp('^[ א-תA-Z]{1,' + scope.textLength + '}$');
                                    modelCtrl.$setValidity('longtext', (patt.test(text)));
                                }
                            }
                            else
                                modelCtrl.$setValidity('hebrew', true);
                            modelCtrl.$setViewValue(text);

                            modelCtrl.$render();
                            scope.$apply();
                        });
                    }
                }
            },
        }
    }]);

app.directive('cstEnCharactersText', ['$compile',
    function ($compile) {
        'use strict';
        return {
            restrict: "A",
            require: 'ngModel',
            compile: function (element) {

                return {

                    pre: function (scope, element, attrs, modelCtrl) {
                        setElementNameAndModel(scope, modelCtrl, element);
                        var elementName = element.closest('form')[0].name + "." + element[0].name;

                        if (scope.textLength == null) {
                            scope.textLength = [];
                        }
                        scope.textLength = attrs.maxLength;
                        var errorDiv = angular.element(document.createElement('div')).attr({
                            "ng-show": '(' + $("form")[0].name + '.invalidSubmitAttempt || ' + elementName + '.$dirty ) && ' + elementName + '.$error.encharacters',
                            "class": "error-message red-text"
                        }).text('Insert only english characters');
                        element.parent().append(errorDiv);
                        $compile(errorDiv)(scope);

                        modelCtrl.errorMessage = modelCtrl.errorMessage || {};
                        modelCtrl.errorMessage.hebrew = attrs.label + ": Insert only english characters";

                    },
                    post: function (scope, element, attrs, modelCtrl) {
                        element.bind("keyup", function () {

                        });
                        element.on("blur", function (attrs) {
                            var text = this.value;
                            if (text != "" && text != null) {
                                var testenCharacters = /^([ a-zA-Z])*$/.test(text);
                                modelCtrl.$setValidity('encharacters', testenCharacters);
                            }
                            else
                                modelCtrl.$setValidity('encharacters', true);
                            modelCtrl.$setViewValue(text);

                            modelCtrl.$render();
                            scope.$apply();
                        });
                    }
                }
            },
        }
    }]);


app.directive('cstSubmit', ['utilitiesSvc', function (utilitiesSvc) {
    return {
        restrict: "A",
        link: function (scope, element, attrs) {
            element.bind('click', function () {
                var formName = $(this).closest('form')[0].name;
                if (scope[formName].$invalid) {
                    scope[formName].invalidSubmitAttempt = true;
                    var msg = [];
                    for (var error in scope[formName].$error) {
                        for (var i = 0 ; i < scope[formName].$error[error].length ; i++) {
                            msg.push(scope[formName].$error[error][i].errorMessage[error]);
                        }
                    }

                    utilitiesSvc.showOKMessage(msg.join('\n'), 'אישור', 'שגיאה - נא לתקן את השדות הבאים');
                    scope.$apply();
                    return false;
                }
                scope[formName].invalidSubmitAttempt = false;
                scope.$eval(attrs.cstSubmit);
            });
        }
    };
}]);

app.directive('cstRequired', ['$compile',
    function ($compile) {
        'use strict';
        return {
            restrict: "A",
            require: 'ngModel',
            compile: function (element) {

                return {
                    pre: function (scope, element, attrs, ngModel) {
                        setElementNameAndModel(scope, ngModel, element);
                        var elementName = element.closest('form')[0].name + "." + element[0].name;

                        var elementName = element.closest('form')[0].name + "." + element[0].name;
                        var errorDiv = angular.element(document.createElement('div')).attr({
                            "ng-show": '(' + $("form")[0].name + '.invalidSubmitAttempt || ' + elementName + '.$dirty ) && ' + elementName + '.$error.required',
                            "class": "error-message"
                        }).text('חובה');
                        element.parent().append(errorDiv);
                        $compile(errorDiv)(scope);
                    },
                    post: function (scope, element, attrs, ngModel) {
                        ngModel.errorMessage = ngModel.errorMessage || {};
                        ngModel.errorMessage.required = attrs.label + ': חובה';
                        element.prop("required", true);
                        //For DOM -> model validation
                        ngModel.$parsers.unshift(function (value) {
                            var valid = value !== "" && value != undefined;
                            ngModel.$setValidity('required', valid);
                            return valid ? value : undefined;
                        });

                        //For model -> DOM validation
                        ngModel.$formatters.unshift(function (value) {
                            ngModel.$setValidity('required', value !== "" && value != undefined);
                            return value;
                        });

                        attrs.$observe('disabled', function (value) {
                            if (value == undefined) return;
                            ngModel.$setValidity('required', value);
                        });
                    }
                }
            }

        };
    }]);

app.directive('cstLowerThenDate', ['$compile',
    function ($compile) {
        'use strict';
        return {
            restrict: "A",
            require: 'ngModel',
            compile: function (element) {

                return {
                    pre: function (scope, element, attrs, ngModel) {
                        setElementNameAndModel(scope, ngModel, element);
                        var elementName = element.closest('form')[0].name + "." + element[0].name;

                        var elementName = element.closest('form')[0].name + "." + element[0].name;
                        var errorDiv = angular.element(document.createElement('div')).attr({
                            "ng-show": '(' + $("form")[0].name + '.invalidSubmitAttempt || ' + elementName + '.$dirty ) && ' + elementName + '.$error.lowerThenDate',
                            "class": "error-message"
                        }).text(attrs.labelMax);
                        element.parent().append(errorDiv);
                        $compile(errorDiv)(scope);
                    },
                    post: function (scope, element, attrs, ngModel) {
                        ngModel.errorMessage = ngModel.errorMessage || {};
                        ngModel.errorMessage.lowerThenDate = attrs.label + attrs.labelMax;
                        var validate = function (viewValue) {
                            var comparisonModel = attrs.dateMaxModel;

                            if (!viewValue || !comparisonModel || viewValue == "") {
                                // It's valid because we have nothing to compare against
                                ngModel.$setValidity('lowerThenDate', true);
                                return viewValue;
                            }

                            // It's valid if model is lower than the model we're comparing against
                            if (!angular.isDate(viewValue)) viewValue = new Date(viewValue);
                            ngModel.$setValidity('lowerThenDate', viewValue <= new Date(comparisonModel.replace(/"/g, "")));
                            return viewValue;
                        };

                        ngModel.$parsers.unshift(validate);
                        ngModel.$formatters.push(validate);

                        attrs.$observe('dateMaxModel', function (comparisonModel) {
                            // Whenever the comparison model changes we'll re-validate
                            return validate(ngModel.$viewValue);
                        });
                    }
                }
            }

        };
    }]);

app.directive('cstBiggerThenDate', ['$compile',
    function ($compile) {
        'use strict';
        return {
            restrict: "A",
            require: 'ngModel',
            compile: function (element) {

                return {
                    pre: function (scope, element, attrs, ngModel) {
                        setElementNameAndModel(scope, ngModel, element);
                        var elementName = element.closest('form')[0].name + "." + element[0].name;

                        var elementName = element.closest('form')[0].name + "." + element[0].name;
                        var errorDiv = angular.element(document.createElement('div')).attr({
                            "ng-show": '(' + $("form")[0].name + '.invalidSubmitAttempt || ' + elementName + '.$dirty ) && ' + elementName + '.$error.biggerThenDate',
                            "class": "error-message"
                        }).text(attrs.labelMin);
                        element.parent().append(errorDiv);
                        $compile(errorDiv)(scope);
                    },
                    post: function (scope, element, attrs, ngModel) {
                        ngModel.errorMessage = ngModel.errorMessage || {};
                        ngModel.errorMessage.biggerThenDate = attrs.label + attrs.labelMin;
                        var validate = function (viewValue) {
                            var comparisonModel = attrs.dateMinModel;

                            if (!viewValue || !comparisonModel || viewValue == "") {
                                // It's valid because we have nothing to compare against
                                ngModel.$setValidity('biggerThenDate', true);
                                return viewValue;
                            }
                            if (!angular.isDate(viewValue)) viewValue = new Date(viewValue);
                            // It's valid if model is lower than the model we're comparing against
                            ngModel.$setValidity('biggerThenDate', viewValue >= new Date(comparisonModel.replace(/"/g, "")));
                            return viewValue;
                        };

                        ngModel.$parsers.unshift(validate);
                        ngModel.$formatters.push(validate);

                        attrs.$observe('dateMinModel', function (comparisonModel) {
                            // Whenever the comparison model changes we'll re-validate
                            return validate(ngModel.$viewValue);
                        });
                    }
                }
            }

        };
    }]);

app.directive('cstEmail', ['$compile',
    function ($compile) {
        'use strict';
        return {
            restrict: "A",
            require: 'ngModel',
            compile: function (element) {

                return {
                    pre: function (scope, element, attrs, modelCtrl) {
                        setElementNameAndModel(scope, modelCtrl, element);
                        var elementName = element.closest('form')[0].name + "." + element[0].name;

                        var errorDiv = angular.element(document.createElement('div')).attr({
                            "ng-show": '(' + $("form")[0].name + '.invalidSubmitAttempt || ' + elementName + '.$dirty ) && ' + elementName + '.$error.email',
                            "class": "error-message"
                        }).text(attrs.labelTxt);
                        element.parent().append(errorDiv);
                        $compile(errorDiv)(scope);

                        modelCtrl.errorMessage = modelCtrl.errorMessage || {};
                        modelCtrl.errorMessage.email = attrs.label + ': כתובת דוא"ל שגויה ';

                    },
                    post: function (scope, element, attrs, modelCtrl) {

                        element.on("blur", function () {
                            var email = this.value;
                            if (email != "") {
                                var testMail = /^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$/.test(email);
                                modelCtrl.$setValidity('email', testMail);
                            }
                            else
                                modelCtrl.$setValidity('email', true);
                            modelCtrl.$setViewValue(email);

                            modelCtrl.$render();
                            scope.$apply();
                        });
                    }
                }
            }
        }
    }]);

function setElementNameAndModel(scope, modelCtrl, element) {
    if (scope.$index != undefined) {
        scope[element.closest('form')[0].name].$removeControl(modelCtrl);
        var name = element[0].name;
        name = name.replace(/\{\{\$index\}\}/g, scope.$index);
        if (scope[element.closest('form')[0].name][name] != undefined) name += "_" + scope.$index;
        element[0].name = name;
        //var modelCtrl = ctrls[0];
        modelCtrl.$name = name;
        scope[element.closest('form')[0].name].$addControl(modelCtrl);
        element.on('$destroy', function () {
            scope[element.closest('form')[0].name].$removeControl(modelCtrl);
        });
    }
}

app.directive('uiBlur', function () {

    return function (scope, elem, attrs) {
        elem.bind('blur', function () {
            scope.$apply(attrs.uiBlur);
        });
    };
});


app.directive('cstPadZero', function () {
    return {
        restrict: 'A',
        replace: true,
        link: function (scope, elem, attrs) {
            elem.bind('blur', function () {
                var n = attrs.cstPadZero;
                if (elem.context.value == "" || elem.context.value.length >= n)
                    return;

                var zeros = "";
                for (i = elem.context.value.length; i < n; i++)
                    zeros += "0";

                elem.context.value = zeros + elem.context.value;
            });
        }
    };
});
app.directive('phoneValidation', ["$timeout", "$compile", "$filter", function ($timeout, $compile, $filter) {
    return {
        require: 'ngModel',
        scope: {
        },
        link: function (scope, element, attr, modelCtrl) {
            modelCtrl.$parsers.push(function (inputValue) {

                if (inputValue == "") {
                    return "";
                }
                var transformedInput = inputValue;
                //remove chartes that not number or comma
                transformedInput = transformedInput.replace(/[^0-9]/g, '').substring(0, 7);

                if (transformedInput != inputValue) {
                    modelCtrl.$setViewValue(transformedInput);
                    modelCtrl.$render();
                }

                return transformedInput.replace(/,/g, '');
            });

            modelCtrl.$formatters.push(function (inputValue) {
                return inputValue;
            });
        }
    };
}]);

app.directive('maxLength', ['$compile', function ($compile) {

    return {
        require: 'ngModel',
        scope: {
            maxCharters: '=?',
        },
        link: function (scope, element, attr, modelCtrl) {
            modelCtrl.$parsers.push(function (inputValue) {

                if (inputValue == "") {
                    return "";
                }

                var transformedInput = inputValue;
            
                //limit text length

                transformedInput = transformedInput.substring(0, scope.maxCharters);

                if (transformedInput != inputValue) {
                    modelCtrl.$setViewValue(transformedInput);
                    modelCtrl.$render();
                }
                //set default number foramt
               
                return transformedInput;
            });

            modelCtrl.$formatters.push(function (inputValue) {
               
                return inputValue;
            });
        }
    };
}]);
app.directive('focus',['$timeout', function ($timeout) {
    return function (scope, element, attrs) {
        $timeout(function () {
            if (attrs.focusme)
                element[0].focus();
        },500);
    }
}]);
app.directive('sortableHeader', ['$timeout', function ($timeout, $window) {

    return {
        restrict: 'A',
        scope: {
            model: '=',
            sort: '&',
            readOnly:'='
        },
        link: function (scope, element, attrs) {
            $timeout(function () {
                var $sortable = element.find('.sortable');
                $sortable.on('click', function () {
                    if (scope.readOnly) return;
                    var asc = $(this).hasClass('asc');
                    var desc = $(this).hasClass('desc');
                    $sortable.removeClass('asc').removeClass('desc');
                    scope.model.field = $(this).attr('field');
                    if (desc || (!asc && !desc)) {
                        $(this).addClass('asc');
                        scope.model.direction = 'asc';
                    } else {
                        $(this).addClass('desc');
                        scope.model.direction = 'desc';
                    }
                    scope.sort();
                });
            }, 0);
        }
    }
}]);
app.factory('serviceHelperSvc', ['$http', '$resource', function ($http, $resource) {
    var baseUrl = '';
    var buildUrl = function (resourceUrl) {
        return baseUrl + resourceUrl;
    };
    var Invoker = $resource(buildUrl('api/Invoker'));

    var InvokerWithCache = $resource(buildUrl('api/Invoker'), {}, {
        get: {
            method: "GET",
            cache: true
        }
    });

    Invoker.createMethodName = function (method) {
        return method + "?key=" + sessionStorage.getItem('sessionid');
    }

    Invoker.convertJsonToUri = function (model) {
        var params = "&";
        for (p in model) {
            params += p + "=" + model[p] + "&";
        }
        return params;
    }
    return {
        Invoker: Invoker,

        InvokerWithCache: InvokerWithCache,

        Users: $resource(buildUrl('api/User')),

        logoff: $resource(buildUrl('api/User/Logoff')),

        updateUserPassword: $resource(buildUrl('api/User'), null,
            {
                updatePassword: { method: 'put' }
            }),

        SendConfirmCode: $resource(buildUrl('api/User/SendConfirmCode')),

        VerifyConfirmCode:$resource(buildUrl('api/User/VerifyConfirmCode')),

        VerifyUsername: $resource(buildUrl('api/User/VerifyUsername')),

        CreateUser: $resource(buildUrl('api/User/Create')),

        updateUserDetails:$resource(buildUrl('api/User/Update')),

        Portfolios: $resource(buildUrl('api/Portfolios')),

        updatePortfolios: $resource(buildUrl('api/Portfolios'), null,
          {
              updatePort: { method: 'put' }
          }),

        PortfoliosId: $resource(buildUrl('api/Portfolios/:id'), { id: "@id" }),

        Securities: $resource(buildUrl('api/Securities'), null,
            {
                getAll: { method: 'get', url: buildUrl('api/Securities/GetAll') }
            }),

        Optimization: $resource(buildUrl('api/Optimization')),

        GetBenchmarkSecurities: $resource(buildUrl('api/Securities/GetBenchmarkSecurities')),

        Backtesting: $resource(buildUrl('api/Backtesting')),

        BacktestingId: $resource(buildUrl('api/Backtesting/:id'), { id: "@id" }),

        BacktestingPortfolios: $resource(buildUrl('api/BacktestingPortfolios')),

        BacktestingPortfoliosId: $resource(buildUrl('api/BacktestingPortfolios/:portId'), { portId: "@portId" }),

        Prices: $resource(buildUrl('api/Prices')),

        License: $resource(buildUrl('api/License')),
      
        CalculateLicense:  $resource(buildUrl('api/License/Calculate')),

        Lookups: $resource(buildUrl('api/Lookup')),

        Session: {
            removeSession: function () {
                var xhr = new XMLHttpRequest();
                xhr.open('post', buildUrl('api/Users/RemoveSession?key=' + sessionStorage.getItem('sessionid')), false);
                //xhr.setRequestHeader("Authorization", $http.defaults.headers.common["Authorization"]);
                xhr.send();
                return true;
            }
        },

        exportToExcel: function (methos, data, type) {
            return $http({
                url: buildUrl('api/Invoker'),
                method: "GET",
                params: {
                    method: methos + '?key=' + sessionStorage.getItem('sessionid'),
                    param: Invoker.convertJsonToUri(data)
                }
            }
            );
        },

        UserGroup: $resource(buildUrl('api/users/GetUserGroup'))
    }
}]);
app.factory('loginSvc', ['$window', '$http', '$q', 'serviceHelperSvc', function ($window, $http, $q, serviceHelperSvc) {

    var applicant = serviceHelperSvc.Users;
    var logoff = serviceHelperSvc.logoff;
    var updateUserPassword = serviceHelperSvc.updateUserPassword;
    var sessionId;

    var setSessionHeader = function (id) {
        sessionId = id;
        $http.defaults.headers.common["Authorization"] = "Basic " + id;
    }

    $window.onbeforeunload = function () {
        if (sessionId)
            sessionStorage.setItem('sessionid', angular.toJson(sessionId));
    };



    var login = {
        get: function (serchParams) {
            var deferred = $q.defer();
            applicant.save(serchParams,
                function (data, getResponseHeaders) {
                    if (data.User != null) {
                        setSessionHeader(getResponseHeaders("sessionid"));
                    }

                    deferred.resolve(data);
                }, function (error) {
                    deferred.reject(error);
                });

            return deferred.promise;
        },
        logoff: function () {
            return logoff.get();
        },

        updatePasswrod: function (parameters) {

            return updateUserPassword.updatePassword(parameters);
        }

    }
    return login;
}]);
app.factory('utilitiesSvc', ['prompt', '$q', function (prompt, $q) {

    return {
        showOKMessage: function (header, msg, okBtnText, hebrewForamt, showNoSession) {
            if (!sessionStorage.getItem('user') && !showNoSession)
                return;
            if (okBtnText == '') okBtnText = 'Ok';
            var promptPromise = prompt({
                title: header,
                message: msg,
                heformat: hebrewForamt,
                buttons: [
                    {
                        "label": okBtnText,
                        "cancel": false,
                        "primary": true,
                        "isfocus": true
                    }
                ]
            });

            return promptPromise;
        },

        showOKMessageWithIndication: function (header, msg, okBtnText, hebrewForamt) {
            if (!okBtnText) okBtnText = 'Ok';
            var defer = $q.defer();
            prompt({
                title: header,
                message: msg,
                heformat: hebrewForamt,
                buttons: [
                    {
                        "label": okBtnText,
                        "cancel": false,
                        "primary": true,
                        "isfocus": true
                    }
                ]
            }).then(function (result) {
                defer.resolve();
            });
            return defer.promise;
        },

        showYesNoMessage: function (header, msg, yesBtnText, noBtnText) {
            var defer = $q.defer();
            prompt({
                title: header,
                message: msg,
                buttons: [
                    {
                        "label": yesBtnText,
                        "cancel": false,
                        "primary": true,
                        "class": 'btn-green'
                    },
                    {
                        "label": noBtnText,
                        "cancel": true,
                        "primary": false,
                        "class": 'bold',
                        "isfocus": true
                    }
                ]
            }).then(function (result) {
                defer.resolve();
            }, function () {
                defer.reject()
            });
            return defer.promise;
        },

        exportToPdf: function (elementId, fileName) {

            var deferred = $q.defer();

            var canvasToImage = function (canvas) {
                var img = new Image();
                var dataURL = canvas.toDataURL('image/png');
                img.src = dataURL;
                return img;
            };
            var canvasShiftImage = function (oldCanvas, shiftAmt) {
                shiftAmt = parseInt(shiftAmt) || 0;
                if (!shiftAmt) { return oldCanvas; }

                var newCanvas = document.createElement('canvas');
                newCanvas.height = oldCanvas.height - shiftAmt;
                newCanvas.width = oldCanvas.width;
                var ctx = newCanvas.getContext('2d');

                var img = canvasToImage(oldCanvas);
                ctx.drawImage(img, 0, shiftAmt, img.width, img.height, 0, 0, img.width, img.height);

                return newCanvas;
            };


            var canvasToImageSuccess = function (canvas) {
                var pdf = new jsPDF('l', 'px'),
                    pdfInternals = pdf.internal,
                    pdfPageSize = pdfInternals.pageSize,
                    pdfScaleFactor = pdfInternals.scaleFactor,
                    pdfPageWidth = pdfPageSize.width,
                    pdfPageHeight = pdfPageSize.height,
                    totalPdfHeight = 0,
                    htmlPageHeight = canvas.height,
                    htmlScaleFactor = canvas.width / (pdfPageWidth * pdfScaleFactor),
                    safetyNet = 0;

                while (totalPdfHeight < htmlPageHeight && safetyNet < 15) {
                    var newCanvas = canvasShiftImage(canvas, totalPdfHeight);
                    pdf.addImage(newCanvas, 'png', 0, 0, pdfPageWidth, 0, null, 'NONE');

                    totalPdfHeight += (pdfPageHeight * pdfScaleFactor * htmlScaleFactor);

                    if (totalPdfHeight < htmlPageHeight) {
                        pdf.addPage();
                    }
                    safetyNet++;
                }

                pdf.save(fileName + '.pdf');
                deferred.resolve();
            };
            $('body').css('overflow', 'auto');
            var h = $('.height-page-body').css('max-height');
            $('.height-page-body').css('max-height', 'none');
            html2canvas($('#' + elementId)[0], {

                onrendered: function (canvas) {
                    canvasToImageSuccess(canvas);
                    $('.height-page-body').css('max-height', h);
                    // $('body').css('overflow', 'hidden');
                }
            });

            return deferred.promise;
        }
    }
}]);
app.factory('portfolioSvc', ['$window', '$resource', '$q', 'serviceHelperSvc', function ($window, $resource, $q, serviceHelperSvc) {

    var portfolios = serviceHelperSvc.Portfolios;
    var updatePortfolios = serviceHelperSvc.updatePortfolios;
    var portfoliosId = serviceHelperSvc.PortfoliosId;

    return {

        getPortfolios: function (paramerts) {
            return portfolios.get({
                "pageSize": paramerts.pageSize, "pageNumber": paramerts.pageNumber, "sortField": paramerts.sortField
            });
        },
        createPortfolio: function (parameters) {
            return portfolios.save(parameters);
        },
        updatePortfolio: function (parameters) {

            return updatePortfolios.updatePort(parameters);
        },
        getSinglePortfolio: function (id) {
            return portfoliosId.get({ "id": id });
        },
        deletePortfolio: function (id) {
            return portfolios.delete({ "portId": id });
        },
        exportOptimize: function (paramerts) {
            return serviceHelperSvc.Optimization.save(paramerts);
        }
    }

}]);
app.factory('errorHandler', ['prompt', function (prompt) {
    return {
        checkErrors: function (data) {
            var errorMessage = []
            if (data.Messages!= null && data.Messages.length > 0) {
                errorMessage = Enumerable.From(data.Messages)
                    .Where(function (x) { return x.LogLevel == Log.Level.Fatal || x.LogLevel == Log.Level.Error })
                    .Select(function (x) { return x.Text }).ToArray();
                
            }
            else if (data.ModelState != null) {
                for (var f in data.ModelState) {
                    if (f != '$id') {
                        for (var i = 0 ; i < data.ModelState[f].length ; i++)
                            errorMessage.push(data.ModelState[f][i]);
                    }
                }
            }
            if (errorMessage.length > 0) {
                var msg = errorMessage.join("<br>");
                prompt({
                    title: 'Error',
                    message: msg,
                    buttons: [
                            {
                                "label": "OK",
                                "cancel": false,
                                "primary": true
                            }
                    ]
                })
                return true;
            }
            return false;
        }
    }
}]);
app.factory('securitiesSvc', ['serviceHelperSvc', function (serviceHelperSvc) {

    var securities = serviceHelperSvc.Securities;
    var benchmarkSecurities = serviceHelperSvc.GetBenchmarkSecurities;

    return {

        getSecurities: function () {
            return securities.get();
        },
        getBenchmarkSecurities: function () {
            return benchmarkSecurities.get();
        },
        getAllSecurities : function (data) {
            return securities.getAll(data);
        }
    }

}]);
app.factory('lookupSvc', ['$window', '$http', '$q', 'serviceHelperSvc', function ($window, $http, $q, serviceHelperSvc) {

    var lookups = serviceHelperSvc.Lookups;

    return {

        getLookup: function () {
            return lookups.get();
        }


    }

}]);
app.factory('generalSvc', ['serviceHelperSvc', function (serviceHelperSvc) {
    var general = serviceHelperSvc.InvokerWithCache;
    var invok = serviceHelperSvc.Invoker;
    return {
        getGeneralObject: function (objectName) {
            return general.get({"method" : invok.createMethodName("offices")});
            //return general.get({"objectName": objectName });
           
        }

    }
}]);

app.filter('percentage', ['$filter', function ($filter) {
    return function (input, decimals) {
        return $filter('number')(input * 100, decimals) + '%';
    };
}]);

//alert('before filter');
app.filter('DistirictCountry', function () {
    return function (input, code, type) {
        var out = [];
        if (input != undefined) {
            for (var i = 0 ; i < input.length ; i++) {
                var value = input[i][type];
               
                if (value == code || value == -1) {
                    out.push(input[i])
                }
            }
        }
        return out;
    }
});
app.factory('priceSvc', ['serviceHelperSvc', function (serviceHelperSvc) {

    var Prices = serviceHelperSvc.Prices;

    return {

        getPrices: function (params) {
            return Prices.get(params);
        }

    }

}]);
app.factory('backtestingSvc', ['serviceHelperSvc', function (serviceHelperSvc) {

    var backtesting = serviceHelperSvc.Backtesting;
    var backtesingPortfolies = serviceHelperSvc.BacktestingPortfolios;
    var backtestingId = serviceHelperSvc.BacktestingId;
    var portfoliosId = serviceHelperSvc.PortfoliosId;

    return {

        createBacktesting: function (paramerts) {
            return backtesting.save(paramerts);
        },
        getbacktesingPortfolies: function (paramerts) {
            return backtesingPortfolies.get({ "pageSize": paramerts.pageSize, "pageNumber": paramerts.pageNumber, "sortField": paramerts.sortField });
        },
        getbacktesingPortfoliesById: function (id) {
            return backtestingId.get({ "id": id });
        },
        deleteBacktesting: function (portId) {
            return backtesingPortfolies.delete({ "portId": portId });
        },

    }

}]);
app.factory('exportDataSvc', function () {
    var download = function (data, getResponseHeaders, fileName) {
        var octetStreamMime = 'application/octet-stream';
        var success = false;

        // Get the headers
        // headers = getResponseHeaders();

        // Get the filename from the x-filename header or default to "download.bin"
        var filename = fileName;

        // Determine the content type from the header or default to "application/octet-stream"
        var contentType = octetStreamMime;

        var binary_string = window.atob(data);
        var len = binary_string.length;
        var bytes = new Uint8Array(len);
        for (var i = 0; i < len; i++) {
            bytes[i] = binary_string.charCodeAt(i);
        }
        data = bytes;
        try {
            // Try using msSaveBlob if supported
            var blob = new Blob([data], { type: contentType });
            if (navigator.msSaveBlob)
                navigator.msSaveBlob(blob, filename);
            else {
                // Try using other saveBlob implementations, if available
                var saveBlob = navigator.webkitSaveBlob || navigator.mozSaveBlob || navigator.saveBlob;
                if (saveBlob === undefined) throw "Not supported";
                saveBlob(blob, filename);
            }
            success = true;
        } catch (ex) {

        }

        if (!success) {
            // Get the blob url creator
            var iOS = /iPad|iPhone|iPod/.test(navigator.userAgent) && !window.MSStream;
            if (iOS){
                var reader = new FileReader();
                reader.onload = function (e) {
                    var bdata = btoa(reader.result);
                    var datauri = 'data:' + 'text/csv;charset=utf-8;' + ';base64,' + bdata;
                    newWindow = window.open(datauri);
                    newWindow.document.title = filename;
                };
                var blob = new Blob([data], { type: contentType });
                reader.readAsBinaryString(blob);
                return;
            }
            var urlCreator = window.URL || window.webkitURL || window.mozURL || window.msURL;
            if (urlCreator) {
                // Try to use a download link
                var link = document.createElement('a');
                try {
                    // Prepare a blob URL

                    var blob = new Blob([data], { type: contentType });
                    var url = urlCreator.createObjectURL(blob);
                    link.setAttribute('href', url);

                    // Set the download attribute (Supported in Chrome 14+ / Firefox 20+)
                    link.setAttribute("download", filename);

                    // Simulate clicking the download link
                    var event = document.createEvent('MouseEvents');
                    event.initMouseEvent('click', true, true, window, 1, 0, 0, 0, 0, false, false, false, false, 0, null);
                    link.dispatchEvent(event);

                    success = true;

                } catch (ex) {

                }

                if (!success) {
                    // Fallback to window.location method
                    try {
                        // Prepare a blob URL
                        // Use application/octet-stream when using window.location to force download

                        var blob = new Blob([data], { type: octetStreamMime });
                        var url = urlCreator.createObjectURL(blob);
                        window.location = url;
                        success = true;
                    } catch (ex) {

                    }
                }

            }
        }

        if (!success) {
            alert('היצוא נכשל');
        }
    }

    return {
        download: download
    }
});
(function (angular)
{
    angular.module('takinApp').constant('tfiCommonConstants', {

        // Event names
        updateAccountData: 'updateAccountData',
        isInProgress: 'isInProgress',
        wantToLogout: 'wantToLogout',
        menuItemChanged: 'menuItemChanged',
        objNameChanged: 'objNameChanged',
        isDataReady: 'isDataReady'
    });

})(angular);
app.factory('userSvc', ['$window', '$http', '$q', 'serviceHelperSvc', function ($window, $http, $q, serviceHelperSvc) {


    return {

        sendUserConfirmCode: function (paramerts) {

            return serviceHelperSvc.SendConfirmCode.save(paramerts);
        },
        verifyConfirmCode: function (params) {
            return serviceHelperSvc.VerifyConfirmCode.save(params);
        },
        verifyUsername: function(params) {
            return serviceHelperSvc.VerifyUsername.save(params);
        },
        createUser: function (parameters) {
            return serviceHelperSvc.CreateUser.save(parameters);
        },

        updateUesrDetails: function (paramaters) {
            return serviceHelperSvc.updateUserDetails.save(paramaters);
        }

    }


}]);
app.factory('licenseSvc', ['serviceHelperSvc', function (serviceHelperSvc) {

    var license = serviceHelperSvc.CalculateLicense;

    return {

        calculateLicense: function (params) {
            return license.get(params);
        },
        updateUserLicence: function (params) {
            return serviceHelperSvc.License.save(params);
        }

    }

}]);
var buttonsType = {
    OK: 'OK'
}

var messagesType = {
    error: 'Error',
    message: 'Message'
}

var riskColors = {
    TP: '#d7dbdc',
    Solid: '#2fd074',
    Low: '#f9ec23',
    Moderate: '#f9b50a',
    High: '#ff8812',
    'Very High': '#fc3d5e',
    none: '#ffffff'
}

var Log = {}

Log.Level = {
    Fatal: 0,
    Error: 1,
    Warning: 2,
    Info: 3
}
var enumEfCalculationType =
{
    BestTP: 1,
    BestRisk: 2,
    SecurityStd: 3,
    Custom: 4
}

var Licensetype = {
    'FullLicense': 1,
    'FollowupLicense': 2,
    'Trial': 3
}
var messages = {

    successLoginMsg: 'כניסתך לאתר אומתה בהצלחה',
    failedLoginMsg:'כניסתך לאתר נכשלה '
}
app.factory('errorHandler', ['prompt', function (prompt) {
    return {
        checkErrors: function (data) {
            var errorMessage = []
            if (data.Messages!= null && data.Messages.length > 0) {
                errorMessage = Enumerable.From(data.Messages)
                    .Where(function (x) { return x.LogLevel == Log.Level.Fatal || x.LogLevel == Log.Level.Error })
                    .Select(function (x) { return x.Text }).ToArray();
                
            }
            else if (data.ModelState != null) {
                for (var f in data.ModelState) {
                    if (f != '$id') {
                        for (var i = 0 ; i < data.ModelState[f].length ; i++)
                            errorMessage.push(data.ModelState[f][i]);
                    }
                }
            }
            if (errorMessage.length > 0) {
                var msg = errorMessage.join("<br>");
                prompt({
                    title: 'Error',
                    message: msg,
                    buttons: [
                            {
                                "label": "OK",
                                "cancel": false,
                                "primary": true
                            }
                    ]
                })
                return true;
            }
            return false;
        }
    }
}]);
(function ($) {
    $.extend($.datepicker, {

        // Reference the orignal function so we can override it and call it later
        _inlineDatepicker2: $.datepicker._inlineDatepicker,

        // Override the _inlineDatepicker method
        _inlineDatepicker: function (target, inst) {

            // Call the original
            this._inlineDatepicker2(target, inst);

            var beforeShow = $.datepicker._get(inst, 'beforeShow');

            if (beforeShow) {
                beforeShow.apply(target, [target, inst]);
            }
        }
    });

    $.datepicker._gotoToday = function (id) {
        var target = $(id);
        var date = new Date();
        if (target.attr('allowd-days') != null) {
            var days = target.attr('allowd-days').split(',');
            if (days.indexOf(date.getDay().toString() == -1)) return;
        }
        var inst = this._getInst(target[0]);
        if (this._get(inst, 'gotoCurrent') && inst.currentDay) {
            inst.selectedDay = inst.currentDay;
            inst.drawMonth = inst.selectedMonth = inst.currentMonth;
            inst.drawYear = inst.selectedYear = inst.currentYear;
        }
        else {

            inst.selectedDay = date.getDate();
            inst.drawMonth = inst.selectedMonth = date.getMonth();
            inst.drawYear = inst.selectedYear = date.getFullYear();
            // the below two lines are new
            this._setDateDatepicker(target, date);
            this._selectDate(id, this._getDateDatepicker(target));
        }
        this._notifyChange(inst);
        this._adjustDate(target);
    }
}(jQuery));


utilities.directive('cstDatepicker', ['$timeout', function ($timeout) {
    return {
        restrict: "A",
        require: 'ngModel',
        replace: true,
        compile: function (tElement)
        {
            tElement.addClass('cst-datepicker');
            return linkDirective;
        }
    };

    function linkDirective(scope, element, attrs, ngModelCtrl)
    {
        var formatDate = attrs.formatDate ? attrs.formatDate : "dd/mm/yy";
        var allowdDays = attrs.allowdDays ? attrs.allowdDays.split(',') : [0, 1, 2, 3, 4, 5, 6];
        $timeout(function ()
        {
            var startData = new Date();
            startData.setMonth(startData.getMonth() - 18);

            var dPicker = element.datepicker({
                showOn: "button",
                buttonImage: "Content/themes/images/Calandar.png",
                buttonImageOnly: true,
                buttonText: "Select date",
                changeMonth: true,
                changeYear: true,
                showButtonPanel: true,
                dateFormat: formatDate,
                minDate: startData,
                maxDate: new Date(),
                //yearRange: "2016:+0",
                onSelect: function (d) {
                    var date = $(this).datepicker('getDate');
                    var h = date.getHours();
                    date.setHours(h + 3);

                    ngModelCtrl.$setViewValue(date);
                    scope.$apply();
                    ngModelCtrl.$setValidity('invalid', true);
                },
                onClose: function (dateText, inst) {
                    //var month = $("#ui-datepicker-div .ui-datepicker-month :selected").val();
                    //var year = $("#ui-datepicker-div .ui-datepicker-year :selected").val();

                    //if (formatDate == "mm/yy" || formatDate == "yy/mm") {
                    //    dateText = new Date(year, month, 1);
                    //    $(this).datepicker('setDate', new Date(year, month, 1, 2));
                    //    ngModelCtrl.$setViewValue(dateText);
                    //    scope.$apply();

                    //}
                    ngModelCtrl.$setValidity('invalid', true);
                },
                onChangeMonthYear: function (year, month, inst) {
                    setTimeout(function () {
                        $($('.ui-datepicker-calendar').find('a')[0]).addClass('ui-state-active');
                    }, 100);
                },
                beforeShow: function () {

                    setTimeout(function () {
                        if ($('.modal').css('display') == 'block') {
                            $('.modal').css('display', '');
                            setTimeout(function () {
                                $('.modal').css('display', 'block');
                            }, 50);
                        }
                    }, 1)
                    //if (formatDate == "mm/yy" || formatDate == "yy/mm") {
                    //    if ($('#hide-days').length == 0)
                    //        $('head').append('<style id="hide-days">.ui-datepicker-calendar { display: none; } </style>');
                    //    if ((selDate = $(this).val()).length > 0) {
                    //        iYear = selDate.substring(selDate.length - 4, selDate.length);
                    //        iMonth = parseInt(selDate.substring(0, selDate.length - 5)) - 1;
                    //        $(this).datepicker('option', 'defaultDate', new Date(iYear, iMonth, 1));
                    //        $(this).datepicker('setDate', new Date(iYear, iMonth, 1));
                    //    }
                    //}
                    //else
                    //    $('#hide-days').remove();

                },
                beforeShowDay: function (date) {
                    var day = date.getDay();
                    return [(allowdDays.indexOf(day.toString()) > -1), ''];
                }
            });

            //element.on('keypress', function (e) {
            //    return false;
            //});

            ngModelCtrl.$render();
        });

        if (attrs.yearRange) $(element).datepicker('option', "yearRange", attrs.yearRange);
        if (attrs.monthRange) $(element).datepicker('option', "monthRange", attrs.monthRange);

        attrs.$observe('disabled', function (value) {
            if (value == undefined) return;
            $(element).datepicker("option", "disabled", value);
        });

        ngModelCtrl.$render = function () {
            var date = ngModelCtrl.$viewValue;
            if (date == "") date = null;
            else if (typeof (date) == "string") {
                date = new Date(date);
                if (date.getFullYear() == 1) {
                    date = null;
                }
            }
            if (angular.isDefined(date) && date !== null && !angular.isDate(date)) {
                throw new Error('ng-Model value must be a Date object - currently it is a ' + typeof date + ' - use ui-date-format to convert it from a string');
            }
            element.datepicker("setDate", date);
        };
        element.on('blur', function (e) {
            if (!$(this).datepicker("widget").is(":visible")) {
                var d;
                if (this.value == "") return;
                var dateParts = this.value.split('/');
                if (formatDate == "mm/yy")
                    d = new Date(dateParts[1], dateParts[0] - 1, 1);
                else if (formatDate == "yy/mm")
                    d = new Date(dateParts[0], dateParts[1] - 1, 1);
                else
                    try {
                        d = $.datepicker.parseDate(formatDate, this.value);
                    }
                    catch (e) {
                        d = "";
                    }
                if (!angular.isDate(d) || d.toString() == "Invalid Date")
                    ngModelCtrl.$setViewValue("");
                else {
                    d.setHours(2);
                    ngModelCtrl.$setViewValue(d);
                }
                ngModelCtrl.$render();
                scope.$apply();
            }
        })
    }
}]);