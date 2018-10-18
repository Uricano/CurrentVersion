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