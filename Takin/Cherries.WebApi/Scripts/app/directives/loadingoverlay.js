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

